using UnityEngine;
using System.Collections;

public class SafeDoor : MonoBehaviour
{
    private bool isOpen = false;
    private Vector3 closedPosition;

    // Inspector에서 열릴 거리 지정 (예: -0.5f면 왼쪽으로 0.5만큼 이동)
    public Vector3 openOffset = new Vector3(0f, 0.7f, 0f);
    public float openDuration = 1.0f;  // 열리는 시간(초)

    void Start()
    {
        // 현재 위치를 "닫힌 위치"로 저장
        closedPosition = transform.localPosition;
    }

    public void OpenSafe()
    {
        if (isOpen) return;

        StartCoroutine(SlideOpen());  // 부드럽게 슬라이드 시작
        isOpen = true;
    }

    private IEnumerator SlideOpen()
    {
        float time = 0f;
        Vector3 start = transform.localPosition;
        Vector3 end = closedPosition + openOffset;

        while (time < 1f)
        {
            transform.localPosition = Vector3.Lerp(start, end, time);
            time += Time.deltaTime / openDuration;  // 속도 조절
            yield return null;
        }

        transform.localPosition = end;
    }
}

