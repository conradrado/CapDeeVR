using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform puzzleViewPosition; // 퍼즐을 위에서 보는 목표 위치
    public float moveSpeed = 3f;         // 이동 속도
    public float rotateSpeed = 180f;     // 회전 속도 (도/초)

    private Transform xrRig;             // XR Origin
    private bool isMoving = false;

    private void Start()
    {
        // XR Origin을 찾음
        xrRig = GameObject.Find("XR Origin (XR Rig)").transform;

        // 처음에는 이동하지 않음
        isMoving = false;
    }

    private void Update()
    {
        if (!isMoving || xrRig == null || puzzleViewPosition == null)
            return;

        // 위치 부드럽게 이동
        xrRig.position = Vector3.MoveTowards(
            xrRig.position,
            puzzleViewPosition.position,
            moveSpeed * Time.deltaTime
        );

        // 회전 부드럽게 (목표 회전: 위에서 아래를 봄)
        Quaternion targetRotation = Quaternion.Euler(90f, 0f, 0f);
        xrRig.rotation = Quaternion.RotateTowards(
            xrRig.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );

        // 도착하면 정지
        if (Vector3.Distance(xrRig.position, puzzleViewPosition.position) < 0.01f &&
            Quaternion.Angle(xrRig.rotation, targetRotation) < 0.5f)
        {
            isMoving = false;
        }
    }

    // 버튼에 연결되는 함수
    public void MoveToPuzzleView()
    {
        isMoving = true;
    }
}
