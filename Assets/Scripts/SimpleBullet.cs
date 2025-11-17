using UnityEngine;

// Minimal projectile that damages TargetEntity on trigger hit
public class SimpleBullet : MonoBehaviour
{
    public float damage = 8f;
    public float life = 5f;
    public LayerMask hitMask = ~0;

    void Start()
    {
        Destroy(gameObject, life);
    }

    void OnTriggerEnter(Collider other)
    {
        // Optional: filter by layer mask
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        if (other.TryGetComponent<TargetEntity>(out var target))
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
        else
        {
            // If it hits environment, optionally destroy
            Destroy(gameObject);
        }
    }
}

