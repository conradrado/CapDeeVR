using UnityEngine;

public class WeightOrderPuzzle : MonoBehaviour
{
    [Header("Slots (left → right)")]
    public WeightOrderSlot[] slots;           // 5칸(원하면 가변)

    [Header("Result")]
    public PuzzleGateController gate;         // CoverBox를 제어
    public Light indicator;                   // (선택) 초록/빨강 표시

    [Header("Rules")]
    public bool requireAllFilled = true;      // 모두 꽂혀야만 판정
    public bool strictIncreasing = true;      // true: >, false: >=

    bool solved;

    void Start()
    {
        foreach (var s in slots) s.OnChange += Evaluate;
        Evaluate();
    }

    void Evaluate()
    {
        // 1) 비어있으면 실패(옵션)
        if (requireAllFilled)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].CurrentItem == null) { SetSolved(false); return; }
        }

        // 2) 오름차순 검사
        float last = float.NegativeInfinity;
        for (int i = 0; i < slots.Length; i++)
        {
            var item = slots[i].CurrentItem;
            if (item == null) { SetSolved(false); return; }

            float w = item.weight;
            if (strictIncreasing ? !(w > last) : !(w >= last))
            { SetSolved(false); return; }

            last = w;
        }

        SetSolved(true);
    }

    void SetSolved(bool ok)
    {
        if (solved == ok) return;
        solved = ok;

        if (indicator) indicator.color = ok ? Color.green : Color.red;

        if (gate)
        {
            if (ok) gate.RevealCover();
            else gate.HideCover();   // 되돌림이 필요 없으면 이 줄 삭제
        }
    }
}

