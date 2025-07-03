using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineInfiniteGenerator : MonoBehaviour
{
    public Transform player;
    public BikeSplineFollower playerFollower; // Reference to your follower script
    public int maxKnots = 10;
    public float knotSpacing = 10f;
    public bool autoRegenerateMesh = true;

    private SplineContainer splineContainer;
    private Spline spline;
    private float nextKnotDistance = 0f;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Spline;
    }

    private void Start()
    {
        if (spline.Count < 2)
        {
            // Ensure at least two knots to begin
            Spline initialSpline = new Spline();
            initialSpline.Add(new BezierKnot(Vector3.zero));
            initialSpline.Add(new BezierKnot(new Vector3(0, 0, knotSpacing)));
            splineContainer.Spline = initialSpline;
            spline = initialSpline;
        }

        nextKnotDistance = spline.GetLength(); // initial total distance
    }

    private void Update()
    {
        if (playerFollower == null || player == null) return;

        float traveledDistance = playerFollower.DistanceTravelledOnSpline(); // You might need to expose this
        float totalSplineLength = spline.GetLength();

        Debug.Log($"Player Traveled: {traveledDistance:F2}, NextKnotAt: {nextKnotDistance:F2}, TotalSplineLength: {totalSplineLength:F2}");

        // Add a new knot ahead of player
        if (traveledDistance + (2 * knotSpacing) > nextKnotDistance)
        {
            AddNewKnot();
            nextKnotDistance += knotSpacing;
        }

        // Remove old knots if too far behind
        if (spline.Count > maxKnots)
        {
            spline.RemoveAt(0);
            splineContainer.Spline = spline;
            RegenerateMeshIfNeeded();
        }

        // Manual test
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddNewKnot();
            Debug.Log($"Knots count after adding: {splineContainer.Spline.Count}");
        }
    }

    private void AddNewKnot()
    {
        spline = splineContainer.Spline; // fresh copy
        Vector3 lastPos = spline[spline.Count - 1].Position;
        Vector3 newPos = lastPos + new Vector3(0f, 0f, knotSpacing);

        spline.Add(new BezierKnot(newPos));
        splineContainer.Spline = spline;
        RegenerateMeshIfNeeded();
    }

    private void RegenerateMeshIfNeeded()
    {
        if (autoRegenerateMesh)
        {
            RoadMeshGenerator meshGen = GetComponent<RoadMeshGenerator>();
            if (meshGen != null)
            {
                meshGen.GenerateRoadMesh();
            }
        }
    }
}
