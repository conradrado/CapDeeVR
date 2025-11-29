using UnityEngine;
using System.Collections;

public class PuzzleGateController : MonoBehaviour
{
    [Header("Cover Settings")]
    public GameObject coverBox;
    public bool animateLift = true;
    public float liftHeight = 1.2f;
    public float liftSpeed = 2f;

    [Header("Optional FX")]
    public AudioSource sfx;
    public ParticleSystem revealFx;

    bool revealed;

    public void RevealCover()
    {
        if (revealed || !coverBox) return;
        revealed = true;
        if (revealFx) revealFx.Play();
        if (sfx) sfx.Play();
        if (animateLift) StartCoroutine(LiftAndDisable());
        else coverBox.SetActive(false);
    }

    public void HideCover()   // 되돌림이 필요 없으면 안 써도 됨
    {
        if (!revealed || !coverBox) return;
        revealed = false;
        coverBox.SetActive(true);
    }

    IEnumerator LiftAndDisable()
    {
        var tr = coverBox.transform;
        var start = tr.position;
        var target = start + Vector3.up * liftHeight;
        while (Vector3.Distance(tr.position, target) > 0.01f)
        {
            tr.position = Vector3.Lerp(tr.position, target, Time.deltaTime * liftSpeed);
            yield return null;
        }
        tr.position = target;
        coverBox.SetActive(false);
    }
}
