using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManufacturingZone : BaseZone
{
    [Header("References")]
    [SerializeField] HandcuffStackZone handcuffStackZone;
    [SerializeField] Material conveyorMaterial;

    [Header("Tuning")]
    [SerializeField] float depositInterval = 0.5f;
    [SerializeField] float manufactureTime = 1.0f;
    [SerializeField] float conveyorScrollSpeed = 0.5f;

    readonly Queue<GameObject> processQueue = new Queue<GameObject>();
    Coroutine depositRoutine;
    bool isProcessing;

    protected override void OnAwake()
    {
        StartCoroutine(ProcessLoop());
    }

    void Update()
    {
        if (conveyorMaterial != null)
            conveyorMaterial.mainTextureOffset += Vector2.right * conveyorScrollSpeed * Time.deltaTime;
    }

    protected override void OnPlayerEnter()
    {
        if (depositRoutine != null) StopCoroutine(depositRoutine);
        depositRoutine = StartCoroutine(DepositLoop());
    }

    protected override void OnPlayerExit()
    {
        if (depositRoutine != null) { StopCoroutine(depositRoutine); depositRoutine = null; }
    }

    IEnumerator DepositLoop()
    {
        while (player != null && inventory != null)
        {
            if (inventory.OreCount == 0) { yield return new WaitForSeconds(0.2f); continue; }

            GameObject oreVisual = inventory.RemoveTopOre();
            if (oreVisual != null)
            {
                oreVisual.transform.position = transform.position;
                processQueue.Enqueue(oreVisual);
            }

            yield return new WaitForSeconds(depositInterval);
        }
    }

    // Runs independently – manufacturing continues after player leaves
    IEnumerator ProcessLoop()
    {
        while (true)
        {
            if (processQueue.Count == 0) { yield return new WaitForSeconds(0.2f); continue; }

            GameObject ore = processQueue.Dequeue();
            yield return new WaitForSeconds(manufactureTime);

            if (ore != null) ItemVisualPool.Instance.ReturnOre(ore);

            if (handcuffStackZone != null)
            {
                GameObject handcuff = SpawnHandcuffVisual(transform.position);
                yield return StartCoroutine(AnimateTo(handcuff, handcuffStackZone.StackTopPosition));
                handcuffStackZone.AddHandcuff(handcuff);
            }
        }
    }

    IEnumerator AnimateTo(GameObject obj, Vector3 destination)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        Vector3 start = obj.transform.position;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float arc = Mathf.Sin(t * Mathf.PI) * 1.2f;
            obj.transform.position = Vector3.Lerp(start, destination, t) + Vector3.up * arc;
            yield return null;
        }
        obj.transform.position = destination;
    }

    GameObject SpawnHandcuffVisual(Vector3 pos)
    {
        return ItemVisualPool.Instance.GetHandcuff(pos);
    }
}
