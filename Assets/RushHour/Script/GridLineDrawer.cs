using UnityEngine;

[ExecuteInEditMode]
public class GridLineDrawer : MonoBehaviour
{
    public float cellSize = 1f;
    public int gridCount = 6;
    public Material lineMaterial;

    void Start()
    {
        Vector3 origin = transform.position - new Vector3(gridCount * cellSize, 0, gridCount * cellSize) / 2;

        for (int i = 0; i <= gridCount; i++)
        {
            // 수직선
            CreateLine(origin + new Vector3(i * cellSize, 0.01f, 0), origin + new Vector3(i * cellSize, 0.01f, gridCount * cellSize));
            // 수평선
            CreateLine(origin + new Vector3(0, 0.01f, i * cellSize), origin + new Vector3(gridCount * cellSize, 0.01f, i * cellSize));
        }

    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = transform;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.useWorldSpace = true;
    }
}
