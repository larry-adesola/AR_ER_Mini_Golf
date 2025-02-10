using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceHole : MonoBehaviour
{

    [SerializeField]
    private GameObject holePrefab;  // Assign your Hole+Flag prefab in Inspector

    private ARRaycastManager _arRaycastManager;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    private GameObject placedHole;

    //Runs on the script being loaded
    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (_arRaycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = _hits[0].pose;

                    // Ensure we only place the hole on a flat, horizontal surface
                    if (IsSurfaceFlat(_hits[0]))
                    {
                        if (placedHole == null)
                        {
                            // First time placing the hole
                            placedHole = Instantiate(holePrefab, hitPose.position, Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                        }
                        else
                        {
                            // Move existing hole instead of spawning a new one
                            placedHole.transform.position = hitPose.position;
                            placedHole.transform.rotation = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);
                        }
                    }
                }
            }
        }
    }

    private bool IsSurfaceFlat(ARRaycastHit hit)
    {
        // The normal of a horizontal plane should be (0,1,0)
        return Vector3.Dot(hit.pose.up, Vector3.up) > 0.95f; // 0.95 ensures slight variations are accepted
    }
}
