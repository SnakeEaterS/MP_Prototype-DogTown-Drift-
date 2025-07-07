using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMeshGenerator : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float roadWidth = 2f;
    public float roadThickness = 0.2f;
    public int resolution = 100;

    private void Awake()
    {
        if (splineContainer == null)
        {
            splineContainer = GetComponent<SplineContainer>();
        }
    }

    private void Start()
    {
        if (splineContainer != null)
        {
            GenerateRoadMesh();
        }
        else
        {
            Debug.LogError("SplineContainer not assigned.");
        }
    }

    public void GenerateRoadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedRoadMesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Spline spline = splineContainer.Spline;

        float totalLength = spline.GetLength();
        float step = 1f / resolution;
        float texScale = 5f;

        List<Vector3> topLefts = new();
        List<Vector3> topRights = new();
        List<Vector3> bottomLefts = new();
        List<Vector3> bottomRights = new();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i * step;
            Vector3 pos = (Vector3)spline.EvaluatePosition(t);
            Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;
            Vector3 binormal = Vector3.Cross(Vector3.up, tangent).normalized;

            Vector3 left = pos - binormal * (roadWidth / 2f);
            Vector3 right = pos + binormal * (roadWidth / 2f);
            Vector3 leftBottom = left - Vector3.up * roadThickness;
            Vector3 rightBottom = right - Vector3.up * roadThickness;

            topLefts.Add(left);
            topRights.Add(right);
            bottomLefts.Add(leftBottom);
            bottomRights.Add(rightBottom);
        }

        // Add all vertices (top + bottom + sides)
        vertices.AddRange(topLefts);     // 0
        vertices.AddRange(topRights);    // resolution+1
        vertices.AddRange(bottomLefts);  // 2*(resolution+1)
        vertices.AddRange(bottomRights); // 3*(resolution+1)

        int row = resolution + 1;

        // Top surface
        for (int i = 0; i < resolution; i++)
        {
            int tl = i;
            int tr = i + row;

            triangles.Add(tl);
            triangles.Add(tr);
            triangles.Add(tl + 1);

            triangles.Add(tl + 1);
            triangles.Add(tr);
            triangles.Add(tr + 1);
        }

        // Bottom surface (reversed winding)
        for (int i = 0; i < resolution; i++)
        {
            int bl = i + row * 2;
            int br = i + row * 3;

            triangles.Add(bl);
            triangles.Add(bl + 1);
            triangles.Add(br);

            triangles.Add(bl + 1);
            triangles.Add(br + 1);
            triangles.Add(br);
        }

        // Left side wall
        for (int i = 0; i < resolution; i++)
        {
            int topA = i;
            int botA = i + row * 2;

            triangles.Add(botA);
            triangles.Add(topA + 1);
            triangles.Add(topA);

            triangles.Add(botA);
            triangles.Add(botA + 1);
            triangles.Add(topA + 1);
        }

        // Right side wall
        for (int i = 0; i < resolution; i++)
        {
            int topB = i + row;
            int botB = i + row * 3;

            triangles.Add(topB);
            triangles.Add(topB + 1);
            triangles.Add(botB);

            triangles.Add(botB);
            triangles.Add(topB + 1);
            triangles.Add(botB + 1);
        }

        // UVs only for the top surface
        for (int i = 0; i <= resolution; i++)
        {
            float v = (i / (float)resolution) * (totalLength / texScale);
            uvs.Add(new Vector2(0, v)); // left top
        }
        for (int i = 0; i <= resolution; i++)
        {
            float v = (i / (float)resolution) * (totalLength / texScale);
            uvs.Add(new Vector2(1, v)); // right top
        }

        // Optional: duplicate top UVs for bottom and sides
        for (int i = 0; i < (row * 2); i++)
        {
            uvs.Add(uvs[i % uvs.Count]);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
