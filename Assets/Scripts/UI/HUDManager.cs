using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI moneyText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged += UpdateMoneyUI;

        UpdateMoneyUI(0);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
    }

    void UpdateMoneyUI(int amount)
    {
        if (moneyText != null)
            moneyText.text = $"${amount}";
    }
}
