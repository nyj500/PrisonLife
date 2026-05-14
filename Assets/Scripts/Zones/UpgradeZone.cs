using UnityEngine;
using System.Collections;

public enum UpgradeType { Tool, Worker, DeskWorker }

public class UpgradeZone : BaseZone
{
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] ZoneUI zoneUI;

    Coroutine upgradeRoutine;

    protected override void OnAwake()
    {
        RefreshUI();
    }

    protected override void OnPlayerEnter()
    {
        if (upgradeRoutine != null) StopCoroutine(upgradeRoutine);
        upgradeRoutine = StartCoroutine(TryUpgrade());
    }

    protected override void OnPlayerExit()
    {
        if (upgradeRoutine != null) { StopCoroutine(upgradeRoutine); upgradeRoutine = null; }
    }

    IEnumerator TryUpgrade()
    {
        yield return new WaitForSeconds(0.5f);
        if (player == null) yield break;

        bool success = false;
        switch (upgradeType)
        {
            case UpgradeType.Tool:
                success = UpgradeManager.Instance.TryUpgradeTool(inventory);
                break;
            case UpgradeType.Worker:
                success = UpgradeManager.Instance.TryHireWorker();
                break;
            case UpgradeType.DeskWorker:
                success = UpgradeManager.Instance.TryHireDeskWorker();
                break;
        }

        if (success) RefreshUI();
    }

    void RefreshUI()
    {
        if (zoneUI == null) return;
        switch (upgradeType)
        {
            case UpgradeType.Tool:
                int level = UpgradeManager.Instance != null ? UpgradeManager.Instance.ToolLevel : 1;
                zoneUI.SetLabel($"Tool Lv{level}");
                break;
            case UpgradeType.Worker:
                zoneUI.SetLabel("Hire Worker");
                break;
            case UpgradeType.DeskWorker:
                zoneUI.SetLabel("Hire Desk Worker");
                break;
        }
    }
}
