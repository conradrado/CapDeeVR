using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GunFire : MonoBehaviour
{
    [Header("총알 프리팹 / 총구")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("탄 속도 & 장탄수")]
    [SerializeField] private float bulletForce = 20f;
    [SerializeField] private int  maxAmmo      = 6;
    public event System.Action OnAmmoDepleted; // 총알 소진 시 Invoke 할 이벤트
    

    [Header("HUD")]
    [SerializeField] private TextMeshPro ammoHud;

    [Header("Animator")]
    [SerializeField] private Animator gunAnimator;

    [Header("Sound Effect")]
    [SerializeField] private AudioSource fireSFX;
    [SerializeField] private AudioClip gunShotClip;
    [SerializeField] private AudioClip gunSpin;

    [Header("총구 불빛 & 파티클 효과")]
    [SerializeField] private Light muzzleFlame;
    [SerializeField] private ParticleSystem muzzleParticle;

    private IEnumerator FlashMuzzle()
    {   
        muzzleFlame.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.15f); // 150ms 정도만 켜짐
        muzzleFlame.gameObject.SetActive(false);
    }



    private int currentAmmo;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;   // 현재 총을 잡은 인터랙터

    /* ---------- Life-cycle ---------- */
    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        currentAmmo = maxAmmo;

        // 총구 화염을 평시에는 꺼둠둠
        muzzleFlame.gameObject.SetActive(false); 

        // 총을 잡아 “Activate(트리거)” 입력이 들어오면 Fire() 실행
        grab.activated.AddListener(OnActivated);

        // 잡을 때 / 놓을 때 HUD 갱신
        grab.selectEntered.AddListener(_ => UpdateHud());
        grab.selectExited.AddListener(_ => UpdateHud());

    }

    private void OnDestroy()
    {
        grab.activated.RemoveListener(OnActivated);
    }

    /* ---------- 이벤트 ---------- */
    private void OnActivated(ActivateEventArgs args) {
            Fire();
            
            // 컨트롤러에 진동 추가 (강도 0~1, 지속시간 초 단위)
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor &&
            controllerInteractor.xrController != null)
            {
                controllerInteractor.xrController.SendHapticImpulse(0.7f, 0.15f); 
            }
    
    }

    /* ---------- 발사 로직 ---------- */
    private void Fire()
    {   
        /// 현재 탄약 없을 시
        if (currentAmmo <= 0)
        {
            OnAmmoDepleted?.Invoke(); // 탄약 없음 이벤트 Invoke. 해당 이벤트는 Reload 스크립트에 전달.
            ammoHud.text = "RELOAD!"; // HUD 업데이트

            // 실린더만 돌아가는 애니메이션 실행
            if(gunAnimator != null){
                gunAnimator.SetTrigger("Empty");
            }
            return;
        }

        // 총알 프리팹 or 총구 위치 둘 다 없으면 리턴턴
        if (!bulletPrefab || !firePoint) return;

        // 총알 생성 & 힘 주기
        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

        // 격발음 실행
        fireSFX.PlayOneShot(gunShotClip);
        

        // 총구 불빛 및 파티클 효과 재생
        FlashMuzzle();

        /*if (muzzleParticle != null){
            muzzleParticle.Play();
        }*/


        // 총기 애니메이션 실행
        if (gunAnimator != null)
        {   
            gunAnimator.SetTrigger("FireGun"); // Animator에서 "FireGun" 트리거 사용
        }
        currentAmmo--;
        UpdateHud();
        
    }

    /* ---------- 유틸 ---------- */
    public void Reload()          // Socket Holster 나 리로드 버튼에서 호출해도 됨
    {
        currentAmmo = maxAmmo;
        UpdateHud();
    }

    private void UpdateHud() =>
        ammoHud.text = $"{currentAmmo} / {maxAmmo}";
}
