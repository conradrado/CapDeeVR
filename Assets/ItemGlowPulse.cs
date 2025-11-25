using UnityEngine;

public class ItemGlowPulse : MonoBehaviour
{
    public Color baseEmissionColor = Color.cyan; 
    public float pulseSpeed = 2f;
    public float pulseStrength = 1.5f;

    Material mat;
    Color originalColor;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        // Emission 켜기
        mat.EnableKeyword("_EMISSION");
        originalColor = baseEmissionColor;
    }

    void Update()
    {
        float emission = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        Color finalColor = originalColor * (1 + emission * pulseStrength);

        mat.SetColor("_EmissionColor", finalColor);
    }
}
