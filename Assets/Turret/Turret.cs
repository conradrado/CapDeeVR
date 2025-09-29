using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("포탑")]
    public Transform gunBody;    
    public Transform firePoint;  

    [Header("속성")]
    public float range = 10f;        // 攻击范围
    public float rotationSpeed = 5f; // 炮塔旋转速度
    public float fireRate = 1f;      // 射击间隔 (每秒几发)
    public GameObject bulletPrefab;  // 子弹预制体

    private Transform target;
    private float fireCooldown = 0f;

    void Update()
    {
        FindTarget();

        if (target != null)
        {
            RotateTowardsTarget();

            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= range)
        {
            target = player.transform;
        }
        else
        {
            target = null;
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 dir = target.position - gunBody.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        gunBody.rotation = Quaternion.Lerp(gunBody.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * 15f; // 子弹速度
        }

        Destroy(bullet, 5f); // 5秒后销毁子弹
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
