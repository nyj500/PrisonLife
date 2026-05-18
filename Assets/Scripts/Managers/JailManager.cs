using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JailManager : MonoBehaviour
{
    public static JailManager Instance { get; private set; }

    [Header("Layout")]
    [SerializeField] Transform jailOrigin;       // 수감자 그리드 시작 위치 (좌하단)
    [SerializeField] Transform jailEntrance;     // 감옥 입구 — ㄱ자 이동의 꺾이는 지점
    [SerializeField] int   initialCapacity = 30;
    [SerializeField] int   upgradeAmount   = 15;
    [SerializeField] int   cols            = 6;
    [SerializeField] float spacingX        = 1.2f;
    [SerializeField] float spacingZ        = 1.2f;

    [Header("Upgrade Visual")]
    [SerializeField] GameObject[] jailBlocks;           // 씬에 미리 배치(비활성화)된 감옥 증축 오브젝트 — 업그레이드 순서대로
    [SerializeField] GameObject[] wallsToDeactivate;    // 업그레이드 순서대로 비활성화할 벽

    int upgradeCount;

    public const int MAX_UPGRADES    = 2;
    public bool      IsMaxUpgraded   => upgradeCount >= MAX_UPGRADES;

    readonly List<Vector3>     slots            = new List<Vector3>();
    readonly List<PrisonerNPC> prisoners        = new List<PrisonerNPC>();
    readonly Stack<int>        availableIndices = new Stack<int>();

    // 한 명씩 순서대로 입장시키기 위한 큐
    readonly Queue<(PrisonerNPC npc, Vector3[] path)> entryQueue = new Queue<(PrisonerNPC, Vector3[])>();
    bool isProcessingEntry;

    public bool IsFull   => availableIndices.Count == 0;
    public int  Capacity => slots.Count;
    public int  Count    => prisoners.Count;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GenerateSlots(initialCapacity, animate: false);
    }

    // fromExitPos: DeskZone의 jailExit 위치 — 거기서 출발해 입구 → 셀 순으로 이동
    public void AcceptPrisoner(PrisonerNPC npc, Vector3 fromExitPos)
    {
        if (IsFull || npc == null) return;
        Vector3 cell = slots[availableIndices.Pop()];
        prisoners.Add(npc);
        npc.transform.SetParent(transform);

        Vector3[] path = jailEntrance != null
            ? new[] { fromExitPos, jailEntrance.position, cell }
            : new[] { fromExitPos, cell };

        entryQueue.Enqueue((npc, path));
        if (!isProcessingEntry)
            StartCoroutine(ProcessEntryQueue());
    }

    IEnumerator ProcessEntryQueue()
    {
        isProcessingEntry = true;
        while (entryQueue.Count > 0)
        {
            var (npc, path) = entryQueue.Dequeue();
            bool arrived = false;
            npc.WalkPath(path, () => arrived = true);
            yield return new WaitUntil(() => arrived);
        }
        isProcessingEntry = false;
    }

    public void UpgradeCapacity()
    {
        if (upgradeCount >= MAX_UPGRADES) return;
        GenerateSlots(upgradeAmount, animate: true);

        if (wallsToDeactivate != null && upgradeCount < wallsToDeactivate.Length)
            wallsToDeactivate[upgradeCount]?.SetActive(false);

        upgradeCount++;
    }

    void GenerateSlots(int count, bool animate)
    {
        Vector3 origin = jailOrigin != null ? jailOrigin.position : transform.position;
        int startIndex = slots.Count;

        Vector3 firstNew = origin, lastNew = origin;
        bool isFirst = true;
        for (int i = 0; i < count; i++)
        {
            int idx = startIndex + i;
            int col = idx % cols;
            int row = idx / cols;
            Vector3 pos = origin + new Vector3(col * spacingX, 0f, row * spacingZ);
            slots.Add(pos);
            availableIndices.Push(idx); // 오름차순 push → 높은 인덱스(마지막 슬롯)가 pop 우선
            if (isFirst) { firstNew = pos; isFirst = false; }
            lastNew = pos;
        }

        if (animate && jailBlocks != null && upgradeCount < jailBlocks.Length)
        {
            GameObject block = jailBlocks[upgradeCount];
            if (block != null)
            {
                block.SetActive(true);
                StartCoroutine(PopIn(block.transform));
            }
        }
    }

    IEnumerator PopIn(Transform t)
    {
        Vector3 target = t.localScale.sqrMagnitude < 0.0001f ? Vector3.one : t.localScale;
        t.localScale = Vector3.zero;
        float duration = 0.7f;
        float elapsed  = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            // 감쇠 스프링: 살짝 튀어오르는 "띠용" 연출
            float decay  = Mathf.Exp(-5f * p);
            float spring = 1f - decay * Mathf.Cos(Mathf.PI * 3.5f * p);
            t.localScale = target * Mathf.Max(0f, spring);
            yield return null;
        }
        t.localScale = target;
    }
}
