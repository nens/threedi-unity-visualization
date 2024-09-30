using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target object the camera will follow
    public Vector3 offset;   // Offset from the target position (x, y, z)
    public float smoothSpeed = 0.125f; // Smoothness factor for the camera movement

    void LateUpdate()
    {
        if (target != null)
        {
            // Define desired position based on the target's position and the offset
            Vector3 desiredPosition = target.position + offset;

            // Smoothly interpolate between current position and desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Optional: Look at the target
            transform.LookAt(target);
        }
    }
}
