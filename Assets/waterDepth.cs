using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class WaterDepth : MonoBehaviour
{
    [Header("Depth Parameter")]
    [SerializeField] private Transform mainCamera;  // XR Main Camera
    [SerializeField] private float depth = 0;

    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private VolumeProfile surfacePostProcessing;
    [SerializeField] private VolumeProfile underwaterPostProcessing;


    private void Update()
    {
        if (mainCamera.position.y < depth)
        {
            EnableEffect(true);
        }
        else
        {
            EnableEffect(false);
        }
    }

    private void EnableEffect(bool active)
    {
        // Volume 활성화 및 프로파일 설정
        postProcessingVolume.enabled = true;  // Volume 활성화
        postProcessingVolume.priority = 10f;  // Volume 우선순위 설정

        // 프로파일 변경
        if (active)
        {
            Debug.Log("Underwater!!");
            RenderSettings.fog = true;
            
            postProcessingVolume.profile = underwaterPostProcessing;
        }
        else
        {   
            
            Debug.Log("Surface!");
            RenderSettings.fog = false;
            postProcessingVolume.profile = surfacePostProcessing;
        }
    }
}
