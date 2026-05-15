using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Stack Layout")]
    [SerializeField] float baseX        = 0f;      // 열의 X 오프셋
    [SerializeField] float baseY        = 0.15f;   // 첫 번째 아이템 Y 오프셋
    [SerializeField] float handcuffZ    = 0.45f;   // 수갑 고정 열 Z (플레이어 앞)
    [SerializeField] float col1Z        = -0.45f;  // 뒤쪽 첫 번째 열 Z (광석·돈)
    [SerializeField] float colZSpacing  = -0.45f;  // 뒤쪽 열 간 Z 간격
    [SerializeField] float itemSpacing  = 0.22f;   // 한 열 안에서 아이템 높이 간격
    [SerializeField] float posLerpSpeed = 14f;

    public int MaxOre { get; set; } = UpgradeManager.ORE_CAP_LVL1;

    readonly List<GameObject> oreStack      = new List<GameObject>();
    readonly List<GameObject> handcuffStack = new List<GameObject>();
    readonly List<GameObject> moneyStack    = new List<GameObject>();

    public int  OreCount      => oreStack.Count;
    public int  HandcuffCount => handcuffStack.Count;
    public int  MoneyCount    => moneyStack.Count;
    public bool CanAddOre     => oreStack.Count < MaxOre;

    void LateUpdate()
    {
        // 수갑: 플레이어 앞 고정 열
        if (handcuffStack.Count > 0)
            UpdateColumn(handcuffStack, handcuffZ);

        // 광석·돈: 뒤쪽 동적 배치 (활성 종류 수에 따라 Z 위치 자동 결정)
        var backCols = new List<List<GameObject>>();
        if (oreStack.Count   > 0) backCols.Add(oreStack);
        if (moneyStack.Count > 0) backCols.Add(moneyStack);

        for (int col = 0; col < backCols.Count; col++)
        {
            float z = col1Z + col * colZSpacing;
            UpdateColumn(backCols[col], z);
        }
    }

    void UpdateColumn(List<GameObject> column, float z)
    {
        // null 제거
        for (int i = column.Count - 1; i >= 0; i--)
            if (column[i] == null) column.RemoveAt(i);

        // index 0 = 맨 아래, Count-1 = 맨 위
        for (int i = 0; i < column.Count; i++)
        {
            Vector3 localTarget = new Vector3(baseX, baseY + i * itemSpacing, z);
            Vector3 worldTarget = transform.TransformPoint(localTarget);
            column[i].transform.position = Vector3.Lerp(
                column[i].transform.position, worldTarget, posLerpSpeed * Time.deltaTime);
            column[i].transform.rotation = Quaternion.Lerp(
                column[i].transform.rotation, transform.rotation, posLerpSpeed * Time.deltaTime);
        }
    }

    // ── 광석 ──────────────────────────────────────────

    public void AddOre(GameObject obj)
    {
        if (obj == null) return;
        PlaceAboveSlot(obj, oreStack.Count, col1Z);
        oreStack.Add(obj);
    }

    public GameObject RemoveTopOre()
    {
        if (oreStack.Count == 0) return null;
        int last = oreStack.Count - 1;
        var obj = oreStack[last];
        oreStack.RemoveAt(last);
        return obj;
    }

    // ── 수갑 ──────────────────────────────────────────

    public void AddHandcuff(GameObject obj)
    {
        if (obj == null) return;
        PlaceAboveSlot(obj, handcuffStack.Count, handcuffZ);
        handcuffStack.Add(obj);
    }

    public GameObject RemoveTopHandcuff()
    {
        if (handcuffStack.Count == 0) return null;
        int last = handcuffStack.Count - 1;
        var obj = handcuffStack[last];
        handcuffStack.RemoveAt(last);
        return obj;
    }

    // ── 돈 ────────────────────────────────────────────

    public void AddMoney(GameObject obj)
    {
        if (obj == null) return;
        PlaceAboveSlot(obj, moneyStack.Count, col1Z + colZSpacing);
        moneyStack.Add(obj);
    }

    public GameObject RemoveTopMoney()
    {
        if (moneyStack.Count == 0) return null;
        int last = moneyStack.Count - 1;
        var obj = moneyStack[last];
        moneyStack.RemoveAt(last);
        ItemVisualPool.Instance.ReturnMoney(obj);
        return obj;
    }

    // ── 공통 ──────────────────────────────────────────

    // 스택 슬롯 바로 위에서 시작 → LateUpdate lerp로 제자리에 안착
    void PlaceAboveSlot(GameObject obj, int stackIndex, float z)
    {
        Vector3 local = new Vector3(baseX, baseY + stackIndex * itemSpacing + 1.2f, z);
        obj.transform.position = transform.TransformPoint(local);
    }
}
