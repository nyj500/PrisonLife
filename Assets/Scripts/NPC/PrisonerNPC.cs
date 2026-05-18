using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class PrisonerNPC : MonoBehaviour
{
    [SerializeField] float walkSpeed = 2f;

    [Header("HUD")]
    [SerializeField] Transform      hudRoot;
    [SerializeField] TextMeshProUGUI requirementText;

    public int RequiredHandcuffs { get; private set; }
    int handcuffsConsumed;

    Vector3 queueTarget;
    bool isWalking;

    Camera mainCam;

    public bool IsFullyProcessed => handcuffsConsumed >= RequiredHandcuffs;
    public bool IsWalking        => isWalking;
    public int  Remaining        => Mathf.Max(0, RequiredHandcuffs - handcuffsConsumed);

    void Awake() => mainCam = Camera.main;

    public void Initialize(int required, Vector3 pos)
    {
        RequiredHandcuffs = required;
        handcuffsConsumed = 0;
        transform.position = pos;
        queueTarget = pos;
        isWalking = false;
        if (hudRoot != null) hudRoot.gameObject.SetActive(true);
        UpdateHUD();
    }

    public void ConsumeHandcuff()
    {
        handcuffsConsumed++;
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (requirementText == null) return;
        requirementText.text = Remaining.ToString();
        if (Remaining == 0)
            StartCoroutine(HideHUDRoutine());
    }

    IEnumerator HideHUDRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (hudRoot != null) hudRoot.gameObject.SetActive(false);
    }

    public void MoveToQueuePosition(Vector3 pos)
    {
        queueTarget = pos;
        isWalking = true;
    }

    // exitPos까지 걸어간 뒤 콜백 호출 — SetActive/풀 처리는 호출부(DeskZone)에서
    public void LeaveToPool(Vector3 exitPos, Action onArrived)
    {
        isWalking = false;
        StartCoroutine(LeaveRoutine(exitPos, onArrived));
    }

    // 여러 경유지를 순서대로 걸어간 뒤 마지막 지점에 멈춤 (ㄱ자 이동 등)
    public void WalkPath(Vector3[] waypoints)
    {
        isWalking = false;
        StartCoroutine(WalkPathRoutine(waypoints));
    }

    IEnumerator WalkPathRoutine(Vector3[] waypoints)
    {
        const float sqThreshold = 0.1f * 0.1f;
        foreach (Vector3 wp in waypoints)
        {
            while (true)
            {
                float dx = transform.position.x - wp.x;
                float dz = transform.position.z - wp.z;
                if (dx * dx + dz * dz <= sqThreshold) break;
                Vector3 xzTarget = new Vector3(wp.x, transform.position.y, wp.z);
                transform.position = Vector3.MoveTowards(transform.position, xzTarget, walkSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = new Vector3(wp.x, transform.position.y, wp.z);
        }
    }

    void LateUpdate()
    {
        if (hudRoot == null) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null) hudRoot.rotation = mainCam.transform.rotation;
    }

    void Update()
    {
        if (!isWalking) return;
        transform.position = Vector3.MoveTowards(transform.position, queueTarget, walkSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, queueTarget) < 0.05f)
        {
            isWalking = false;
            transform.position = queueTarget;
        }
    }

    IEnumerator LeaveRoutine(Vector3 exitPos, Action onArrived)
    {
        float sqThreshold = 0.1f * 0.1f;
        while (true)
        {
            float dx = transform.position.x - exitPos.x;
            float dz = transform.position.z - exitPos.z;
            if (dx * dx + dz * dz <= sqThreshold) break;
            // Y를 그대로 유지하면서 XZ 방향으로만 이동 (jailExit Y 차이로 루프 탈출 불가 방지)
            Vector3 xzTarget = new Vector3(exitPos.x, transform.position.y, exitPos.z);
            transform.position = Vector3.MoveTowards(transform.position, xzTarget, walkSpeed * Time.deltaTime);
            yield return null;
        }
        onArrived?.Invoke();
    }
}
