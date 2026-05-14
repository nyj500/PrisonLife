using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeskZone : BaseZone
{
    [Header("References")]
    [SerializeField] MoneyStackZone moneyStackZone;
    [SerializeField] GameObject prisonerPrefab;
    [SerializeField] GameObject moneyPrefab;
    [SerializeField] Transform[] queueSlots;
    [SerializeField] Transform jailExit;          // 수감자 퇴장 방향 기준점

    [Header("Tuning")]
    [SerializeField] float consumeInterval = 0.3f;
    [SerializeField] int moneyPerPrisoner = 1;

    // queueSlots 길이만큼 수감자 고정 풀로 운영
    readonly List<PrisonerNPC> queue = new List<PrisonerNPC>();
    Coroutine processRoutine;

    void Start()
    {
        // 비활성 상태로 생성 → 위치 초기화 → 활성화 (0,0,0에서 한 프레임 렌더링되는 현상 방지)
        for (int i = 0; i < queueSlots.Length; i++)
        {
            PrisonerNPC npc = CreatePrisonerNPC();
            if (npc == null) return;
            // npc.gameObject.SetActive(false);
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

            // 수갑 하나씩 소비 (수갑 부족하면 대기)
            while (!front.IsFullyProcessed)
            {
                if (inventory.HandcuffCount == 0) { yield return new WaitForSeconds(0.2f); continue; }
                GameObject vis = inventory.RemoveTopHandcuff();
                if (vis != null) Destroy(vis);
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

            // MoneyStackZone에 돈 오브젝트 쌓기 (카운터는 플레이어가 수령할 때 증가)
            moneyStackZone?.AddMoney(SpawnMoneyVisual(), moneyPerPrisoner);

            // 퇴장 후 마지막 슬롯으로 재사용
            int lastSlot = queueSlots.Length - 1;
            Vector3 exitDir = jailExit != null
                ? (jailExit.position - front.transform.position).normalized
                : Vector3.right;

            Vector3 recyclePos = queueSlots[lastSlot].position;
            recyclePos.y = 1f;
            front.LeaveAndRecycle(exitDir, recyclePos, Random.Range(2, 5), () =>
            {
                queue.Add(front);
                front.MoveToQueuePosition(queueSlots[lastSlot].position);
            });

            yield return new WaitForSeconds(0.3f);
        }
    }

    PrisonerNPC CreatePrisonerNPC()
    {
        if (prisonerPrefab == null)
        {
            Debug.LogError("[DeskZone] prisonerPrefab이 비어 있습니다. Inspector에서 연결해 주세요.", this);
            return null;
        }
        GameObject go = Instantiate(prisonerPrefab);
        return go.GetComponent<PrisonerNPC>() ?? go.AddComponent<PrisonerNPC>();
    }

    GameObject SpawnMoneyVisual()
    {
        GameObject go = moneyPrefab != null
            ? Instantiate(moneyPrefab)
            : CreatePrimitive(PrimitiveType.Cube, new Vector3(0.18f, 0.08f, 0.25f), new Color(0.1f, 0.8f, 0.2f));

        if (go.GetComponent<MoneyItem>() == null)
            go.AddComponent<MoneyItem>(); // Value는 MoneyStackZone.AddMoney에서 설정
        return go;
    }
}
