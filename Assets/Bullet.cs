using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("총알 유지 시간 (초)")]
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
