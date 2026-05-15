using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeskZone : BaseZone
{
    [Header("References")]
    [SerializeField] MoneyStackZone moneyStackZone;
    [SerializeField] GameObject prisonerPrefab;
    [SerializeField] Transform[] queueSlots;
    [SerializeField] Transform jailExit;
    [SerializeField] Transform prisonerPoolParent;

    [Header("Tuning")]
    [SerializeField] float consumeInterval = 0.3f;
    [SerializeField] int moneyPerPrisoner = 1;

    readonly List<PrisonerNPC> queue = new List<PrisonerNPC>();
    readonly List<PrisonerNPC> pool  = new List<PrisonerNPC>();
    Coroutine processRoutine;

    void Start()
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            PrisonerNPC npc = CreatePrisonerNPC();
            if (npc == null) return;
            Vector3 spawnPos = queueSlots[i].position;
            spawnPos.y = 1f;
            npc.Initialize(Random.Range(2, 5), spawnPos);
            npc.gameObject.SetActive(true);
            queue.Add(npc);
        }
    }

    protected override void OnPlayerEnter()
    {
        if (processRoutine != null) StopCoroutine(processRoutine);
        processRoutine = StartCoroutine(ProcessLoop());
    }

    protected override void OnPlayerExit()
    {
        if (processRoutine != null) { StopCoroutine(processRoutine); processRoutine = null; }
    }

    IEnumerator ProcessLoop()
    {
        while (player != null && inventory != null)
        {
            if (queue.Count == 0 || inventory.HandcuffCount == 0)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            PrisonerNPC front = queue[0];

            while (!front.IsFullyProcessed)
            {
                if (inventory.HandcuffCount == 0) { yield return new WaitForSeconds(0.2f); continue; }
                GameObject vis = inventory.RemoveTopHandcuff();
                if (vis != null) ItemVisualPool.Instance.ReturnHandcuff(vis);
                front.ConsumeHandcuff();
                yield return new WaitForSeconds(consumeInterval);
            }

            // --- 처리 완료 ---
            queue.RemoveAt(0);

            // 나머지 수감자들 한 칸씩 전진
            for (int i = 0; i < queue.Count && i < queueSlots.Length; i++)
            {
                Vector3 slotPos = queueSlots[i].position;
                slotPos.y = 1f;
                queue[i].MoveToQueuePosition(slotPos);
            }

            moneyStackZone?.AddMoney(SpawnMoneyVisual(), moneyPerPrisoner);

            // 처리된 수감자: jailExit까지 걷고 비활성화 → 풀 반환
            PrisonerNPC leaving = front;
            Vector3 exitPos = jailExit != null
                ? jailExit.position
                : leaving.transform.position + Vector3.right * 8f;
            leaving.LeaveToPool(exitPos, () =>
            {
                leaving.gameObject.SetActive(false);
                pool.Add(leaving);
            });

            // 마지막 슬롯 수감자가 앞 슬롯에 도착하면 새 수감자 생성
            if (queue.Count > 0)
            {
                PrisonerNPC newLast = queue[queue.Count - 1];
                yield return new WaitUntil(() => !newLast.IsWalking);
            }

            int lastSlot = queueSlots.Length - 1;
            PrisonerNPC newNpc = GetOrCreatePrisoner();
            if (newNpc != null)
            {
                Vector3 newPos = queueSlots[lastSlot].position;
                newPos.y = 1f;
                newNpc.Initialize(Random.Range(2, 5), newPos);
                newNpc.gameObject.SetActive(true);
                queue.Add(newNpc);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    PrisonerNPC GetOrCreatePrisoner()
    {
        if (pool.Count > 0)
        {
            var npc = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            return npc;
        }
        return CreatePrisonerNPC();
    }

    PrisonerNPC CreatePrisonerNPC()
    {
        if (prisonerPrefab == null)
        {
            Debug.LogError("[DeskZone] prisonerPrefab이 비어 있습니다. Inspector에서 연결해 주세요.", this);
            return null;
        }
        GameObject go = Instantiate(prisonerPrefab, prisonerPoolParent);
        return go.GetComponent<PrisonerNPC>() ?? go.AddComponent<PrisonerNPC>();
    }

    GameObject SpawnMoneyVisual()
    {
        GameObject go = ItemVisualPool.Instance.GetMoney(transform.position);
        if (go != null && go.GetComponent<MoneyItem>() == null)
            go.AddComponent<MoneyItem>();
        return go;
    }
}
