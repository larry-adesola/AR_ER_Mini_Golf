using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.VisualScripting;
using UnityEngine.XR.ARSubsystems;

// Define placement modes.
public enum PlacementMode { None, PlacingFlag, PlacingGolf, PlacingCube, PlacingAnchor}

public class PlaceGolfObjects : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject holePrefab;   // The flag/hole prefab.
    [SerializeField] private GameObject ballPrefab;   // The golf ball prefab.
    [SerializeField] private GameObject cubePrefab;   // cube obstacle prefab
    private ARRaycastManager _arRaycastManager;
    private ARPlaneManager _arPlaneManager;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    // References to the placed objects.
    private GameObject placedHole;
    private GameObject placedBall;
    private GameObject placedCube;

    private Vector3 holeOffset = new Vector3(0f,-0.03f,0f);
    private Vector3 ballOffset = new Vector3(0f,0.02f,0f);
    private Vector3 cubeOffset = new Vector3(0f,0f,0f);
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
    public GameObject anchorButton;
    public Slider widthSlider;
    public GameObject completeLabel;
    public GameObject StrokeCountText;
    public LayerMask groundLayer; // Assign the AR plane's layer here

    public LayerMask ballLayer; // Assign the ball's layer here
    private bool buttonPressed;
    private bool isDragging = false;
    public GameObject groundPlane;
    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
        _arPlaneManager = GetComponent<ARPlaneManager>();
    }
    private void Update()
    {

        // Only process placement if we're in a placement mode.
        if (currentPlacementMode == PlacementMode.None) return;

        //if button pressed this frame, do not place objects
        if(buttonPressed){
            buttonPressed = false;
            return;
        }
        // Check for touch input.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandlePlacement(touch.position, touch.phase);
        }
        // Check for mouse input.
        else if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            // Simulate touch phases for mouse input
            TouchPhase phase = TouchPhase.Canceled;

            if (Input.GetMouseButtonDown(0))
                phase = TouchPhase.Began;
            else if (Input.GetMouseButton(0))
                phase = TouchPhase.Moved;
            else if (Input.GetMouseButtonUp(0))
                phase = TouchPhase.Ended;
            HandlePlacement(Input.mousePosition, phase);
        }
    }

    private void HandlePlacement(Vector2 inputPosition, TouchPhase phase)
    {
        // Raycast into the AR scene.
        Pose hitPose = new Pose();
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        bool raycast = Physics.Raycast(ray, out RaycastHit hit);

        if (raycast){
            hitPose = new Pose(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        }else{
            raycast = _arRaycastManager.Raycast(inputPosition, _hits, TrackableType.PlaneWithinPolygon);
            if(raycast) hitPose = _hits[0].pose;
        }
        
        if (raycast)
        {
            print(hitPose);
            // Check that the surface is roughly horizontal.
            if (IsSurfaceFlat(hitPose))
            {
                if (phase == TouchPhase.Began)
                {
                    // Begin drag: instantiate a preview object if not already created.
                    if (currentPlacementMode == PlacementMode.PlacingFlag && placedHole == null)
                    {
                        placedHole = Instantiate(holePrefab, hitPose.position + holeOffset, Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                        foreach (Collider col in placedHole.GetComponentsInChildren<Collider>()){col.enabled = false;}
                        MiniGolfHole triggerScript = placedHole.transform.Find("Trigger").gameObject.GetComponent<MiniGolfHole>();
                        triggerScript.completeLabel = completeLabel;
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingGolf && placedBall == null)
                    {
                        int ballLayerNum = Mathf.RoundToInt(Mathf.Log(ballLayer.value, 2));
                        int groundLayerNum = Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2));
                        Physics.IgnoreLayerCollision(ballLayerNum, groundLayerNum, false);
                        placedBall = Instantiate(ballPrefab, hitPose.position + ballOffset, Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                        placedBall.GetComponent<Collider>().enabled = false;
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingCube)
                    {
                        float size = widthSlider.value / 12;
                        cubeOffset = Vector3.up * (size/2);
                        placedCube = Instantiate(cubePrefab, hitPose.position + cubeOffset, Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));                   
                        placedCube.transform.localScale = new Vector3(size, size, size);
                        placedCube.GetComponent<Collider>().enabled = false;
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingAnchor){
                        //ARPlane hitPlane = _hits[0].trackable as ARPlane;
                        //hitPlane.AddComponent<ARAnchor>();   
                        foreach (var plane in _arPlaneManager.trackables){
                            Destroy(plane.gameObject); // Remove each detected plane
                        }
                        groundPlane.SetActive(true);
                        _arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
                        groundPlane.transform.SetWorldPose(hitPose);
                        print("Ground Plane set");
                    } 
                    isDragging = true;
                }
                else if (phase == TouchPhase.Moved || phase == TouchPhase.Stationary)
                {
                    // Update the preview object's position as the user drags.
                    if (currentPlacementMode == PlacementMode.PlacingFlag && placedHole != null)
                    {
                        placedHole.transform.position = hitPose.position + holeOffset;
                        placedHole.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingGolf && placedBall != null)
                    {
                        placedBall.transform.position = hitPose.position + ballOffset;
                        placedBall.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingCube && placedCube != null)
                    {
                        placedCube.transform.position = hitPose.position + cubeOffset;
                        placedCube.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                    }
                }
                else if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                {
                    // Finalize placement: hide the corresponding button. 
                    // Add the placedCube to the cubeList so more can be added
                    if (currentPlacementMode == PlacementMode.PlacingFlag)
                    {
                        if (flagButton != null && placedHole != null)
                            flagButton.SetActive(false);
                        foreach (Collider col in placedHole.GetComponentsInChildren<Collider>()){col.enabled = true;}
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingGolf)
                    {
                        if (golfButton != null && placedBall != null)
                            golfButton.SetActive(false);
                        placedBall.GetComponent<Collider>().enabled = true;
                    }
                    else if (currentPlacementMode == PlacementMode.PlacingCube)
                    {
                        cubeList.Add(placedCube);
                        placedCube.GetComponent<Collider>().enabled = true;
                    }

                    // Reset the mode so the user can choose the other item.
                    currentPlacementMode = PlacementMode.None;
                    isDragging = false;

                    // If flag and ball placed, show the Go button
                    if (placedHole != null && placedBall != null)
                    {
                        cubeButton.SetActive(false);
                        widthSlider.gameObject.SetActive(false);
                        if (goButton != null)
                            goButton.SetActive(true);
                    }
                }
            }
        }
    }

    // Check if the ARRaycastHit represents a nearly horizontal surface.
    private bool IsSurfaceFlat(Pose hit)
    {
        return Vector3.Dot(hit.up, Vector3.up) > 0.95f;
    }

    // --- UI Button Callbacks ---

    // Called when the user taps the Flag button.
    public void OnFlagButtonPressed()
    {
        buttonPressed = true;
        currentPlacementMode = PlacementMode.PlacingFlag;

    }

    // Called when the user taps the Golf button.
    public void OnGolfButtonPressed()
    {
        buttonPressed = true;
        currentPlacementMode = PlacementMode.PlacingGolf;
    }

    public void OnCubeButtonPressed()
    {
        buttonPressed = true;
        currentPlacementMode = PlacementMode.PlacingCube;
        cubeList.Add(placedCube);
        placedCube = null;
    }
    // Called when the user taps the Reset button.
    public void ResetPlacement()
    {
        buttonPressed = true;
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
        if (widthSlider != null)
        {
            widthSlider.gameObject.SetActive(true);
        }
        if (completeLabel != null)
        {
            completeLabel.SetActive(false);
        }
        if (goButton != null)
            goButton.SetActive(false);

        StrokeCountText.GetComponent<StrokeScore>().HideScore();
        anchorButton.SetActive(true);

    }

    // Called when the user taps the Go button.
    public void OnGoButtonPressed()
    {
        buttonPressed = true;
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
        widthSlider.gameObject.SetActive(false);
        anchorButton.SetActive(false);
        StrokeCountText.GetComponent<StrokeScore>().resetScore();
        StrokeCountText.GetComponent<StrokeScore>().ShowScore();
    }

    public void OnAnchorButtonPressed(){
        buttonPressed = true;
        currentPlacementMode = PlacementMode.PlacingAnchor;
    }
    public ARPlane GetLargestPlane()
    {
        float maxArea = 0f;
        ARPlane largestPlane = null;

        foreach (var plane in _arPlaneManager.trackables) // Loop instead of LINQ
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                float area = plane.size.x * plane.size.y; // Compute area
                if (area > maxArea)
                {
                    maxArea = area;
                    largestPlane = plane;
                }
            }
        }

        return largestPlane;
    }
}
//TODO: bug when you press one button then another
//probably because raycast going through button
//might be best to handle all touches in one script