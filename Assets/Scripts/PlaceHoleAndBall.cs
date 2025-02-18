using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceGolfObjects : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject holePrefab;
    [SerializeField] private GameObject ballPrefab;

    //[SerializeField] private GameObject invisibleFloorPrefab;

    private ARRaycastManager _arRaycastManager;

    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    private GameObject placedHole;
    private GameObject placedBall;

    private enum PlacementStage
    {
        Hole,
        Ball,
        Done
    }
    private PlacementStage currentStage = PlacementStage.Hole;

    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();

        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void Update()
    {
        // Only proceed if there's exactly one touch
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            // If we've already placed both objects, do nothing (or handle differently if you like)
            if (currentStage == PlacementStage.Done) return;

            // Raycast into AR scene
            if (_arRaycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = _hits[0].pose;

                // For demonstration, let's keep it simple and just require a roughly horizontal plane
                if (IsSurfaceFlat(_hits[0]))
                {
                    if (currentStage == PlacementStage.Hole)
                    {
                        // First tap: place hole
                        placedHole = Instantiate(holePrefab, hitPose.position,
                            Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));

                        currentStage = PlacementStage.Ball; // Next tap will place the ball
                    }
                    else if (currentStage == PlacementStage.Ball)
                    {
                        // Second tap: place ball
                        placedBall = Instantiate(ballPrefab, hitPose.position,
                            Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                        //float floorY = hitPose.position.y;

                        // 2. Instantiate the large invisible floor at this Y
                        //GameObject floorCollider = Instantiate(invisibleFloorPrefab, hitPose.position, Quaternion.identity);
                        //floorCollider.transform.position = new Vector3(0, floorY, 0);

                        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;

                        currentStage = PlacementStage.Done; // We are done placing
                    }
                }
            }
        }
    }

    private bool IsSurfaceFlat(ARRaycastHit hit)
    {
        // The normal of a horizontal plane is (0,1,0). 0.95 allows slight tilt.
        return Vector3.Dot(hit.pose.up, Vector3.up) > 0.95f;
    }

    // Optionally, a reset method:
    public void ResetPlacement()
    {
        if (placedHole != null) Destroy(placedHole);
        if (placedBall != null) Destroy(placedBall);

        placedHole = null;
        placedBall = null;
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        currentStage = PlacementStage.Hole; // Start over at hole placement
    }
}