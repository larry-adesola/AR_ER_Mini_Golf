using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARPlaneManager))]
public class FloorPlaneSelector : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject holePrefab;
    [SerializeField] private GameObject ballPrefab;

    [Header("Plane Selection Settings")]
    [SerializeField] private float minPlaneArea = 0.5f;   // in m², e.g. 0.5
    [SerializeField] private float maxDistance = 3f;      // in meters
    [SerializeField] private float yOffsetBelowCamera = 0.1f; // plane should be at least 0.1m below camera

    private ARPlaneManager planeManager;
    private Camera arCamera;

    private bool hasPlacedObjects = false;

    private GameObject placedHole;
    private GameObject placedBall;

    private void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = Camera.main;
        planeManager.planesChanged += OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (hasPlacedObjects) return; // Already placed, do nothing

        // Instead of just checking args.added/updated, let's look at all known planes
        foreach (ARPlane plane in planeManager.trackables)
        {
            if (IsValidFloorPlane(plane))
            {
                // This plane meets our criteria
                PlaceObjectsOnPlane(plane);

                hasPlacedObjects = true;

                // Optionally disable plane detection once found:
                // planeManager.requestedDetectionMode = PlaneDetectionMode.None;
                break;
            }
        }
    }

    private bool IsValidFloorPlane(ARPlane plane)
    {
        // 1) Check alignment
        if (plane.alignment != PlaneAlignment.HorizontalUp) return false;

        // 2) Check area
        float area = (2 * plane.extents.x) * (2 * plane.extents.y);
        if (area < minPlaneArea) return false;

        // 3) Check distance from camera
        Vector3 planeCenterWorld = plane.transform.TransformPoint(plane.center);
        float distToCamera = Vector3.Distance(planeCenterWorld, arCamera.transform.position);
        if (distToCamera > maxDistance) return false;

        // 4) Check if plane is below camera by some margin
        float cameraY = arCamera.transform.position.y;
        if (planeCenterWorld.y > cameraY - yOffsetBelowCamera) return false;

        return true;
    }

    private void PlaceObjectsOnPlane(ARPlane plane)
    {
        Vector3 planeCenter = plane.transform.TransformPoint(plane.center);

        // For demonstration, place them side by side
        Vector3 holePos = planeCenter + plane.transform.right * -0.1f;
        Vector3 ballPos = planeCenter + plane.transform.right * 0.1f;

        placedHole = Instantiate(holePrefab, holePos, Quaternion.identity);
        placedBall = Instantiate(ballPrefab, ballPos, Quaternion.identity);

        // Force them at the same Y if you want them perfectly level
        float floorY = planeCenter.y;
        placedHole.transform.position = new Vector3(holePos.x, floorY, holePos.z);
        placedBall.transform.position = new Vector3(ballPos.x, floorY, ballPos.z);

        Debug.Log("Placed hole & ball on a valid floor plane!");
    }

    public void ResetPlacement()
    {
        if (placedHole != null) Destroy(placedHole);
        if (placedBall != null) Destroy(placedBall);

        placedHole = null;
        placedBall = null;

        hasPlacedObjects = false;

        // Re-enable plane detection if you disabled it
        // planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        Debug.Log("Reset. Waiting for next valid floor plane...");
    }
}