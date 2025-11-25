using UnityEngine;

public class GunFollow : MonoBehaviour
{
    [Header("총이 따라갈 Target (RightHandController > GunSocket)")]
    public Transform target;

    [Header("따라가는 속도 (Position, Rotation)")]
    public float positionLerpSpeed = 20f;
    public float rotationLerpSpeed = 20f;

    void Update()
    {
        if (target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * positionLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * rotationLerpSpeed);
    }
}
