using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class ShotgunController : MonoBehaviour
{
    [Header("Pump (슬라이드) 관련")]
    public Transform pumpTransform;       // 펌프 오브젝트 Transform (PumpHandle)
    public Transform slideStart;          // PumpSlide에서 사용한 SlideStart (총 자식)
    public Transform slideEnd;            // PumpSlide에서 사용한 SlideEnd (총 자식)

    [Header("탄/배출 설정")]
    public int maxAmmo;                   // 탄창 총알 수 (실제 장전 가능한 수)
    public GameObject shellPrefab;        // 배출할 탄피 프리팹 (Rigidbody 필요)
    public Transform ejectPoint;          // 탄피 스폰 위치(총의 배출구)
    public float ejectForce = 1.5f;
    public Vector3 ejectTorque = new Vector3(0.2f, 0.6f, 0.2f);
    private int ammo;

    [Header("사운드")]
    public AudioClip pumpBackClip;        // 펌프 완전 후퇴(철컥) 사운드
    public AudioClip pumpForwardClip;     // 펌프 완전 전진(철컥) 사운드
    public AudioClip fireClip;            // 발사 사운드
    public AudioClip emptyClickClip;      // 탄 없음 클릭음 (선택)

    [Header("발사(선택)")]
    public Transform muzzlePoint;         // 발사 이펙트/발사체 스폰 위치 (optional)
    public GameObject projectilePrefab;   // 발사체(선택) - 없으면 단순 사운드만
    
    public int pellets = 8;                 // 발사되는 탄 수
    public float spread = 5f;          // 퍼짐 정도 (도 단위)
    public float bulletSpeed = 80f;         // 총알 속도
    public GameObject muzzleFlashPrefab;    // Fire3D 머티리얼 적용된 이펙트 (선택)

    [Header("장탄수 표시 및 재장전")]
    [SerializeField] private TextMeshPro ammoHud;
    public AudioClip reloadClip;                // 리로드 사운드
    public Collider reloadZoneCollider;         // 장전 영역 콜라이더

    [Header("파티클 이펙트")]
    public ParticleSystem muzzleFlame;   // 발사 화염 파티클 (Hierarchy 내 Particle System)


    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    [Header("설정(민감도)")]
    [Range(0f, 0.1f)] public float backThreshold = 0.02f;   // 후퇴 판정 (near 0)
    [Range(0f, 0.1f)] public float forwardThreshold = 0.98f; // 전진 판정 (near 1)

    // 내부 상태
    public bool isChambered { get; private set; } = false;
    private bool wasBack = false;
    private bool wasForward = false;

    private AudioSource audioSource;

    // 이벤트(외부에서 리스닝 가능)
    public UnityEvent onShellEjected;
    public UnityEvent onChambered;
    public UnityEvent onFired;

    void Awake()
    {
        ammo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // 🔹 이벤트 연결
        grab.activated.AddListener(OnActivated);
    }

    void Start()
    {
        // 안전 확인
        if (pumpTransform == null || slideStart == null || slideEnd == null)
        {
            Debug.LogError("[ShotgunController] pumpTransform/slideStart/slideEnd 를 모두 할당하세요.");
            enabled = false;
            return;
        }

        // 초기 상태 (만약 펌프 전진 상태로 시작하면 chambered 할 수 있음)
        float norm = GetPumpNormalized();
        wasBack = norm <= backThreshold;
        wasForward = norm >= forwardThreshold;
        if (wasForward) isChambered = true;
    }

    void Update()
    {
        ammoHud.text = ammo + " / 8";

        float norm = GetPumpNormalized();

        // ---- 후퇴 판정 (edge) ----
        if (norm <= backThreshold)
        {
            if (!wasBack)
            {
                // 처음 후퇴 도달 시 동작
                HandlePumpBack();
                wasBack = true;
                wasForward = false; // reset forward edge until moves again
            }
        }
        else
        {
            wasBack = false;
        }

        // ---- 전진 판정 (edge) ----
        if (norm >= forwardThreshold)
        {
            if (!wasForward)
            {
                HandlePumpForward();
                wasForward = true;
                wasBack = false;
            }
        }
        else
        {
            wasForward = false;
        }
    }

    // 펌프 정규화: 0..1 (0 = slideEnd, 1 = slideStart) — 자동으로 min/max 정렬
    float GetPumpNormalized()
    {
        float zMin = Mathf.Min(slideStart.localPosition.z, slideEnd.localPosition.z);
        float zMax = Mathf.Max(slideStart.localPosition.z, slideEnd.localPosition.z);
        float currZ = pumpTransform.localPosition.z;
        if (Mathf.Approximately(zMax, zMin)) return 0f;
        return Mathf.InverseLerp(zMin, zMax, currZ);
    }

    void HandlePumpBack()
    {
        // 후퇴시: 탄피 배출 + ammo 감소
        if (ammo > 0)
        {
            // 탄피 배출 시점에 ammo 감소 (사용자 요구에 따름)
            ammo = Mathf.Max(0, ammo - 1);

            // 탄피 스폰
            if (shellPrefab != null && ejectPoint != null)
            {
                var shell = Instantiate(shellPrefab, ejectPoint.position, ejectPoint.rotation);
                var rb = shell.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 fwd = (ejectPoint.forward * ejectForce) + (Random.insideUnitSphere * 0.2f);
                    rb.AddForce(fwd, ForceMode.Impulse);
                    rb.AddTorque(new Vector3(
                        Random.Range(-ejectTorque.x, ejectTorque.x),
                        Random.Range(-ejectTorque.y, ejectTorque.y),
                        Random.Range(-ejectTorque.z, ejectTorque.z)
                    ), ForceMode.Impulse);
                }
            }

            // 사운드
            if (pumpBackClip != null) audioSource.PlayOneShot(pumpBackClip);

            // 이벤트
            onShellEjected?.Invoke();
        }
        else
        {
            // 탄 없음인데 후퇴하면 그냥 소리 재생시킬 수도 있음
            if (pumpBackClip != null) audioSource.PlayOneShot(pumpBackClip);
        }
    }

    void HandlePumpForward()
    {
        // 전진시: chambered true
        isChambered = true;
        if (pumpForwardClip != null) audioSource.PlayOneShot(pumpForwardClip);
        onChambered?.Invoke();
    }

        /* ---------- 발사 이벤트 ---------- */
    private void OnActivated(ActivateEventArgs args)
    {
        Fire();

        // 🔸 컨트롤러에 진동 피드백
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor &&
            controllerInteractor.xrController != null)
        {
            controllerInteractor.xrController.SendHapticImpulse(0.7f, 0.15f);
        }
    }

    // 외부에서 호출하는 발사 함수 (예: 트리거 입력)
    public void Fire()
    {
        if (!isChambered)
        {
            if (emptyClickClip != null) audioSource.PlayOneShot(emptyClickClip);
            return;
        }

        if (ammo <= 0)
        {
            if (emptyClickClip != null) audioSource.PlayOneShot(emptyClickClip);
            return;
        }

        // 🔥 발사 사운드
        if (fireClip != null) audioSource.PlayOneShot(fireClip);

        // 🔥 총구 파티클 재생
        if (muzzleFlame != null)
            muzzleFlame.Play();

        // 🔸 산탄 발사 (혹은 단발)
        if (projectilePrefab != null && muzzlePoint != null)
        {   
            for (int i = 0; i < pellets; i++)
            {
                // 무작위 방향 벡터(원뿔 형태) 생성
                Vector3 randomSpread = muzzlePoint.forward +
                                    Random.insideUnitSphere * spread;

                randomSpread.Normalize(); // 방향성 유지

                var bullet = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(randomSpread));
                var rb = bullet.GetComponent<Rigidbody>();

                if (rb != null)
                    rb.linearVelocity = randomSpread * bulletSpeed;
            }
        }

        ammo--;
        isChambered = false;
        onFired?.Invoke();
    }


    public void Reload()
    {
    ammo = maxAmmo;
    if (reloadClip != null)
        audioSource.PlayOneShot(reloadClip);

    Debug.Log($"🔫 리로드 완료: {ammo}발 장전됨");
    }

}
