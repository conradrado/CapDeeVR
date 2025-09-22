using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Elevator : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 2f;
    public Vector3 targetPosition;           // 월드 좌표
    public float stopEpsilon = 0.005f;       // 5mm

    public UnityEvent onReachedTop, onReachedBottom;

    private Vector3 startPosition;
    private bool movingUp, movingDown;
    private float eps2;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        startPosition = rb.position;
        eps2 = stopEpsilon * stopEpsilon;

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation
                       | RigidbodyConstraints.FreezePositionX
                       | RigidbodyConstraints.FreezePositionZ;
    }

    void FixedUpdate()
    {
        if (movingUp)
        {
            Vector3 before = rb.position;
            Vector3 after  = Vector3.MoveTowards(before, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(after);

            // 1) 충분히 가까움  2) 목표를 '지나쳤음'(교차 판정)
            if ((targetPosition - after).sqrMagnitude <= eps2 ||
                Vector3.Dot(targetPosition - startPosition, targetPosition - after) <= 0f)
            {
                StopAt(targetPosition);
                movingUp = false;
                onReachedTop?.Invoke();
                Debug.Log("Yo Top!");
            }
        }
        else if (movingDown)
        {
            Vector3 before = rb.position;
            Vector3 after  = Vector3.MoveTowards(before, startPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(after);

            if ((startPosition - after).sqrMagnitude <= eps2 ||
                Vector3.Dot(startPosition - targetPosition, startPosition - after) <= 0f)
            {
                StopAt(startPosition);
                movingDown = false;
                onReachedBottom?.Invoke();
            }
        }
    }

    void StopAt(Vector3 pos)
    {
        rb.MovePosition(pos);     // 마지막 스냅
        rb.linearVelocity = Vector3.zero;   // 혹시 모를 잔여 속도 제거(다른 힘 대비)
        rb.angularVelocity = Vector3.zero;
    }

    public void MoveUp()   { movingDown = false; movingUp   = true; }
    public void MoveDown() { movingUp   = false; movingDown = true; }
    public void Stop()     { movingUp = movingDown = false; }
}
