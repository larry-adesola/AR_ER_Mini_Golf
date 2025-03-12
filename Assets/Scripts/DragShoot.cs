using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody))]
public class DragShoot : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Force Settings")]
    public float powerMultiplier = 0.05f;  // Scales how strong the shot is
    public float maxForce = 50f;         // Cap the maximum force if needed
    StrokeScore strokeScore;
    // [Header("Aim Line")]
    // public LineRenderer aimLine;          // Assign via Inspector, or create one at runtime
    // public float lineMaxLength = 2f;      // Max length of the visual line in world units
    private bool isDragging = false;
    private Vector2 startTouchPos;        // Screen-space start of drag
    private Vector3 ballScreenPos;        // Where the ball is in screen coordinates

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        strokeScore = GameObject.Find("Stroke Count Text").GetComponent<StrokeScore>();
    }


    private void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                // Check if the user tapped the ball (or near it) before dragging
                if (IsTouchingBall(touch.position))
                {
                    isDragging = true;
                    startTouchPos = touch.position;

                    // Store the ball’s screen position once
                    ballScreenPos = Camera.main.WorldToScreenPoint(transform.position);

                    // Enable aim line if available
                    //aimLine.enabled = true;

                }
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isDragging)
                {
                    // Update the aim line visuals
                    //UpdateAimLine(touch.position);
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging)
                {
                    isDragging = false;

                    // Hide the line
                    //aimLine.enabled = false;

                    // Calculate the force (opposite direction of drag)
                    Vector2 endTouchPos = touch.position;
                    Vector2 dragVector = endTouchPos - startTouchPos;

                    float dragMagnitude = dragVector.magnitude * powerMultiplier;
                    dragMagnitude = Mathf.Clamp(dragMagnitude, 0, maxForce);

                    // Convert drag direction to world space
                    // The direction is negative because we "pull back" to shoot forward
                    Vector3 forceDir = new Vector3(-dragVector.x, 0, -dragVector.y);

                    // A basic approach for orientation: align with Camera's forward
                    // This can get more complex if your AR camera can rotate a lot.
                    // For simplicity, let's do a rough conversion from screen space to world space:
                    forceDir = ScreenDirectionToWorld(forceDir);

                    // Scale with dragMagnitude
                    forceDir = forceDir.normalized * dragMagnitude;

                    // Apply force
                    rb.AddForce(forceDir, ForceMode.Impulse);

                    //uncomment this line if you are using strokescore.setactive(false) anywhere in proj
                    //strokeScore = GameObject.Find("Stroke Count Text").GetComponent<StrokeScore>();
                    strokeScore.IncrementScore();       
                }
                break;
        }
    }

    // Checks if the touch hits this ball's collider
    private bool IsTouchingBall(Vector2 touchPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPos);

        // Vector from the ray origin to the ball's center
        Vector3 toBall = transform.position - ray.origin;

        // Angle between the ray direction and the vector to the ball
        float angle = Vector3.Angle(ray.direction, toBall);

        // Distance from the ball's center to the closest point on the ray
        float perpendicularDist = toBall.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

        // Compare to a threshold radius in world space
        float nearRadius = 0.05f; // e.g. 20 cm in world units
        return perpendicularDist <= nearRadius;
    }

    // Update the aim line from the ball to the finger
    // private void UpdateAimLine(Vector2 currentTouchPos)
    // {
    //     if (aimLine == null) return;

    //     // 1) Convert screen positions to a world direction
    //     Vector2 dragVector = currentTouchPos - startTouchPos;

    //     // Direction is negative in XZ if "pulling back"
    //     Vector3 direction = new Vector3(-dragVector.x, 0, -dragVector.y);

    //     // Convert from screen space direction to world space
    //     direction = ScreenDirectionToWorld(direction).normalized;

    //     // The magnitude for the line's length
    //     float magnitude = dragVector.magnitude * 5f; // scale factor for line length
    //     magnitude = Mathf.Min(magnitude, lineMaxLength);

    //     Vector3 t = transform.position;
    //     Vector3 lineStart = new Vector3(t.x, t.y + 2f, t.z);
    //     Vector3 lineEnd = new Vector3(t.x, t.y + 2f, t.z) + direction * magnitude;

    //     Debug.Log($"Line: {lineStart} -> {lineEnd}");
    //     // 2) Set line renderer points
    //     aimLine.positionCount = 2;
    //     aimLine.SetPosition(0, lineStart);
    //     aimLine.SetPosition(1, lineEnd);
    // }

    // Convert a screen-space direction to a rough world-space direction
    private Vector3 ScreenDirectionToWorld(Vector3 screenDir)
    {
        // Example approach: project the screen direction onto the XZ-plane 
        // relative to the camera's forward and right vectors
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // Zero out the Y component so we don't aim up/down with the camera's pitch
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // screenDir.x corresponds to "horizontal drag" → use camRight
        // screenDir.z corresponds to "vertical drag" → use camForward
        Vector3 worldDir = (camRight * screenDir.x) + (camForward * screenDir.z);
        return worldDir;
    }
}
