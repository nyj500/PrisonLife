using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManufacturingZone : BaseZone
{
    [Header("References")]
    [SerializeField] HandcuffStackZone handcuffStackZone;
    [SerializeField] Material conveyorMaterial;

    [Header("Stack Layout")]
    [SerializeField] Vector3 stackBase = Vector3.zero;
    [SerializeField] float itemSpacing  = 0.3f;

    [Header("Tuning")]
    [SerializeField] float depositInterval    = 0.5f;
    [SerializeField] float manufactureTime    = 1.0f;
    [SerializeField] float conveyorScrollSpeed = 0.5f;

    readonly List<GameObject> pendingOres = new List<GameObject>();
    Coroutine depositRoutine;

    protected override void OnAwake()
    {
        StartCoroutine(ProcessLoop());
    }

    void Update()
    {
        if (conveyorMaterial != null)
            conveyorMaterial.mainTextureOffset += Vector2.right * conveyorScrollSpeed * Time.deltaTime;
    }

    void LateUpdate()
    {
        for (int i = pendingOres.Count - 1; i >= 0; i--)
        {
            if (pendingOres[i] == null) { pendingOres.RemoveAt(i); continue; }
            Vector3 target = transform.position + stackBase + Vector3.up * (i * itemSpacing);
            pendingOres[i].transform.position = Vector3.Lerp(
                pendingOres[i].transform.position, target, 12f * Time.deltaTime);
        }
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
                // 스택 맨 위에서 아래로 낙하하는 연출
                Vector3 spawnPos = transform.position + stackBase
                    + Vector3.up * (pendingOres.Count * itemSpacing + 1.5f);
                oreVisual.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
                pendingOres.Add(oreVisual);
                SoundManager.Instance?.Play(SFXType.OreDeposit);
            }

            yield return new WaitForSeconds(depositInterval);
        }
    }

    // Runs independently – manufacturing continues after player leaves
    IEnumerator ProcessLoop()
    {
        while (true)
        {
            if (pendingOres.Count == 0) { yield return new WaitForSeconds(0.2f); continue; }

            GameObject ore = pendingOres[0];
            pendingOres.RemoveAt(0);
            yield return new WaitForSeconds(manufactureTime);

            if (ore != null) ItemVisualPool.Instance.ReturnOre(ore);

            if (handcuffStackZone != null)
            {
                GameObject handcuff = SpawnHandcuffVisual(transform.position);
                yield return StartCoroutine(AnimateTo(handcuff, handcuffStackZone.StackTopPosition));
                handcuffStackZone.AddHandcuff(handcuff);
                SoundManager.Instance?.Play(SFXType.ManufactureComplete);
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

    // WorkerNPC가 호출 — 광석 비주얼을 제조 존 스택에 추가
    public void ReceiveOre(GameObject oreVisual)
    {
        if (oreVisual == null) return;
        Vector3 spawnPos = transform.position + stackBase
            + Vector3.up * (pendingOres.Count * itemSpacing + 1.5f);
        oreVisual.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        pendingOres.Add(oreVisual);
        SoundManager.Instance?.Play(SFXType.OreDeposit);
    }

    GameObject SpawnHandcuffVisual(Vector3 pos)
    {
        return ItemVisualPool.Instance.GetHandcuff(pos);
    }
}
