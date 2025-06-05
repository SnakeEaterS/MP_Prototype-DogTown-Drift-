using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMeshGenerator : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float roadWidth = 2f;
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

    void GenerateRoadMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Spline spline = splineContainer.Spline;

        float totalLength = spline.GetLength();
        float step = 1f / resolution;

        float textureLengthUnit = 5f;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i * step;

            Vector3 pos = (Vector3)spline.EvaluatePosition(t);
            Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;

            Vector3 normal = Vector3.up;
            Vector3 binormal = Vector3.Cross(normal, tangent).normalized;

            Vector3 left = pos - binormal * (roadWidth / 2f);
            Vector3 right = pos + binormal * (roadWidth / 2f);

            vertices.Add(left);
            vertices.Add(right);

            // Adjust UVs to control tiling:
            uvs.Add(new Vector2(0, (t * totalLength) / textureLengthUnit));
            uvs.Add(new Vector2(1, (t * totalLength) / textureLengthUnit));
        }


        for (int i = 0; i < resolution; i++)
        {
            int baseIndex = i * 2;

            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);

            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
