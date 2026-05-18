using UnityEngine;
using UnityEngine.EventSystems;

// 터치한 자리에 배경+핸들이 나타나고, 드래그로 이동 방향을 결정하는 조이스틱
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform background; // 배경 원 이미지
    [SerializeField] RectTransform handle;     // 핸들 이미지
    [SerializeField] float radius = 80f;       // 핸들 최대 이동 반경 (px, 기준 해상도 기준)

    Canvas rootCanvas;
    Vector2 touchStartScreenPos;

    public Vector2 Direction { get; private set; }

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (background != null) background.gameObject.SetActive(false);
        if (handle != null)     handle.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchStartScreenPos = eventData.position;
        PlaceAt(background, eventData.position);
        PlaceAt(handle, eventData.position);
        if (background != null) background.gameObject.SetActive(true);
        if (handle != null)     handle.gameObject.SetActive(true);
        Direction = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta   = eventData.position - touchStartScreenPos;
        Vector2 clamped = Vector2.ClampMagnitude(delta, radius);
        PlaceAt(handle, touchStartScreenPos + clamped);
        Direction = clamped / radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (background != null) background.gameObject.SetActive(false);
        if (handle != null)     handle.gameObject.SetActive(false);
        Direction = Vector2.zero;
    }

    void PlaceAt(RectTransform rt, Vector2 screenPos)
    {
        if (rt == null || rootCanvas == null) return;
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)rootCanvas.transform, screenPos, cam, out Vector3 worldPos))
            rt.position = worldPos;
    }
}
