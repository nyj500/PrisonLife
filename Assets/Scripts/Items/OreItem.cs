using UnityEngine;
using System.Collections;

public class OreItem : MonoBehaviour
{
    [SerializeField] float respawnDelay = 5f;

    Renderer[] renderers;
    Collider[] cols;
    Vector3 originPos;

    public bool IsMined { get; private set; }

    void Awake()
    {
        originPos = transform.position;
        renderers = GetComponentsInChildren<Renderer>();
        cols = GetComponentsInChildren<Collider>();
    }

    public void Mine()
    {
        if (IsMined) return;
        IsMined = true;
        SetVisible(false);
        StartCoroutine(RespawnRoutine());
    }

    void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
        foreach (var c in cols) c.enabled = visible;
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        transform.position = originPos;
        SetVisible(true);
        IsMined = false;
    }
}
