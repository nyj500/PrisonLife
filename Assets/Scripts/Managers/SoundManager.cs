using UnityEngine;

public enum SFXType
{
    Mine,
    MoneyPickup,
    HandcuffPickup,
    OreDeposit,
    HandcuffDeposit,
    UpgradeComplete,
    ManufactureComplete,
    PrisonerSatisfied
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] AudioSource source;

    [Header("Clips")]
    [SerializeField] AudioClip mineClip;
    [SerializeField] AudioClip moneyPickupClip;
    [SerializeField] AudioClip handcuffPickupClip;
    [SerializeField] AudioClip oreDepositClip;
    [SerializeField] AudioClip handcuffDepositClip;
    [SerializeField] AudioClip upgradeCompleteClip;
    [SerializeField] AudioClip manufactureCompleteClip;
    [SerializeField] AudioClip prisonerSatisfiedClip;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Play(SFXType type)
    {
        AudioClip clip = type switch
        {
            SFXType.Mine                => mineClip,
            SFXType.MoneyPickup         => moneyPickupClip,
            SFXType.HandcuffPickup      => handcuffPickupClip,
            SFXType.OreDeposit          => oreDepositClip,
            SFXType.HandcuffDeposit     => handcuffDepositClip,
            SFXType.UpgradeComplete     => upgradeCompleteClip,
            SFXType.ManufactureComplete => manufactureCompleteClip,
            SFXType.PrisonerSatisfied   => prisonerSatisfiedClip,
            _                           => null
        };
        if (clip != null) source.PlayOneShot(clip);
    }
}
