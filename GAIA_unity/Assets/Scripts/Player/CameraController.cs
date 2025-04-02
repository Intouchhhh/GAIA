using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; 
    public Vector3 offset = new Vector3(0f, 0f, -10f); 
    public float smoothTime = 0.125f; 
    public Vector3 Velocity = Vector3.zero;

    void LateUpdate()
    {
        Vector3 TargetPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, TargetPosition, ref Velocity, smoothTime);
        transform.position = smoothedPosition;
    }
}