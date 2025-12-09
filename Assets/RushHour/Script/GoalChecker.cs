using UnityEngine;

public class GoalChecker : MonoBehaviour
{
    public GameObject mainCar; // 빨간 차
    public Transform goalTransform; // 출구 위치
    public float successThreshold = 0.2f; // 어느 정도 가까워야 도달로 판단

    private void Update()
    {
        if (Vector3.Distance(mainCar.transform.position, goalTransform.position) < successThreshold)
        {
            Debug.Log("퍼즐 클리어!");
            // TODO: 퍼즐 완료 이벤트 호출, UI 표시 등
        }
    }
}

