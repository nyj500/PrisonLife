using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int MineHash = Animator.StringToHash("Mine");
    static readonly int PickupHash = Animator.StringToHash("Pickup");

    Animator anim;
    PlayerController controller;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (anim == null || controller == null) return;
        anim.SetFloat(SpeedHash, controller.IsMoving ? 1f : 0f);
    }

    public void TriggerMine() => anim?.SetTrigger(MineHash);
    public void TriggerPickup() => anim?.SetTrigger(PickupHash);
}
