using UnityEngine;

public class LaserSight : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private float laserDistance = 50f;
    [SerializeField] private LayerMask hitLayers;

    private LineRenderer lr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {  
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 start = muzzle.position;
        Vector3 direction = muzzle.forward;
        Vector3 end = start + direction * laserDistance;

        if (Physics.Raycast(start, direction, out RaycastHit hit, laserDistance, hitLayers))
        {
            end = hit.point;
        }        

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
