using UnityEngine;
using TMPro;

public class ZoneUI : MonoBehaviour
{
    [SerializeField] TextMeshPro labelText;
    [SerializeField] TextMeshPro costText;

    public void SetLabel(string text)
    {
        if (labelText != null) labelText.text = text;
    }

    public void SetCost(string text)
    {
        if (costText != null) costText.text = text;
    }
}
