using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Define placement modes.
public enum PlacementMode { None, PlacingFlag, PlacingGolf }

public class PlaceGolfObjects : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject holePrefab;   // The flag/hole prefab.
    [SerializeField] private GameObject ballPrefab;   // The golf ball prefab.

    private ARRaycastManager _arRaycastManager;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    // References to the placed objects.
    private GameObject placedHole;
    private GameObject placedBall;

    // Current placement mode.
    public PlacementMode currentPlacementMode = PlacementMode.None;

    // UI Button references (assign these in the Inspector).
    [Header("UI Buttons")]
    public GameObject flagButton;
    public GameObject golfButton;
    public GameObject resetButton;
    public GameObject goButton;

    private bool isDragging = false;

    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    private void Update()
    {
        // Only process placement if we're in a placement mode.
        if (currentPlacementMode == PlacementMode.None) return;
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        // Raycast into the AR scene.
        if (_arRaycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = _hits[0].pose;

            // Check that the surface is roughly horizontal.
            if (IsSurfaceFlat(_hits[0]))
            {
                if (touch.phase == TouchPhase.Began)
                {
                    // Begin drag: instantiate a preview object if not already created.
                    if (currentPlacementMode == PlacementMode.PlacingFlag && placedHole == null)
                    {
                        placedHole = Instantiate(holePrefab, hitPose.position,
                            Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingGolf && placedBall == null)
                    {
                        placedBall = Instantiate(ballPrefab, hitPose.position,
                            Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                    }
                    isDragging = true;
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    // Update the preview object's position as the user drags.
                    if (currentPlacementMode == PlacementMode.PlacingFlag && placedHole != null)
                    {
                        placedHole.transform.position = hitPose.position;
                        placedHole.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingGolf && placedBall != null)
                    {
                        placedBall.transform.position = hitPose.position;
                        placedBall.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Finalize placement: hide the corresponding button.
                    if (currentPlacementMode == PlacementMode.PlacingFlag)
                    {
                        if (flagButton != null)
                            flagButton.SetActive(false);
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingGolf)
                    {
                        if (golfButton != null)
                            golfButton.SetActive(false);
                    }
                    // Reset the mode so the user can choose the other item.
                    currentPlacementMode = PlacementMode.None;
                    isDragging = false;

                    // If both objects are placed, show the Go button.
                    if (placedHole != null && placedBall != null)
                    {
                        if (goButton != null)
                            goButton.SetActive(true);
                    }
                }
            }
        }
    }

    // Check if the ARRaycastHit represents a nearly horizontal surface.
    private bool IsSurfaceFlat(ARRaycastHit hit)
    {
        return Vector3.Dot(hit.pose.up, Vector3.up) > 0.95f;
    }

    // --- UI Button Callbacks ---

    // Called when the user taps the Flag button.
    public void OnFlagButtonPressed()
    {
        currentPlacementMode = PlacementMode.PlacingFlag;
    }

    // Called when the user taps the Golf button.
    public void OnGolfButtonPressed()
    {
        currentPlacementMode = PlacementMode.PlacingGolf;
    }

    // Called when the user taps the Reset button.
    public void ResetPlacement()
    {
        if (placedHole != null) Destroy(placedHole);
        if (placedBall != null) Destroy(placedBall);
        placedHole = null;
        placedBall = null;
        currentPlacementMode = PlacementMode.None;

        // Show the placement buttons again and hide the Go button.
        if (flagButton != null)
            flagButton.SetActive(true);
        if (golfButton != null)
            golfButton.SetActive(true);
        if (goButton != null)
            goButton.SetActive(false);
    }

    // Called when the user taps the Go button.
    public void OnGoButtonPressed()
    {
        // Here you could enable the DragShoot component on the ball to let the user shoot it.
        // For example, if the ball prefab has a DragShoot script that is disabled by default:
        DragShoot ds = placedBall.GetComponent<DragShoot>();
        if (ds != null)
        {
            ds.enabled = true;
            goButton.SetActive(false);
        }
        // Optionally hide the UI or disable further placement.
    }
}