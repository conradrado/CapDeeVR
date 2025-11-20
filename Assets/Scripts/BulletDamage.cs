using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    // Bullet lifetime and base damage
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 3f;

    public float Damage => damage;

    public void AddDamage(float additionalDamage)
    {
        damage += additionalDamage;
    }

    public void SetDamage(float newValue)
    {
        damage = newValue;
    }

    // Apply damage on collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out EnemyStat enemy))
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject, lifeTime);
    }
}
