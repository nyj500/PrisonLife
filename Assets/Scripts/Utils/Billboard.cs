using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] float distance  = 3f;    // 카메라와의 거리
    [SerializeField] float yOffset   = 0.5f;  // 화면 중심에서 위로 오프셋 (월드 Y)

    Camera cam;

    void Awake() => cam = Camera.main;

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        transform.position = cam.transform.position
            + cam.transform.forward * distance
            + Vector3.up * yOffset;

        transform.rotation = cam.transform.rotation;
    }
}
