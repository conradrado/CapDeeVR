using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class bodySocket
{
    public GameObject gameObject;
    [Range(0.01f, 1f)]
    public float heightRatio;
}

public class BodySocketInventory : MonoBehaviour
{
    public GameObject HMD;
    public bodySocket[] bodySockets;

    private Vector3 _currentHMDlocalPosition;
    private Quaternion _currentHMDRotation;

    void Update()
    {
        _currentHMDlocalPosition = HMD.transform.localPosition;
        _currentHMDRotation = HMD.transform.rotation;
        
        // 1. BodySocketInventory 오브젝트의 위치를 HMD의 X, Z 평면에 고정
        UpdateSocketInventory();

        // 2. 각 bodySockets의 위치를 HMD의 높이를 기준으로 조정
        foreach (var bodySocket in bodySockets)
        {
            UpdateBodySocketHeight(bodySocket);
        }
    }

    private void UpdateSocketInventory()
    {
        // BodySocketInventory의 Y 좌표는 항상 0으로 고정 (기준점 역할)
        transform.localPosition = new Vector3(_currentHMDlocalPosition.x, (_currentHMDlocalPosition.y -0.29f), _currentHMDlocalPosition.z);
        
        // 회전은 HMD의 Y축 회전만 따라가도록
        transform.rotation = new Quaternion(transform.rotation.x, _currentHMDRotation.y, transform.rotation.z, _currentHMDRotation.w);
    }

    private void UpdateBodySocketHeight(bodySocket bodySocket)
    {
        // bodySocket의 Y 좌표를 HMD 높이에 비례하여 설정
        // 이 때 기준점은 Y=0인 BodySocketInventory가 됨
        bodySocket.gameObject.transform.localPosition = new Vector3(
            bodySocket.gameObject.transform.localPosition.x,
            _currentHMDlocalPosition.y * bodySocket.heightRatio, 
            bodySocket.gameObject.transform.localPosition.z
        );
    }
}