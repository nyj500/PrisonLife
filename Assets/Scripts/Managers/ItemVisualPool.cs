using UnityEngine;

public class ItemVisualPool : MonoBehaviour
{
    public static ItemVisualPool Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] GameObject oreVisualPrefab;
    [SerializeField] GameObject handcuffVisualPrefab;
    [SerializeField] GameObject moneyVisualPrefab;

    [Header("Pool Sizes")]
    [SerializeField] int oreInitialSize      = 20;
    [SerializeField] int handcuffInitialSize = 20;
    [SerializeField] int moneyInitialSize    = 20;

    GameObjectPool orePool;
    GameObjectPool handcuffPool;
    GameObjectPool moneyPool;

    void Awake()
    {
        Instance = this;
        orePool      = new GameObjectPool(oreVisualPrefab,      oreInitialSize,      transform);
        handcuffPool = new GameObjectPool(handcuffVisualPrefab, handcuffInitialSize, transform);
        moneyPool    = new GameObjectPool(moneyVisualPrefab,    moneyInitialSize,    transform);
    }

    public GameObject GetOre(Vector3 pos)           => orePool.Get(pos, Quaternion.identity);
    public void       ReturnOre(GameObject go)      => orePool.Return(go);

    public GameObject GetHandcuff(Vector3 pos)      => handcuffPool.Get(pos, Quaternion.identity);
    public void       ReturnHandcuff(GameObject go) => handcuffPool.Return(go);

    public GameObject GetMoney(Vector3 pos)         => moneyPool.Get(pos, Quaternion.identity);
    public void       ReturnMoney(GameObject go)    => moneyPool.Return(go);
}
