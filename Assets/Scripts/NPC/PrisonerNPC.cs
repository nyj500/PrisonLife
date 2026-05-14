using UnityEngine;
using System.Collections;
using System;

public class PrisonerNPC : MonoBehaviour
{
    [SerializeField] float walkSpeed = 2f;

    public int RequiredHandcuffs { get; private set; }
    int handcuffsConsumed;

    Vector3 queueTarget;
    bool isWalking;

    public bool IsFullyProcessed => handcuffsConsumed >= RequiredHandcuffs;
    public int Remaining => Mathf.Max(0, RequiredHandcuffs - handcuffsConsumed);

    // SetActive 호출 없음 — 호출부(DeskZone)에서 활성 상태를 직접 관리
    public void Initialize(int required, Vector3 pos)
    {
        RequiredHandcuffs = required;
        handcuffsConsumed = 0;
        transform.position = pos;
        queueTarget = pos;
        isWalking = false;
    }

    public void ConsumeHandcuff() => handcuffsConsumed++;

    public void MoveToQueuePosition(Vector3 pos)
    {
        queueTarget = pos;
        isWalking = true;
    }

    // 퇴장 → 마지막 슬롯으로 재사용
    public void LeaveAndRecycle(Vector3 exitDir, Vector3 recyclePos, int newRequirement, Action onRecycled)
    {
        isWalking = false;
        StartCoroutine(LeaveRoutine(exitDir, recyclePos, newRequirement, onRecycled));
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

    IEnumerator LeaveRoutine(Vector3 exitDir, Vector3 recyclePos, int newRequirement, Action onRecycled)
    {
        // 감옥 방향으로 퇴장
        Vector3 exitTarget = transform.position + exitDir.normalized * 8f;
        while (Vector3.Distance(transform.position, exitTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitTarget, walkSpeed * Time.deltaTime);
            yield return null;
        }

        // 텔레포트 중 깜빡임 방지
        gameObject.SetActive(false);

        RequiredHandcuffs = newRequirement;
        handcuffsConsumed = 0;
        transform.position = recyclePos;
        queueTarget = recyclePos;
        isWalking = false;

        gameObject.SetActive(true);
        onRecycled?.Invoke();
    }
}
