using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineZone : BaseZone
{
    [Header("Ore Grid (런타임 자동 생성)")]
    [SerializeField] int gridCols = 8;
    [SerializeField] int gridRows = 12;
    [SerializeField] float oreSpacing = 1f;
    [SerializeField] GameObject orePrefab;
    [SerializeField] Transform oreParent;

    [Header("채굴 튜닝")]
    [SerializeField] float mineAnimSeconds = 0.5f;
    [SerializeField] float harvestRange = 1.2f;  // 이 범위 안에 광석이 있어야 채굴 가능

    readonly List<OreItem> ores          = new List<OreItem>();
    readonly List<OreItem> inRangeBuffer = new List<OreItem>();
    Coroutine mineLoop;

    // ── Grid 생성 ──────────────────────────────────────

    protected override void OnAwake() => GenerateOreGrid();

    void GenerateOreGrid()
    {
        Collider col = GetComponent<Collider>();
        Bounds bounds = col != null
            ? col.bounds
            : new Bounds(transform.position, new Vector3(gridCols, 1f, gridRows));

        float startX = bounds.min.x + oreSpacing * 0.5f;
        float startZ = bounds.min.z + oreSpacing * 0.5f;
        float oreY   = bounds.max.y + 0.25f;

        Transform parent = oreParent != null
            ? oreParent
            : (transform.parent != null ? transform.parent : transform);

        for (int row = 0; row < gridRows; row++)
        {
            for (int col2 = 0; col2 < gridCols; col2++)
            {
                Vector3 pos = new Vector3(startX + col2 * oreSpacing, oreY, startZ + row * oreSpacing);

                GameObject go = orePrefab != null
                    ? Instantiate(orePrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent)
                    : CreateSceneOre(pos, parent);

                OreItem item = go.GetComponent<OreItem>() ?? go.AddComponent<OreItem>();
                ores.Add(item);
            }
        }
    }

    static GameObject CreateSceneOre(Vector3 pos, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.transform.localScale = new Vector3(
            Random.Range(0.45f, 0.65f),
            Random.Range(0.38f, 0.55f),
            Random.Range(0.45f, 0.65f));
        go.GetComponent<Renderer>().material.color = new Color(0.18f, 0.18f, 0.18f);
        return go;
    }

    // ── 업그레이드 연동 ────────────────────────────────

    void Start()
    {
        ApplyToolLevel();
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnToolUpgraded += ApplyToolLevel;
    }

    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnToolUpgraded -= ApplyToolLevel;
    }

    void ApplyToolLevel()
    {
        if (UpgradeManager.Instance == null) return;
        harvestRange    = UpgradeManager.Instance.CurrentMineRange;
        mineAnimSeconds = UpgradeManager.Instance.CurrentMineSpeed;
    }

    // ── Zone 이벤트 ────────────────────────────────────

    protected override void OnPlayerEnter()
    {
        player.SetToolActive(true);   // 곡괭이 활성화
        if (mineLoop != null) StopCoroutine(mineLoop);
        mineLoop = StartCoroutine(MineLoop());
    }

    protected override void OnPlayerExit()
    {
        player?.SetToolActive(false); // 곡괭이 비활성화
        if (mineLoop != null) { StopCoroutine(mineLoop); mineLoop = null; }
    }

    // ── 채굴 루프 ──────────────────────────────────────
    // 플레이어가 직접 조이스틱으로 이동해서 광석 범위 안에 들어오면 채굴

    IEnumerator MineLoop()
    {
        while (player != null && inventory != null)
        {
            if (!inventory.CanAddOre) { yield return new WaitForSeconds(0.25f); continue; }

            OreItem nearest = GetNearestInRange(harvestRange);
            if (nearest == null) { yield return new WaitForSeconds(0.1f); continue; }

            playerAnim?.TriggerMine();
            yield return new WaitForSeconds(mineAnimSeconds);

            if (player == null || inventory == null) break;

            int toolLevel = UpgradeManager.Instance != null ? UpgradeManager.Instance.ToolLevel : 1;

            if (toolLevel >= 2)
            {
                // Lv2+: 범위 내 모든 광석 채굴
                GetAllInRange(harvestRange, inRangeBuffer);
                foreach (var ore in inRangeBuffer)
                {
                    if (!inventory.CanAddOre) break;
                    if (ore.IsMined) continue;
                    ore.Mine();
                    inventory.AddOre(SpawnInventoryOre(player.transform.position + Vector3.up * 0.8f));
                }
            }
            else
            {
                // Lv1: 가장 가까운 광석 1개
                OreItem target = GetNearestInRange(harvestRange);
                if (target == null) continue;
                target.Mine();
                inventory.AddOre(SpawnInventoryOre(player.transform.position + Vector3.up * 0.8f));
            }
        }
    }

    // ── 헬퍼 ───────────────────────────────────────────

    // WorkerNPC가 호출 — 지정 위치 기준 가장 가까운 채굴 가능 광석 반환
    public OreItem GetNearestOre(Vector3 fromPos, float range)
    {
        OreItem nearest = null;
        float minSqr = range * range;
        foreach (var ore in ores)
        {
            if (ore == null || ore.IsMined) continue;
            float sq = XZSqrDist(fromPos, ore.transform.position);
            if (sq < minSqr) { minSqr = sq; nearest = ore; }
        }
        return nearest;
    }

    OreItem GetNearestInRange(float range)
    {
        OreItem nearest = null;
        float minSqr = range * range;
        foreach (var ore in ores)
        {
            if (ore == null || ore.IsMined) continue;
            float sq = XZSqrDist(player.transform.position, ore.transform.position);
            if (sq < minSqr) { minSqr = sq; nearest = ore; }
        }
        return nearest;
    }

    void GetAllInRange(float range, List<OreItem> result)
    {
        result.Clear();
        float rangeSqr = range * range;
        foreach (var ore in ores)
        {
            if (ore == null || ore.IsMined) continue;
            if (XZSqrDist(player.transform.position, ore.transform.position) <= rangeSqr)
                result.Add(ore);
        }
    }

    static float XZSqrDist(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    GameObject SpawnInventoryOre(Vector3 spawnPos)
    {
        return ItemVisualPool.Instance.GetOre(spawnPos);
    }
}
