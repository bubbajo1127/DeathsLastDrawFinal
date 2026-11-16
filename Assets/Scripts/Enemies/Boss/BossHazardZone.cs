using UnityEngine;

public class BossHazardZone : MonoBehaviour
{
    [SerializeField] private float width = 8f;
    [SerializeField] private float length = 15f;
    [SerializeField] private float height = 2f;

    public float Width => width;
    public float Length => length;
    public float Height => height;

    public Vector3 Center =>
        transform.position +
        transform.forward * (length * 0.5f) +
        Vector3.up * (height * 0.5f);

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            Center,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(width, height, length)
        );
    }
}