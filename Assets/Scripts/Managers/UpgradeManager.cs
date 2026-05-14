using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public int ToolLevel { get; private set; } = 1;
    public bool HasWorker { get; private set; }
    public bool HasDeskWorker { get; private set; }

    // Ore capacity per tool level
    public const int ORE_CAP_LVL1 = 10;
    public const int ORE_CAP_LVL2 = 20;
    public const int ORE_CAP_LVL3 = 30;

    [SerializeField] int toolUpgradeCost1 = 20;
    [SerializeField] int toolUpgradeCost2 = 50;
    [SerializeField] int workerHireCost = 50;
    [SerializeField] int deskWorkerHireCost = 50;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryUpgradeTool(PlayerInventory inventory)
    {
        if (ToolLevel >= 3) return false;
        int cost = ToolLevel == 1 ? toolUpgradeCost1 : toolUpgradeCost2;
        if (!GameManager.Instance.TrySpendMoney(cost)) return false;
        ToolLevel++;
        inventory.MaxOre = ToolLevel == 2 ? ORE_CAP_LVL2 : ORE_CAP_LVL3;
        return true;
    }

    public bool TryHireWorker()
    {
        if (HasWorker) return false;
        if (!GameManager.Instance.TrySpendMoney(workerHireCost)) return false;
        HasWorker = true;
        return true;
    }

    public bool TryHireDeskWorker()
    {
        if (HasDeskWorker) return false;
        if (!GameManager.Instance.TrySpendMoney(deskWorkerHireCost)) return false;
        HasDeskWorker = true;
        return true;
    }
}
