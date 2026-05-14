using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] VirtualJoystick joystick;
    [SerializeField] Camera gameCamera;
    [SerializeField] GameObject toolObject;  // 곡괭이(ArmPivot) — MineZone 진입 시에만 활성화

    Rigidbody rb;
    bool autoMoving;
    Vector3 autoTarget;
    float autoStopDist = 0.5f;
    System.Action onReached;

    public bool IsMoving { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (gameCamera == null) gameCamera = Camera.main;
        if (toolObject != null) toolObject.SetActive(false); // 기본 비활성
    }

    void FixedUpdate()
    {
        if (autoMoving)
            HandleAutoMove();
        else
            HandleJoystickMove();
    }

    void HandleAutoMove()
    {
        Vector3 dir = autoTarget - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (dist < autoStopDist)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            IsMoving = false;
            var callback = onReached;
            ClearAutoMove();
            callback?.Invoke();
        }
        else
        {
            Vector3 norm = dir.normalized;
            rb.velocity = new Vector3(norm.x * moveSpeed, rb.velocity.y, norm.z * moveSpeed);
            RotateToward(norm);
            IsMoving = true;
        }
    }

    void HandleJoystickMove()
    {
        Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;

        // 조이스틱 입력을 카메라 기준 월드 방향으로 변환
        // 카메라 Y 회전(-45도 등)에 관계없이 화면 위 = 캐릭터 전진으로 동작
        Vector3 camForward = gameCamera.transform.forward;
        Vector3 camRight   = gameCamera.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y   = 0f; camRight.Normalize();

        Vector3 dir = camForward * input.y + camRight * input.x;
        rb.velocity = new Vector3(dir.x * moveSpeed, rb.velocity.y, dir.z * moveSpeed);

        if (dir.sqrMagnitude > 0.01f)
        {
            RotateToward(dir);
            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }
    }

    void RotateToward(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 12f * Time.fixedDeltaTime));
    }

    public void SetAutoMove(Vector3 target, System.Action callback, float stopDist = 0.5f)
    {
        autoMoving = true;
        autoTarget = target;
        autoStopDist = stopDist;
        onReached = callback;
    }

    public void ClearAutoMove()
    {
        autoMoving = false;
        onReached = null;
    }

    public void SetToolActive(bool active)
    {
        if (toolObject != null) toolObject.SetActive(active);
    }

    public void SetJoystick(VirtualJoystick j) => joystick = j;
}
