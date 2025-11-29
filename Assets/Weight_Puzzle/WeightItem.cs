using UnityEngine;

public class WeightItem : MonoBehaviour
{
    [Min(0f)] public float weight = 1f;     // 이 값으로 오름차순을 판정
    public string label;                    // (선택) UI/Text 용
}
