using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("포탑")]
    public Transform gunBody; // 터렛의 총몸
    public Transform firePoint; // 총알이 나가게 될 총구 

    [Header("속성")]
    public float range = 10f; // 탐지 거리?    
    public float rotationSpeed = 5f; // 터렛 총몸이 플레이어를 향할 때의 속도
    public float fireRate = 1f; // 연사 속도
    public GameObject bulletPrefab; // 발사할 물체 (총알)

    private Transform target; // 타겟(플레이어)의 위치 (벡터값)
    private float fireCooldown = 0f; // 발사 후 쿨다운?

    void Update()
    {
        FindTarget(); // FindTarget() 함수를 항상 실행하여 플레이어를 탐지함

        // 플레이어 탐지 
        if (target != null) 
        {
            RotateTowardsTarget(); // 터렛 총몸 회전

            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }
    }

    // 플레이어를 탐지해내는 함수이다.
    void FindTarget()
    {
        
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // 플레이어 태그를 가진 물체, 즉 플레이어를 player에다 할당.
        if (player == null) return; // 플레이어가 탐지되지 않으면 리턴

        // 플레이어가 탐지되었을 떄 로직
        float distance = Vector3.Distance(transform.position, player.transform.position); // 터렛과 플레이어의 거리 distance
        

        if (distance <= range) // 탐지범위(range) 내에 플레이어가 들어옴
        {
            target = player.transform; // target을 플레이어의 위치로 바꿈
        }
        else // 플레이어가 탐지범위 밖으로 나감
        {
            target = null;
        }
    }

    // 총몸을 회전하는 함수
    void RotateTowardsTarget()
    {
        Vector3 dir = target.position - gunBody.position; // 목표위치 dir = 플레이어의 포지션
        Quaternion lookRotation = Quaternion.LookRotation(dir); // 쿼터니안 머시기로 부드러운 회전
        gunBody.rotation = Quaternion.Lerp(gunBody.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // 발사 함수
    void Shoot()
    {
        if (bulletPrefab == null) return; // 총알 프리팹 없으면 리턴

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation); // 총알 프리팹 인스턴스화
        Rigidbody rb = bullet.GetComponent<Rigidbody>(); // 총알의 rigidbody => rb
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * 15f; // 총알 rb에 velocity를 더함 (firepoint의 정면forward 방향으로 15f만큼)
        }

        Destroy(bullet, 5f); // 5초후 총알은 destroy
    }

    /*void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, range);
    }*/
}
