using UnityEngine;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public int  ToolLevel     { get; private set; } = 1;
    public bool HasWorker     { get; private set; }
    public bool HasDeskWorker { get; private set; }

    public const int ORE_CAP_LVL1   = 10;
    public const int ORE_CAP_LVL2   = 20;
    public const int ORE_CAP_LVL3   = 30;
    public const int TOOL_MAX_LEVEL  = 3;

    [Header("Tool Upgrade — index 0 = Lv1→2, index 1 = Lv2→3")]
    [SerializeField] int[]   toolUpgradeCosts = { 20, 50 };
    [SerializeField] float[] toolMineRange    = { 1.2f, 1.8f, 2.5f };
    [SerializeField] float[] toolMineSpeed    = { 0.5f, 0.35f, 0.2f };

    [Header("Worker NPC (씬에 비활성화로 배치된 3개)")]
    [SerializeField] int         workerHireCost = 50;
    [SerializeField] WorkerNPC[] workerNPCs;

    [Header("Desk Worker NPC")]
    [SerializeField] int           deskWorkerHireCost = 50;
    [SerializeField] DeskWorkerNPC deskWorkerNPC;

    [Header("Jail Capacity Upgrade")]
    [SerializeField] int jailUpgradeCost = 50;

    static readonly int[] oreCaps = { ORE_CAP_LVL1, ORE_CAP_LVL2, ORE_CAP_LVL3 };

    public event Action OnToolUpgraded;

    public float CurrentMineRange   => toolMineRange[ToolLevel - 1];
    public float CurrentMineSpeed   => toolMineSpeed[ToolLevel - 1];
    public int   CurrentOreCap      => oreCaps[ToolLevel - 1];
    public int   ToolUpgradeCost    => ToolLevel < TOOL_MAX_LEVEL ? toolUpgradeCosts[ToolLevel - 1] : 0;
    public int   WorkerHireCost     => workerHireCost;
    public int   DeskWorkerHireCost => deskWorkerHireCost;
    public int   JailUpgradeCost    => jailUpgradeCost;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryUpgradeTool(PlayerInventory inventory)
    {
        if (ToolLevel >= TOOL_MAX_LEVEL) return false;
        if (!GameManager.Instance.TrySpendMoney(ToolUpgradeCost)) return false;
        ToolLevel++;
        inventory.MaxOre = CurrentOreCap;
        OnToolUpgraded?.Invoke();
        return true;
    }

    public bool TryHireWorker()
    {
        if (HasWorker) return false;
        if (!GameManager.Instance.TrySpendMoney(workerHireCost)) return false;
        HasWorker = true;
        SpawnWorkers();
        return true;
    }

    public bool TryHireDeskWorker()
    {
        if (HasDeskWorker) return false;
        if (!GameManager.Instance.TrySpendMoney(deskWorkerHireCost)) return false;
        HasDeskWorker = true;
        deskWorkerNPC?.Activate();
        return true;
    }

    public bool TryUpgradeJail()
    {
        if (!GameManager.Instance.TrySpendMoney(jailUpgradeCost)) return false;
        JailManager.Instance?.UpgradeCapacity();
        return true;
    }

    void SpawnWorkers()
    {
        if (workerNPCs == null) return;
        foreach (var npc in workerNPCs)
            npc?.Activate();
    }
}
