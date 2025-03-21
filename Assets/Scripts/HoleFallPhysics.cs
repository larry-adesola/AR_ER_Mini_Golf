using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
public class MiniGolfHole : MonoBehaviour
{
    public string ballTag = "GolfBall";
    [SerializeField]public int highScore = 999;
    // public LayerMask groundLayer; // Assign the AR plane's layer here

    // public LayerMask ballLayer; // Assign the ball's layer here
    public GameObject completeLabel;
    private BallResetter ballResetScript;
    private PlaceGolfObjects placeGolfObjects;
    private DragShoot dragShootScript;

    private StrokeScore strokeScore;
    private void OnTriggerEnter(Collider other)
    {
        //dont trigger if we are in the ball placement mode.(can no longer change xr origin name)
        placeGolfObjects = GameObject.Find("XR Origin (XR Rig)").GetComponent<PlaceGolfObjects>();
        if (other.CompareTag(ballTag) && placeGolfObjects.currentPlacementMode == PlacementMode.None)
        {
            // 1. Disable collisions between the ball and AR plane
            // int ballLayerNum = Mathf.RoundToInt(Mathf.Log(ballLayer.value, 2));
            // int groundLayerNum = Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2));


            // Physics.IgnoreLayerCollision(
            //     ballLayerNum,
            //     groundLayerNum,
            //     true
            // );
            strokeScore = GameObject.Find("Stroke Count Text").GetComponent<StrokeScore>();
            ballResetScript = other.GetComponent<BallResetter>();
            if (ballResetScript != null)
            {
                ballResetScript.enabled = false;
            }

            dragShootScript = other.GetComponent<DragShoot>();
            if (dragShootScript != null)
            {
                dragShootScript.enabled = false;
            }

            if (completeLabel != null)
            {
                int score = strokeScore.getScore();
                if (score < highScore)highScore = score;
                strokeScore.HideScore();
                completeLabel.GetComponentInChildren<TMP_Text>().text = $"Complete!!\nScore: {score}\nBest Score: {highScore}";
                completeLabel.SetActive(true);
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