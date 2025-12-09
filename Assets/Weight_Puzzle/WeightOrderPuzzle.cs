using UnityEngine;
using UnityEngine.SceneManagement;   //  씬 이동용
using System.Collections;            //  코루틴용

public class WeightOrderPuzzle : MonoBehaviour
{
    [Header("Slots (left → right)")]
    public WeightOrderSlot[] slots;           // 5칸(원하면 가변)

    [Header("Result")]
    public PuzzleGateController gate;         // CoverBox를 제어 (기존 기능 유지)
    public Light indicator;                   // (선택) 초록/빨강 표시

    [Header("Rules")]
    public bool requireAllFilled = true;      // 모두 꽂혀야만 판정
    public bool strictIncreasing = true;      // true: >, false: >=

    // 여기부터 새로 추가하는 부분
    [Header("Scene Clear Settings")]
    [Tooltip("클리어 후 돌아갈 메인 퍼즐 씬 이름")]
    public string returnSceneName = "Puzzle";

    [Tooltip("클리어 문구가 뜬 뒤 씬을 넘기기까지 대기 시간(초)")]
    public float returnDelay = 5f;

    [Tooltip("클리어 문구 UI 오브젝트 (Canvas/Text 등)")]
    public GameObject clearMessageObject;  // 미리 꺼둔 상태로 둘 것

    bool solved;
    bool isReturning;                      // 중복 코루틴 방지용

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

        // 기존 기능: 불 색상 + 커버박스 열고 닫기
        if (indicator) indicator.color = ok ? Color.green : Color.red;

        if (gate)
        {
            if (ok) gate.RevealCover();
            else gate.HideCover();   // 되돌림 필요 없으면 이 줄 지워도 됨
        }

        // 여기서 퍼즐 성공 시 클리어 연출 + 씬 복귀
        if (ok)
            OnPuzzleSolved();
    }

    void OnPuzzleSolved()
    {
        if (isReturning) return;
        isReturning = true;

        // 클리어 문구 UI 켜기
        if (clearMessageObject)
            clearMessageObject.SetActive(true);

        // 5초 뒤 메인 퍼즐 씬으로 이동
        StartCoroutine(ReturnToMainScene());
    }

    IEnumerator ReturnToMainScene()
    {
        yield return new WaitForSeconds(returnDelay);
        if (!string.IsNullOrEmpty(returnSceneName))
        {
            SceneManager.LoadScene(returnSceneName, LoadSceneMode.Single);
        }
    }
}


