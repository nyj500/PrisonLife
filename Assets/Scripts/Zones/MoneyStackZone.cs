using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoneyStackZone : BaseZone
{
    [Header("Stack Layout")]
    [SerializeField] Vector3 stackBase = Vector3.zero;
    [SerializeField] float itemSpacing = 0.15f;

    [Header("Tuning")]
    [SerializeField] float collectInterval = 0.15f;

    readonly List<GameObject> stack = new List<GameObject>();
    Coroutine collectRoutine;

    // DeskZone에서 수감자 처리 완료 시 호출 — 돈 오브젝트를 스택에 추가
    public void AddMoney(GameObject obj, int value = 1)
    {
        var item = obj.GetComponent<MoneyItem>();
        if (item == null) item = obj.AddComponent<MoneyItem>();
        item.Value = value;

        obj.transform.position = StackTopPosition;
        stack.Add(obj);
    }

    public Vector3 StackTopPosition =>
        transform.TransformPoint(stackBase + Vector3.up * (stack.Count * itemSpacing));

    void LateUpdate()
    {
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            if (stack[i] == null) { stack.RemoveAt(i); continue; }
            Vector3 target = transform.TransformPoint(stackBase + Vector3.up * (i * itemSpacing));
            stack[i].transform.position = Vector3.Lerp(stack[i].transform.position, target, 12f * Time.deltaTime);
        }
    }

    protected override void OnPlayerEnter()
    {
        if (collectRoutine != null) StopCoroutine(collectRoutine);
        collectRoutine = StartCoroutine(CollectLoop());
    }

    protected override void OnPlayerExit()
    {
        if (collectRoutine != null) { StopCoroutine(collectRoutine); collectRoutine = null; }
    }

    IEnumerator CollectLoop()
    {
        while (player != null && inventory != null)
        {
            if (stack.Count == 0) { yield return new WaitForSeconds(0.2f); continue; }

            GameObject top = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            if (top != null)
            {
                // 수령 시점에 돈 카운터 증가
                var item = top.GetComponent<MoneyItem>();
                int value = item != null ? item.Value : 1;
                GameManager.Instance.AddMoney(value);

                // 플레이어 등 뒤 스택에 시각적으로 추가
                inventory.AddMoney(top);
            }

            yield return new WaitForSeconds(collectInterval);
        }
    }
}
