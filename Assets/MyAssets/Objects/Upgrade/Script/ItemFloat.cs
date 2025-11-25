using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatHeight = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // vertical floating only
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
