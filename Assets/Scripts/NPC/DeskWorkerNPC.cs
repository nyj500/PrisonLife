using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeskWorkerNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] HandcuffStackZone handcuffStackZone;
    [SerializeField] DeskZone deskZone;
    [SerializeField] Transform pickupWaypoint;
    [SerializeField] Transform deskWaypoint;

    [Header("Tuning")]
    [SerializeField] int   carryCapacity   = 5;
    [SerializeField] float walkSpeed       = 3f;
    [SerializeField] float pickupInterval  = 0.2f;
    [SerializeField] float depositInterval = 0.2f;
    [SerializeField] float carryBaseHeight = 1.5f;
    [SerializeField] float carrySpacing    = 0.2f;

    readonly List<GameObject> carryStack = new List<GameObject>();

    public void Activate()
    {
        gameObject.SetActive(true);
        StartCoroutine(WorkLoop());
    }

    void LateUpdate()
    {
        for (int i = 0; i < carryStack.Count; i++)
        {
            if (carryStack[i] == null) continue;
            Vector3 target = transform.position + Vector3.up * (carryBaseHeight + i * carrySpacing);
            carryStack[i].transform.position = Vector3.Lerp(
                carryStack[i].transform.position, target, 12f * Time.deltaTime);
        }
    }

    IEnumerator WorkLoop()
    {
        yield return null; // StartCoroutine이 즉시 반환되도록 먼저 yield

        while (true)
        {
            if (handcuffStackZone == null || deskZone == null || pickupWaypoint == null || deskWaypoint == null)
            {
                Debug.LogWarning("[DeskWorkerNPC] 레퍼런스 미설정 — Inspector를 확인하세요.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            // 1. 수갑 존으로 이동
            yield return StartCoroutine(MoveTo(pickupWaypoint.position));

            // 2. 수갑 수령 (있는 만큼, 최대 carryCapacity)
            while (carryStack.Count < carryCapacity)
            {
                if (!handcuffStackZone.TryTakeHandcuff(out GameObject vis)) break;
                carryStack.Add(vis);
                SoundManager.Instance?.Play(SFXType.HandcuffPickup);
                yield return new WaitForSeconds(pickupInterval);
            }

            if (carryStack.Count == 0) { yield return new WaitForSeconds(0.5f); continue; }

            // 3. 데스크로 이동
            yield return StartCoroutine(MoveTo(deskWaypoint.position));

            // 4. 수갑 납품
            while (carryStack.Count > 0)
            {
                int last = carryStack.Count - 1;
                GameObject vis = carryStack[last];
                carryStack.RemoveAt(last);
                deskZone.AddHandcuffToDesk(vis);
                SoundManager.Instance?.Play(SFXType.HandcuffDeposit);
                yield return new WaitForSeconds(depositInterval);
            }
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        while (true)
        {
            float dx = transform.position.x - target.x;
            float dz = transform.position.z - target.z;
            if (dx * dx + dz * dz <= 0.05f * 0.05f) break;

            Vector3 xzTarget = new Vector3(target.x, transform.position.y, target.z);
            Vector3 dir = xzTarget - transform.position;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir.normalized), 10f * Time.deltaTime);

            transform.position = Vector3.MoveTowards(transform.position, xzTarget, walkSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
