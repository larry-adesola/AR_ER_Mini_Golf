using UnityEngine;
using System.Collections;

public class MiniGolfHole : MonoBehaviour
{
    public string ballTag = "GolfBall";
    public LayerMask groundLayer; // Assign the AR plane's layer here

    public LayerMask ballLayer; // Assign the ball's layer here

    private BallResetter ballResetScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ballTag))
        {
            // 1. Disable collisions between the ball and AR plane
            int ballLayerNum = Mathf.RoundToInt(Mathf.Log(ballLayer.value, 2));
            int groundLayerNum = Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2));


            Physics.IgnoreLayerCollision(
                ballLayerNum,
                groundLayerNum,
                true
            );

            ballResetScript = other.GetComponent<BallResetter>();
            if (ballResetScript != null)
            {
                ballResetScript.enabled = false;
            }

            // 2. Optional: Add downward force to simulate falling
            // Rigidbody ballRb = other.GetComponent<Rigidbody>();
            // if (ballRb != null)
            // {
            //     ballRb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            // }

            // 3. Trigger effects (sound, particles, reset ball, etc.)
        }
    }
}