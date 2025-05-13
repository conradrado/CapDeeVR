using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GunFire : MonoBehaviour
{
    [Header("총알 Prefab")]
    public GameObject bulletPrefab;

    [Header("총알 생성 위치 (총구 위치)")]
    public Transform firePoint;
    
    public TextMeshPro AmmoHud; 

    public int MaxAmmo = 6;
    private int CurrentAmmo = 6;

    [Header("총알 속도")]
    public float bulletForce = 20f;

    private void Update()
    {
        // RightController Trigger Input (Primary Button)
        if (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
        {
            Fire();
        }
        else if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // PC 테스트용 : Space 눌러도 발사
            Fire();
        }
    }

    private void Fire()
    {
        if (CurrentAmmo <= 0){
            AmmoHud.text = "RELOAD!";
            return;
        
        }

        if (bulletPrefab == null || firePoint == null)
            return;

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        CurrentAmmo -= 1;
        AmmoHud.text = CurrentAmmo + " / " + MaxAmmo;

        // 총알 Rigidbody에 힘을 줘서 발사
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
    }
}
