using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector2 offset;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = transform.position;

        if (followX)
        {
            targetPosition.x = target.position.x + offset.x;
        }

        if (followY)
        {
            targetPosition.y = target.position.y + offset.y;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}