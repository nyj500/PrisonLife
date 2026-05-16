using UnityEngine;
using System.Collections;

public enum UpgradeType { Tool, Worker, DeskWorker }

public class UpgradeZone : BaseZone
{
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] ZoneUI zoneUI;

    Coroutine upgradeRoutine;

    void Start() => RefreshUI();

    protected override void OnPlayerEnter()
    {
        if (IsMaxed()) return;
        if (upgradeRoutine != null) StopCoroutine(upgradeRoutine);
        upgradeRoutine = StartCoroutine(TryUpgradeLoop());
    }

    protected override void OnPlayerExit()
    {
        if (upgradeRoutine != null) { StopCoroutine(upgradeRoutine); upgradeRoutine = null; }
    }

    IEnumerator TryUpgradeLoop()
    {
        while (player != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (player == null) yield break;

            int cost = GetCurrentCost();
            bool success = upgradeType switch
            {
                UpgradeType.Tool       => UpgradeManager.Instance.TryUpgradeTool(inventory),
                UpgradeType.Worker     => UpgradeManager.Instance.TryHireWorker(),
                UpgradeType.DeskWorker => UpgradeManager.Instance.TryHireDeskWorker(),
                _                      => false
            };

            if (success)
            {
                inventory?.RemoveMoneyVisuals(cost);
                RefreshUI();
                if (IsMaxed()) yield break;
            }
        }
    }

    int GetCurrentCost() => upgradeType switch
    {
        UpgradeType.Tool       => UpgradeManager.Instance.ToolUpgradeCost,
        UpgradeType.Worker     => UpgradeManager.Instance.WorkerHireCost,
        UpgradeType.DeskWorker => UpgradeManager.Instance.DeskWorkerHireCost,
        _                      => 0
    };

    bool IsMaxed() => upgradeType switch
    {
        UpgradeType.Tool       => UpgradeManager.Instance.ToolLevel >= UpgradeManager.TOOL_MAX_LEVEL,
        UpgradeType.Worker     => UpgradeManager.Instance.HasWorker,
        UpgradeType.DeskWorker => UpgradeManager.Instance.HasDeskWorker,
        _                      => false
    };

    void RefreshUI()
    {
        if (zoneUI == null || UpgradeManager.Instance == null) return;

        switch (upgradeType)
        {
            case UpgradeType.Tool:
                int lv = UpgradeManager.Instance.ToolLevel;
                if (lv >= UpgradeManager.TOOL_MAX_LEVEL)
                {
                    zoneUI.SetLabel($"Tool Lv{lv} MAX");
                    zoneUI.SetCost("");
                }
                else
                {
                    zoneUI.SetLabel($"Tool Lv{lv} → Lv{lv + 1}");
                    zoneUI.SetCost($"${UpgradeManager.Instance.ToolUpgradeCost}");
                }
                break;

            case UpgradeType.Worker:
                if (UpgradeManager.Instance.HasWorker)
                {
                    zoneUI.SetLabel("Worker Active");
                    zoneUI.SetCost("");
                }
                else
                {
                    zoneUI.SetLabel("Hire Worker");
                    zoneUI.SetCost($"${UpgradeManager.Instance.WorkerHireCost}");
                }
                break;

            case UpgradeType.DeskWorker:
                if (UpgradeManager.Instance.HasDeskWorker)
                {
                    zoneUI.SetLabel("Desk Worker Active");
                    zoneUI.SetCost("");
                }
                else
                {
                    zoneUI.SetLabel("Hire Desk Worker");
                    zoneUI.SetCost($"${UpgradeManager.Instance.DeskWorkerHireCost}");
                }
                break;
        }
    }
}
