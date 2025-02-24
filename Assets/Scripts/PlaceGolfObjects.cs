using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Define placement modes.
public enum PlacementMode { None, PlacingFlag, PlacingGolf, PlacingCube }

public class PlaceGolfObjects : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject holePrefab;   // The flag/hole prefab.
    [SerializeField] private GameObject ballPrefab;   // The golf ball prefab.
    [SerializeField] private GameObject cubePrefab;   // cube obstacle prefab
    private ARRaycastManager _arRaycastManager;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    // References to the placed objects.
    private GameObject placedHole;
    private GameObject placedBall;

    private GameObject placedCube;
    private List<GameObject> cubeList = new List<GameObject>();
    // Current placement mode.
    public PlacementMode currentPlacementMode = PlacementMode.None;

    // UI Button references (assign these in the Inspector).
    [Header("UI Buttons")]
    public GameObject flagButton;
    public GameObject golfButton;
    public GameObject resetButton;
    public GameObject goButton;
    public GameObject cubeButton;

    public LayerMask groundLayer; // Assign the AR plane's layer here

    public LayerMask ballLayer; // Assign the ball's layer here
    private Boolean flagButtonPressed, golfButtonPressed, cubeButtonPressed;
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
        if (flagButtonPressed || golfButtonPressed || cubeButtonPressed)
        {
            flagButtonPressed = golfButtonPressed = cubeButtonPressed = false;
            return;
        }
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
                        int ballLayerNum = Mathf.RoundToInt(Mathf.Log(ballLayer.value, 2));
                        int groundLayerNum = Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2));
                        Physics.IgnoreLayerCollision(
                            ballLayerNum,
                            groundLayerNum,
                            false
                        );
                        placedBall = Instantiate(ballPrefab, hitPose.position,
                            Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingCube)
                    {
                        placedCube = Instantiate(cubePrefab, hitPose.position,
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
                    else if (currentPlacementMode == PlacementMode.PlacingCube && placedCube != null)
                    {
                        placedCube.transform.position = hitPose.position;
                        placedCube.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Finalize placement: hide the corresponding button. 
                    // Add the placedCube to the cubeList so more can be added
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
                    else if (currentPlacementMode == PlacementMode.PlacingCube)
                    {
                        cubeList.Add(placedCube);
                        //placedCube = null;                        
                    }
                    // Reset the mode so the user can choose the other item.
                    currentPlacementMode = PlacementMode.None;
                    isDragging = false;

                    // If flag and ball placed, show the Go button
                    if (placedHole != null && placedBall != null)
                    {

                        if (goButton != null)
                            goButton.SetActive(true);
                    }
                }
            }
        }
        flagButtonPressed = golfButtonPressed = cubeButtonPressed = false;
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
        flagButtonPressed = true;
        currentPlacementMode = PlacementMode.PlacingFlag;

    }

    // Called when the user taps the Golf button.
    public void OnGolfButtonPressed()
    {
        flagButtonPressed = true;
        currentPlacementMode = PlacementMode.PlacingGolf;
    }

    public void OnCubeButtonPressed()
    {
        cubeButtonPressed = true;
        currentPlacementMode = PlacementMode.PlacingCube;
        cubeList.Add(placedCube);
        placedCube = null;
    }
    // Called when the user taps the Reset button.
    public void ResetPlacement()
    {
        if (placedHole != null) Destroy(placedHole);
        if (placedBall != null) Destroy(placedBall);
        foreach (GameObject cube in cubeList)
        {
            if (cube != null)
            {
                Destroy(cube);
            }

        }
        cubeList.Clear();
        placedHole = null;
        placedBall = null;
        placedCube = null;
        currentPlacementMode = PlacementMode.None;

        // Show the placement buttons again and hide the Go button.
        if (flagButton != null)
            flagButton.SetActive(true);
        if (golfButton != null)
            golfButton.SetActive(true);
        if (cubeButton != null)
        {
            cubeButton.SetActive(true);
        }
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
        currentPlacementMode = PlacementMode.None;
        cubeButton.SetActive(false);
    }
}
//TODO: bug when you press one button then another
//probably because raycast going through button
//might be best to handle all touches in one script