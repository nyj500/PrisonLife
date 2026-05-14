using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BaseZone : MonoBehaviour
{
    protected PlayerController player;
    protected PlayerInventory inventory;
    protected PlayerAnimator playerAnim;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        OnAwake();
    }

    protected virtual void OnAwake() { }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.GetComponent<PlayerController>();
        inventory = other.GetComponent<PlayerInventory>();
        playerAnim = other.GetComponentInChildren<PlayerAnimator>();
        OnPlayerEnter();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerExit();
        player = null;
        inventory = null;
        playerAnim = null;
    }

    protected abstract void OnPlayerEnter();
    protected abstract void OnPlayerExit();

    // Creates a minimal primitive visual for use as an inventory item
    protected static GameObject CreatePrimitive(PrimitiveType type, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = color;
        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
        return go;
    }
}
