using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorkerNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MineZone mineZone;
    [SerializeField] ManufacturingZone manufacturingZone;
    [SerializeField] Transform factoryWaypoint;

    [Header("Tuning")]
    [SerializeField] int   carryCapacity   = 5;
    [SerializeField] float walkSpeed       = 3f;
    [SerializeField] float mineInterval    = 1f;
    [SerializeField] float depositInterval = 0.3f;
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
            if (mineZone == null || manufacturingZone == null || factoryWaypoint == null)
            {
                Debug.LogWarning("[WorkerNPC] 레퍼런스 미설정 — UpgradeManager Inspector를 확인하세요.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            // ── 채굴 단계 ────────────────────────────────
            while (carryStack.Count < carryCapacity)
            {
                // 가장 가까운 채굴 가능 광석 탐색
                OreItem ore = mineZone.GetNearestOre(transform.position, float.MaxValue);
                if (ore == null) { yield return new WaitForSeconds(1f); continue; }

                // 광석 위치로 이동
                yield return StartCoroutine(MoveTo(ore.transform.position));

                // 이동하는 동안 다른 NPC가 먼저 채굴했을 수 있음
                if (ore.IsMined) continue;

                // 채굴
                yield return new WaitForSeconds(mineInterval);
                if (ore.IsMined) continue;

                ore.Mine();
                GameObject vis = ItemVisualPool.Instance.GetOre(
                    transform.position + Vector3.up * 0.5f);
                carryStack.Add(vis);
            }

            // ── 납품 단계 ────────────────────────────────
            yield return StartCoroutine(MoveTo(factoryWaypoint.position));

            while (carryStack.Count > 0)
            {
                int last = carryStack.Count - 1;
                GameObject vis = carryStack[last];
                carryStack.RemoveAt(last);
                manufacturingZone.ReceiveOre(vis);
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

            transform.position = Vector3.MoveTowards(
                transform.position, xzTarget, walkSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
