using UnityEngine;

public class HealingObject : MonoBehaviour
{
    [Header("회복량")]
    [SerializeField] private float healAmount = 15f;

    void OnCollisionEnter(Collision collision)
    {
        // 충돌한 상대가 TargetEntity이고, 체력이 가득 차지 않으면
        if (collision.gameObject.TryGetComponent(out PlayerStat target))
        {
            // 만약 TargetEntity가 만피라면 충돌을 무시
            if (target.isFull())
            {
                // 충돌을 무시
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                return; // 이후 코드 실행하지 않음
            }

            // 힐을 진행
            Debug.Log("힐!!");
            target.HealDamage(healAmount);

            // 만약 체력이 가득 차지 않으면 HealingObject를 파괴
            if (!target.isFull())
            {
                Destroy(gameObject);
            }
        }
    }
}