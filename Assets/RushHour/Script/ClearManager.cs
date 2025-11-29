using UnityEngine;
using System.Collections;

public class ClearManager : MonoBehaviour
{
    public GameObject clearMessage;

    private void Start()
    {
        // 게임 시작 시 메시지 비활성화
        clearMessage.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            Debug.Log("클리어!");
            StartCoroutine(ShowClearMessage());
        }
    }

    IEnumerator ShowClearMessage()
    {
        clearMessage.SetActive(true);
        yield return new WaitForSeconds(4f); // 4초간 유지
        clearMessage.SetActive(false);
    }
}
