using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Image gaugeImage;

    public void SetLabel(string text)
    {
        if (labelText != null) labelText.text = text;
    }

    public void SetCost(string text)
    {
        if (costText != null) costText.text = text;
    }

    public void SetGauge(float t)
    {
        if (gaugeImage == null) return;
        gaugeImage.fillAmount = Mathf.Clamp01(t);
        gaugeImage.gameObject.SetActive(t > 0f);
    }
}
