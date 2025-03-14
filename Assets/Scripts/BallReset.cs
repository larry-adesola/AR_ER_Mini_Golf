using UnityEngine;

public class BallResetter : MonoBehaviour
{
    [Header("Reset Settings")]
    // How far below the safe Y value the ball is allowed to drop before being reset.
    [SerializeField] private float fallMargin = 0.2f;

    // This value will hold the y-level of the plane the ball was placed on.
    private float safeY;

    private Rigidbody rb;
    private bool safeYSet = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Option 1: Use the initial y-position of the ball as the safe level.
        // This works if the ball is placed on the plane and that position is good.
        safeY = transform.position.y;
        safeYSet = true;
    }

    // Option 2: Expose a public method so your placement script can set the safeY.
    public void SetSafeY(float y)
    {
        safeY = y;
        safeYSet = true;
    }

    private void Update()
    {
        if (!safeYSet)
            return;

        // If the ball falls below safeY by more than fallMargin, reset its y.
        if (transform.position.y < safeY - fallMargin)
        {
            // Keep current x and z, but reset y to safeY.
            transform.position = new Vector3(transform.position.x, safeY + 1, transform.position.z);

            // Reset vertical velocity so it doesn't keep moving downward.
            //consider setting velocity to 0,0,0
            Vector3 currentVel = rb.velocity;
            rb.velocity = new Vector3(currentVel.x, 0, currentVel.z);
        }
    }
}