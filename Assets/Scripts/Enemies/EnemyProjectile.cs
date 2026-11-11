using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float hitRadius = 0.35f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask playerLayer;

    private Vector3 moveDirection;
    private float damage;
    private bool hasHit;

    public void Initialize(Vector3 direction, float projectileDamage)
    {
        direction.y = 0f;
        moveDirection = direction.normalized;
        damage = projectileDamage;

        transform.forward = moveDirection;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit)
            return;

        MoveProjectile();
        CheckForPlayerHit();
    }

    private void MoveProjectile()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void CheckForPlayerHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerStats player = hit.GetComponentInParent<PlayerStats>();

            if (player == null)
                continue;

            hasHit = true;

            player.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
