using UnityEngine;

public class ShotgunReloadZone : MonoBehaviour
{
    public ShotgunController shotgun;

    private void OnTriggerEnter(Collider other)
    {
        // 탄약 상자는 "AmmoBox" 태그로 구분
        if (other.CompareTag("AmmoBox"))
        {
            if (shotgun != null)
                shotgun.Reload();

            // 탄약 상자 제거 (선택)
            Destroy(other.gameObject);
        }
    }
}
