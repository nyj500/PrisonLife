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

    [Header("Desk Handcuff Stack")]
    [SerializeField] Transform deskStackBase;
    [SerializeField] float deskItemSpacing = 0.15f;
    [SerializeField] float depositInterval  = 0.2f;

    [Header("Tuning")]
    [SerializeField] float consumeInterval = 0.3f;
    [SerializeField] int moneyPerPrisoner  = 1;

    readonly List<PrisonerNPC> queue         = new List<PrisonerNPC>();
    readonly List<PrisonerNPC> pool          = new List<PrisonerNPC>();
    readonly List<GameObject>  deskHandcuffs = new List<GameObject>();

    Coroutine depositRoutine;

    // ── 초기화 ────────────────────────────────────────

    protected override void OnAwake() => StartCoroutine(ProcessLoop());

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

    // ── 데스크 수갑 스택 시각 업데이트 ──────────────────

    void LateUpdate()
    {
        if (deskStackBase == null) return;
        for (int i = deskHandcuffs.Count - 1; i >= 0; i--)
        {
            if (deskHandcuffs[i] == null) { deskHandcuffs.RemoveAt(i); continue; }
            Vector3 target = deskStackBase.position + Vector3.up * (i * deskItemSpacing);
            deskHandcuffs[i].transform.position = Vector3.Lerp(
                deskHandcuffs[i].transform.position, target, 12f * Time.deltaTime);
        }
    }

    Vector3 DeskStackTopPosition =>
        deskStackBase != null
            ? deskStackBase.position + Vector3.up * (deskHandcuffs.Count * deskItemSpacing)
            : transform.position;

    // ── 플레이어 진입/퇴장 ────────────────────────────

    protected override void OnPlayerEnter()
    {
        if (depositRoutine != null) StopCoroutine(depositRoutine);
        depositRoutine = StartCoroutine(DepositLoop());
    }

    protected override void OnPlayerExit()
    {
        if (depositRoutine != null) { StopCoroutine(depositRoutine); depositRoutine = null; }
    }

    // ── 수갑 납품 (플레이어 → 데스크) ────────────────────

    IEnumerator DepositLoop()
    {
        while (player != null && inventory != null)
        {
            if (inventory.HandcuffCount == 0) { yield return new WaitForSeconds(0.2f); continue; }

            GameObject vis = inventory.RemoveTopHandcuff();
            if (vis != null)
            {
                vis.transform.SetPositionAndRotation(DeskStackTopPosition + Vector3.up * 1.5f, Quaternion.identity);
                deskHandcuffs.Add(vis);
            }

            yield return new WaitForSeconds(depositInterval);
        }
    }

    // ── 수감자 처리 (독립 실행) ───────────────────────

    IEnumerator ProcessLoop()
    {
        while (true)
        {
            if (queue.Count == 0) { yield return new WaitForSeconds(0.2f); continue; }

            PrisonerNPC front = queue[0];

            // 수감자가 데스크에 도착할 때까지 대기
            yield return new WaitUntil(() => !front.IsWalking);

            // 데스크에 수갑이 충분해질 때까지 대기
            yield return new WaitUntil(() => deskHandcuffs.Count >= front.RequiredHandcuffs);

            // 수갑 소비
            int needed = front.RequiredHandcuffs;
            for (int i = 0; i < needed; i++)
            {
                int last = deskHandcuffs.Count - 1;
                GameObject vis = deskHandcuffs[last];
                deskHandcuffs.RemoveAt(last);
                front.ConsumeHandcuff();
                if (vis != null) ItemVisualPool.Instance.ReturnHandcuff(vis);
                yield return new WaitForSeconds(consumeInterval);
            }

            // --- 처리 완료 ---
            queue.RemoveAt(0);

            for (int i = 0; i < queue.Count && i < queueSlots.Length; i++)
            {
                Vector3 slotPos = queueSlots[i].position;
                slotPos.y = 1f;
                queue[i].MoveToQueuePosition(slotPos);
            }

            moneyStackZone?.AddMoney(SpawnMoneyVisual(), moneyPerPrisoner);

            PrisonerNPC leaving = front;
            Vector3 exitPos = jailExit != null
                ? jailExit.position
                : leaving.transform.position + Vector3.right * 8f;
            leaving.LeaveToPool(exitPos, () =>
            {
                leaving.gameObject.SetActive(false);
                pool.Add(leaving);
            });

            // 앞줄 이동과 동시에 새 수감자가 뒤에서 걸어오도록 즉시 생성
            int lastSlot = queueSlots.Length - 1;
            Vector3 targetPos = queueSlots[lastSlot].position;
            targetPos.y = 1f;

            Vector3 entryPos = lastSlot > 0
                ? targetPos + (queueSlots[lastSlot].position - queueSlots[lastSlot - 1].position)
                : targetPos + Vector3.back * 2f;
            entryPos.y = 1f;

            PrisonerNPC newNpc = GetOrCreatePrisoner();
            if (newNpc != null)
            {
                newNpc.Initialize(Random.Range(2, 5), entryPos);
                newNpc.gameObject.SetActive(true);
                newNpc.MoveToQueuePosition(targetPos);
                queue.Add(newNpc);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    // ── 풀 관리 ──────────────────────────────────────

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
