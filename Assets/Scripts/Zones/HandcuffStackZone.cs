using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandcuffStackZone : BaseZone
{
    [Header("Stack Layout")]
    [SerializeField] Vector3 stackBase = Vector3.zero;
    [SerializeField] float itemSpacing = 0.18f;

    [Header("Tuning")]
    [SerializeField] float collectInterval = 0.2f;

    readonly List<GameObject> stack = new List<GameObject>();
    Coroutine collectRoutine;

    // Called by ManufacturingZone to deposit a finished handcuff
    public void AddHandcuff(GameObject obj)
    {
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
            if (top != null) inventory.AddHandcuff(top);

            yield return new WaitForSeconds(collectInterval);
        }
    }
}
