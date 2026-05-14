using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 12f, -9f);
    [SerializeField] float smoothSpeed = 8f;

    // Camera Y stays fixed; only X/Z tracks the player
    float fixedY;

    void Start()
    {
        fixedY = transform.position.y;
        if (target != null)
            transform.position = new Vector3(target.position.x + offset.x, fixedY, target.position.z + offset.z);
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(target.position.x + offset.x, fixedY, target.position.z + offset.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform t) => target = t;
}
