using UnityEngine;
using TMPro;

public class ZoneUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] TextMeshProUGUI costText;

    public void SetLabel(string text)
    {
        if (labelText != null) labelText.text = text;
    }

    public void SetCost(string text)
    {
        if (costText != null) costText.text = text;
    }
}
