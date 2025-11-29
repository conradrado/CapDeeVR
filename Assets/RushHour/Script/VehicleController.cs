using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public float cellSize = 1.0f;          // 그리드 한 칸 크기
    public float moveTime = 0.2f;          // 부드럽게 이동할 시간
    public LayerMask obstacleMask;         // 충돌 감지용 마스크

    private bool isMoving = false;

    /// <summary>
    /// 이동 시도 함수 (성공 시 true 반환)
    /// </summary>
    public bool TryMove(Vector3 direction)
    {
        if (isMoving) return false;

        Vector3 targetPos = transform.position + direction * cellSize;

        // 충돌 감지: BoxCast로 한 칸 앞에 물체가 있는지 확인
        Collider[] hits = Physics.OverlapBox(
            targetPos,
            transform.localScale / 2 * 0.9f,
            transform.rotation,
            obstacleMask
        );

        if (hits.Length > 0)
        {
            // 앞에 장애물 있음
            return false;
        }

        // 이동 시작
        StartCoroutine(MoveToPosition(targetPos));
        return true;
    }

    /// <summary>
    /// 부드럽게 이동하는 코루틴
    /// </summary>
    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(start, targetPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }
}
