using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform.

    public Vector3 offset = new Vector3(0f, 2f, -10f); // Offset to adjust camera position.

    public float smoothSpeed = 0.125f; // Adjust for smoother camera movement.

    void FixedUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player Transform is not assigned!");
            return;
        }

        // Calculate the desired position of the camera.
        Vector3 desiredPosition = player.position + offset;

        // Smoothly interpolate between the camera's current position and the desired position.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply the new position to the camera.
        transform.position = smoothedPosition;
    }
}