using UnityEngine;

public class Reload : MonoBehaviour
{
    [SerializeField] private GunFire gunFire;          // GunFire 참조
    [SerializeField] private Transform cylinderToEject; // 배출할 실린더 오브젝트
    [SerializeField] private AudioSource cylinderAudioSource;
    [SerializeField] private AudioClip cylinderAudioClip;
    
    [SerializeField] private Vector3 ejectForce = new Vector3(0, 2, -2);


    private void Start()
    {
        if (gunFire != null)
            gunFire.OnAmmoDepleted += EjectCasing;
    }

    private void OnDestroy()
    {
        if (gunFire != null)
            gunFire.OnAmmoDepleted -= EjectCasing;
    }

    public void SetCurrentCylinder(Transform newCylinder){
        cylinderToEject = newCylinder;
    }

    /// 탄피 배출
   private void EjectCasing()
{
    if (cylinderToEject != null)
    {
        cylinderAudioSource.PlayOneShot(cylinderAudioClip); // 배출 사운드 재생

        // 부모에서 분리
        cylinderToEject.SetParent(null);

        // 기존 Rigidbody가 있으면 재사용, 없으면 추가
        Rigidbody rb = cylinderToEject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = cylinderToEject.gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(transform.TransformDirection(ejectForce), ForceMode.Impulse);

        // Collider도 trigger 해제
        if (cylinderToEject.TryGetComponent(out Collider col))
        {
            col.isTrigger = false;
        }

        Destroy(cylinderToEject.gameObject, 1f);
    }
}

}
