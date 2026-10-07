using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;

    private Vector3 offset;

    private void Start()
    {
        if (target == null)
            return;

        // Remember the camera's starting position relative to the player.
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            1f - Mathf.Exp(-followSpeed * Time.deltaTime)
        );
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;
    }
}
