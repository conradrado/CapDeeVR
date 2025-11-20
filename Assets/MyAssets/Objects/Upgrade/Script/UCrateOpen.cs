using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class UCrateOpen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator crateAnimator;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private PlayerStat playerStat;
    [SerializeField] private Collider triggerCollider;
 
    [SerializeField] private XRRayInteractor rightHandRay;

    [Header("Upgrade Values")]
    [SerializeField] private int ammoIncreaseAmount = 2;
    [SerializeField] private float damageIncreaseAmount = 10f;
    [SerializeField] private float openDelay = 1f;

    [Header("Options")]
    [SerializeField] private bool showUiOnEnter = true;
    [SerializeField] private string playerTag = "Player";

    private bool _opened;
    private bool _isOpen;
    private Coroutine _openRoutine;

    void Awake()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (upgradeUI != null)
            upgradeUI.SetActive(false);

        if (rightHandRay == null && TutorialManager.Instance != null)
            rightHandRay = TutorialManager.Instance.rightRay;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_opened || !other.CompareTag(playerTag))
            return;

        // If gun is not manually assigned, try to find one on the player that entered.
        if (playerStat == null)
            playerStat = other.GetComponentInChildren<PlayerStat>();

        if (rightHandRay != null)
            rightHandRay.enabled = true;

        StartOpenRoutine();
    }

    void OnTriggerExit(Collider other)
    {
        if (_opened || !other.CompareTag(playerTag))
            return;

        StopOpenRoutine();

        if (rightHandRay != null)
            rightHandRay.enabled = false;
        CloseCrate();
    }

    void OpenCrate()
    {
        if (_opened || _isOpen)
            return;

        StopOpenRoutine();
        _isOpen = true;
        crateAnimator.SetBool("IsOpen",true);
        if (showUiOnEnter && upgradeUI != null)
            upgradeUI.SetActive(true);
    }

    void CloseCrate()
    {
        if(!_isOpen)
            return;

        _isOpen = false;
        crateAnimator.SetBool("IsOpen", false);
        
        if (showUiOnEnter && upgradeUI != null)
            upgradeUI.SetActive(false);
           
    }


    public void ChooseAmmoUpgrade()
    {
        if (_opened)
            return;

        if (playerStat != null)
            playerStat.AddAmmoBonus(ammoIncreaseAmount);

        CompleteUpgrade();
    }

    public void ChooseDamageUpgrade()
    {
        if (_opened)
            return;

        if (playerStat != null)
            playerStat.AddDamageBonus(damageIncreaseAmount);

        CompleteUpgrade();
    }

    void CompleteUpgrade()
    {
        _opened = true;

        if (upgradeUI != null)
            upgradeUI.SetActive(false);

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        if (rightHandRay != null)
            rightHandRay.enabled = false;

        Destroy(gameObject, 0.7f);
    }

    void StartOpenRoutine()
    {
        StopOpenRoutine();
        _openRoutine = StartCoroutine(OpenDelayed());
    }

    void StopOpenRoutine()
    {
        if (_openRoutine != null)
        {
            StopCoroutine(_openRoutine);
            _openRoutine = null;
        }
    }

    System.Collections.IEnumerator OpenDelayed()
    {
        float elapsed = 0f;
        while (elapsed < openDelay)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        _openRoutine = null;
        OpenCrate();
    }
}
