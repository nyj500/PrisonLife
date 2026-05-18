using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    static readonly int SpeedHash     = Animator.StringToHash("Speed");
    static readonly int MineHash      = Animator.StringToHash("Mine");
    static readonly int MineLevelHash = Animator.StringToHash("MineLevel");
    static readonly int PickupHash    = Animator.StringToHash("Pickup");

    [SerializeField] GameObject[] tools; // index 0 = Lv1, 1 = Lv2, 2 = Lv3

    Animator         anim;
    PlayerController controller;
    bool             toolActive;

    void Awake()
    {
        anim       = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
    }

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

    void Update()
    {
        if (anim == null || controller == null) return;
        anim.SetFloat(SpeedHash, controller.IsMoving ? 1f : 0f);
    }

    // MineZone 진입/퇴장 시 PlayerController가 호출
    public void SetToolActive(bool active)
    {
        toolActive = active;
        ApplyToolLevel();
    }

    void ApplyToolLevel()
    {
        int level = UpgradeManager.Instance != null ? UpgradeManager.Instance.ToolLevel : 1;

        // toolActive 상태 + 현재 레벨에 맞는 도구만 활성화
        for (int i = 0; i < tools.Length; i++)
            tools[i]?.SetActive(toolActive && i == level - 1);

        anim?.SetInteger(MineLevelHash, level);
    }

    public void TriggerMine()   => anim?.SetTrigger(MineHash);
    public void TriggerPickup() => anim?.SetTrigger(PickupHash);
}
