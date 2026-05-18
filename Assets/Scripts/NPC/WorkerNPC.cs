using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class WorkerNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MineZone         mineZone;
    [SerializeField] ManufacturingZone manufacturingZone;
    [SerializeField] Transform         factoryWaypoint;

    [Header("Tuning")]
    [SerializeField] int   carryCapacity   = 5;
    [SerializeField] float walkSpeed       = 3f;
    [SerializeField] float mineInterval    = 1f;
    [SerializeField] float depositInterval = 0.3f;
    [SerializeField] float carryBaseHeight = 1.5f;
    [SerializeField] float carrySpacing    = 0.2f;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int MineHash  = Animator.StringToHash("Mine");

    NavMeshAgent agent;
    Animator     anim;
    readonly List<GameObject> carryStack = new List<GameObject>();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponentInChildren<Animator>();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        agent.speed = walkSpeed;
        StartCoroutine(WorkLoop());
    }

    void LateUpdate()
    {
        anim?.SetFloat(SpeedHash, agent.velocity.sqrMagnitude > 0.01f ? 1f : 0f);

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
        yield return null;

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
                OreItem ore = mineZone.GetNearestOre(transform.position, float.MaxValue);
                if (ore == null) { yield return new WaitForSeconds(1f); continue; }

                ore.Reserve(); // 다른 워커가 같은 광석을 목표로 잡지 않도록 예약

                yield return StartCoroutine(MoveTo(ore.transform.position));

                // 도착 전 리스폰·선점 됐을 경우
                if (ore.IsMined) { ore.Unreserve(); continue; }

                anim?.SetTrigger(MineHash);
                yield return new WaitForSeconds(mineInterval);
                if (ore.IsMined) { ore.Unreserve(); continue; }

                ore.Mine(); // Mine() 이후 리스폰 시 IsReserved 자동 초기화
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
        agent.SetDestination(target);
        yield return null; // pathPending 갱신 대기
        yield return new WaitUntil(() =>
            !agent.pathPending &&
            (agent.remainingDistance <= agent.stoppingDistance + 0.05f ||
             agent.pathStatus == NavMeshPathStatus.PathInvalid));
    }
}
