using UnityEngine;
using UnityEngine.EventSystems;

// 터치한 자리에 핸들이 나타나고, 드래그로 이동 방향을 결정하는 조이스틱
// Canvas 직계 자식인 Handle Image 하나만 필요 (배경 이미지 불필요)
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform handle;   // Canvas 직계 자식 Image
    [SerializeField] float radius = 80f;     // 핸들 최대 이동 반경 (px, 기준 해상도 기준)

    Canvas rootCanvas;
    Vector2 touchStartScreenPos;

    public Vector2 Direction { get; private set; }

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (handle != null) handle.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchStartScreenPos = eventData.position;
        if (handle != null) handle.gameObject.SetActive(true);
        PlaceHandle(eventData.position);
        Direction = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - touchStartScreenPos;
        Vector2 clamped = Vector2.ClampMagnitude(delta, radius);
        PlaceHandle(touchStartScreenPos + clamped);
        Direction = clamped / radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (handle != null) handle.gameObject.SetActive(false);
        Direction = Vector2.zero;
    }

    // 스크린 좌표 → Canvas 월드 좌표로 핸들 배치 (부모 무관하게 정확히 위치함)
    void PlaceHandle(Vector2 screenPos)
    {
        if (handle == null || rootCanvas == null) return;
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)rootCanvas.transform, screenPos, cam, out Vector3 worldPos))
        {
            handle.position = worldPos;
        }
    }
}
