// PlayerRespawn.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    public Transform respawnPoint;     // �ǻ츱 ��ġ
    public DeathManager deathManager;  // ��ư ������ ����
    public bool useFade = false;       // ���ϸ� ���̵� ȿ�� ON
    public CanvasGroup fadeCanvas;     // ����(���̵�� CanvasGroup)

    [Header("Timings")]
    public float fadeOut = 0.4f;
    public float fadeIn = 0.4f;
    public float holdBlack = 0.1f;

    // ĳ��
    CharacterController cc;
    Rigidbody rb;
    LocomotionSystem loco;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        loco = GetComponentInChildren<LocomotionSystem>();
    }

    public void Respawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // 1) �̵�/���� ��� ����
        SetLocomotion(false);
        if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        // 2) (����) ���̵� �ƿ�
        if (useFade && fadeCanvas) yield return StartCoroutine(Fade(0f, 1f, fadeOut));

        // 3) ��ġ/ȸ�� �̵�
        Vector3 pos = respawnPoint ? respawnPoint.position : transform.position;
        Quaternion rot = respawnPoint ? respawnPoint.rotation : transform.rotation;

        if (cc) cc.enabled = false;                     // CharacterController�� �̵� �� ��Ȱ��
        transform.SetPositionAndRotation(pos, rot);
        yield return null;                              // �� ������ �����ָ� ����
        if (cc) cc.enabled = true;

        // 4) (����) ��� Ȧ�� �� ���̵� ��
        if (useFade && fadeCanvas)
        {
            yield return new WaitForSeconds(holdBlack);
            yield return StartCoroutine(Fade(1f, 0f, fadeIn));
        }

        // 5) GameOver UI ����
        if (deathManager)
        {
            if (deathManager.rstButton) deathManager.rstButton.SetActive(false);
            if (deathManager.quitButton) deathManager.quitButton.SetActive(false);
        }

        // 6) �̵� �簳
        SetLocomotion(true);
    }

    void SetLocomotion(bool on)
    {
        if (loco) loco.enabled = on;
        var providers = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(true);
        foreach (var p in providers) p.enabled = on;
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        if (!fadeCanvas) yield break;
        fadeCanvas.blocksRaycasts = true;  // ���̵� �� UI ����
        while (t < dur)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        fadeCanvas.alpha = to;
        fadeCanvas.blocksRaycasts = false;
    }
}
