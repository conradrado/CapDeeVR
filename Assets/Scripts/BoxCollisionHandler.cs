using UnityEngine;

// 이 스크립트는 XR Origin이 박스를 밀었을 때, 박스가 힘을 받아서 움직이게 만들어줍니다
public class BoxCollisionHandler : MonoBehaviour
{
    // Inspector 창에서 밀치는 힘의 세기를 조절할 수 있도록 public으로 선언
    public float pushForce = 300f;

    // 지난 프레임에서의 XR Origin 위치를 저장할 변수
    private Vector3 lastPosition;
    
    // XR Origin의 이동 방향 및 속도
    private Vector3 velocity;

    // 게임 시작 시 초기 위치 저장
    void Start()
    {
        lastPosition = transform.position;
    }

    // 일정 시간 간격으로 실행되는 물리 업데이트 함수
    void FixedUpdate()
    {
        // 현재 위치 - 지난 위치 = 이동 거리
        // 이동 거리 ÷ 시간 = 속도 (velocity)
        velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

        // 다음 계산을 위해 현재 위치 저장
        lastPosition = transform.position;
    }

    // 충돌이 일어났을 때 자동으로 호출되는 함수
    void OnCollisionEnter(Collision collision)
    {
        // 충돌한 물체가 "Box" 태그를 가진 오브젝트일 경우에만 실행
        if (collision.gameObject.CompareTag("Box"))
        {
            // 박스에서 Rigidbody 컴포넌트를 가져오기
            Rigidbody boxRb = collision.gameObject.GetComponent<Rigidbody>();

            // Rigidbody가 있다면 (null이 아니라면)
            if (boxRb != null)
            {
                // XR Origin의 이동 방향을 정규화해서 방향만 추출
                Vector3 pushDir = velocity.normalized;

                // 박스에 즉시 힘을 가해 밀어냄 (Impulse 모드로 순간적으로 밀기)
                boxRb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            }
        }
    }
}
