using UnityEngine;
using System.Collections;

public enum UpgradeType { Tool, Worker, DeskWorker, Jail }

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
        zoneUI?.SetGauge(0f);
    }

    IEnumerator TryUpgradeLoop()
    {
        const float holdTime = 1f;

        while (player != null)
        {
            if (UpgradeManager.Instance == null || GameManager.Instance == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 돈 부족하면 게이지 없이 대기
            int cost = GetCurrentCost();
            if (GameManager.Instance.Money < cost)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // 돈 충분: 게이지 0→1 채우기
            float elapsed = 0f;
            while (elapsed < holdTime && player != null)
            {
                elapsed += Time.deltaTime;
                zoneUI?.SetGauge(elapsed / holdTime);
                yield return null;
            }

            zoneUI?.SetGauge(0f);
            if (player == null) break;

            bool success = upgradeType switch
            {
                UpgradeType.Tool       => UpgradeManager.Instance.TryUpgradeTool(inventory),
                UpgradeType.Worker     => UpgradeManager.Instance.TryHireWorker(),
                UpgradeType.DeskWorker => UpgradeManager.Instance.TryHireDeskWorker(),
                UpgradeType.Jail       => UpgradeManager.Instance.TryUpgradeJail(),
                _                      => false
            };

            if (success)
            {
                SoundManager.Instance?.Play(SFXType.UpgradeComplete);
                inventory?.RemoveMoneyVisuals(cost);
                RefreshUI();
                if (IsMaxed()) yield break;
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    int GetCurrentCost()
    {
        if (UpgradeManager.Instance == null) return 0;
        return upgradeType switch
        {
            UpgradeType.Tool       => UpgradeManager.Instance.ToolUpgradeCost,
            UpgradeType.Worker     => UpgradeManager.Instance.WorkerHireCost,
            UpgradeType.DeskWorker => UpgradeManager.Instance.DeskWorkerHireCost,
            UpgradeType.Jail       => UpgradeManager.Instance.JailUpgradeCost,
            _                      => 0
        };
    }

    bool IsMaxed()
    {
        if (UpgradeManager.Instance == null) return false;
        return upgradeType switch
        {
            UpgradeType.Tool       => UpgradeManager.Instance.ToolLevel >= UpgradeManager.TOOL_MAX_LEVEL,
            UpgradeType.Worker     => UpgradeManager.Instance.HasWorker,
            UpgradeType.DeskWorker => UpgradeManager.Instance.HasDeskWorker,
            UpgradeType.Jail       => JailManager.Instance?.IsMaxUpgraded ?? false,
            _                      => false
        };
    }

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
                    zoneUI.SetLabel($"Tool \nLv{lv} → Lv{lv + 1}");
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

            case UpgradeType.Jail:
                int jailCount = JailManager.Instance?.Count ?? 0;
                int jailCap   = JailManager.Instance?.Capacity ?? 0;
                if (JailManager.Instance?.IsMaxUpgraded ?? false)
                {
                    zoneUI.SetLabel($"Jail MAX ({jailCount}/{jailCap})");
                    zoneUI.SetCost("");
                }
                else
                {
                    zoneUI.SetLabel($"Expand Jail ({jailCount}/{jailCap})");
                    zoneUI.SetCost($"${UpgradeManager.Instance.JailUpgradeCost}");
                }
                break;
        }
    }
}
