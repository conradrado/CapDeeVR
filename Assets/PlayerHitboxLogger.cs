using UnityEngine;

public class PlayerHitboxLogger : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[PlayerHitBox] 충돌한 오브젝트: {collision.gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PlayerHitBox] 트리거 충돌: {other.gameObject.name}");
    }
}
