using UnityEngine;
using Cinemachine;
using System.Collections;

public class MapTransition : MonoBehaviour
{
    [Header("Boundary & Camera")]
    [SerializeField] private PolygonCollider2D mapBoundry; // The camera boundary for the NEW area

    [Header("Teleport Logic")]
    [SerializeField] private Direction direction;
    [SerializeField] private Transform teleportTargetPosition; // Where the player lands

    [Header("Audio")]
    [SerializeField] private string newMusicTrack; // The name of the song for this area

    private CinemachineConfiner confiner;
    private CinemachineVirtualCamera vcam;

    private enum Direction { Up, Down, Left, Right, Teleport }

    // Static variables share data across ALL instances of this script
    private static float lastTeleportTime;
    private const float TeleportCooldown = 0.5f;

    private void Awake()
    {
        // Find the camera components in the scene
        confiner = FindObjectOfType<CinemachineConfiner>();
        vcam = FindObjectOfType<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player entered and if we aren't currently in a teleport cooldown
        if (collision.CompareTag("Player") && Time.time > lastTeleportTime + TeleportCooldown)
        {
            lastTeleportTime = Time.time;
            StartCoroutine(TeleportSequence(collision.gameObject));
        }
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        // 1. Prepare Camera: Disable confiner briefly to prevent "snapping" artifacts
        if (confiner != null) confiner.enabled = false;

        // 2. Update Map Trackers (for UI/Minimaps if you have them)
        MapController_Manual.Instance?.HighlightArea(mapBoundry.name);
        MapController_Dynamic.Instance?.UpdateCurrentArea(mapBoundry.name);

        // 3. Calculate and Move Player
        Vector3 oldPos = player.transform.position;
        Vector3 newPos = (direction == Direction.Teleport) ? teleportTargetPosition.position : CalculateDirectionalPos(oldPos);

        player.transform.position = newPos;

        // Force Physics2D to update position immediately
        if (player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.position = newPos;
            rb.velocity = Vector2.zero; // Stop momentum so player doesn't slide
        }
        Physics2D.SyncTransforms();

        // 4. Warp Camera: Tell Cinemachine the player jumped so it doesn't "pan"
        if (vcam != null)
        {
            vcam.OnTargetObjectWarped(player.transform, newPos - oldPos);
        }

        // 5. Swap Boundary: Assign the new polygon collider
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = mapBoundry;
            confiner.InvalidatePathCache();

            // Wait one frame for Cinemachine to calculate the new bounds
            yield return new WaitForEndOfFrame();
            confiner.enabled = true;
        }

        // 6. Audio: Change the music
        if (!string.IsNullOrEmpty(newMusicTrack) && MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayTrackByName(newMusicTrack);
        }
    }

    private Vector3 CalculateDirectionalPos(Vector3 current)
    {
        // Offsets the player slightly so they aren't standing exactly on the trigger
        switch (direction)
        {
            case Direction.Up: current.y += 2.5f; break;
            case Direction.Down: current.y -= 2.5f; break;
            case Direction.Left: current.x -= 2.5f; break;
            case Direction.Right: current.x += 2.5f; break;
        }
        return current;
    }
}