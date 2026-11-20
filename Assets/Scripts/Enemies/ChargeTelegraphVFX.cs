using UnityEngine;

public class ChargeTelegraphVFX : MonoBehaviour
{
    [SerializeField] private Transform warning;

    private float width;
    private float length;

    public void Initialize(float width, float length)
    {
        this.width = width;
        this.length = length;

        warning.localScale = new Vector3(width, 0f, 1f);
        warning.localPosition = Vector3.zero;
    }

    public void UpdateTelegraph(Vector3 origin, Vector3 direction, float progress)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();
        progress = Mathf.Clamp01(progress);

        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(direction);

        float currentLength = length * progress;

        warning.localScale = new Vector3(width, currentLength, 1f);

        // Keeps the bottom of the warning at the enemy while the top grows outward.
        warning.localPosition = new Vector3(0f, 0f, currentLength * 0.5f);
    }
}