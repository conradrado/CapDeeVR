using UnityEngine;

public class Elevator : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 2f;
    public Vector3 targetPosition;   // 월드 좌표
    public float stopEpsilon = 0.01f;

    Vector3 startPosition;
    bool movingUp, movingDown;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        startPosition = rb.position;             // 시작 위치 저장
        rb.isKinematic = true;                   // MovePosition 사용 시 권장
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (movingUp)
        {
            var newPos = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            if (Vector3.Distance(newPos, targetPosition) <= stopEpsilon) movingUp = false;
        }
        else if (movingDown)
        {
            var newPos = Vector3.MoveTowards(rb.position, startPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            if (Vector3.Distance(newPos, startPosition) <= stopEpsilon) movingDown = false;
        }
    }

    public void MoveUp()   { movingDown = false; movingUp = true; }
    public void MoveDown() { movingUp = false;  movingDown = true; }
}
