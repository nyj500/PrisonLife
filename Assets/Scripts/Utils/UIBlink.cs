using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIBlink : MonoBehaviour
{
    [SerializeField] float fadeInDuration  = 0.4f;
    [SerializeField] float fadeOutDuration = 0.4f;
    [SerializeField] float holdVisible     = 0.1f; // 완전히 보이는 구간 유지 시간
    [SerializeField] float holdInvisible   = 0.1f; // 완전히 사라지는 구간 유지 시간

    CanvasGroup canvasGroup;

    void Awake() => canvasGroup = GetComponent<CanvasGroup>();

    void OnEnable()
    {
        canvasGroup.alpha = 0f;
        StartCoroutine(BlinkLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 1f;
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdVisible);
            yield return Fade(1f, 0f, fadeOutDuration);
            yield return new WaitForSeconds(holdInvisible);
        }
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
