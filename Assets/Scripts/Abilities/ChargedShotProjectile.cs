using UnityEngine;

public class ChargedShotProjectile : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject impactVFXPrefab;
    [SerializeField] private float impactVFXLifetime = 2f;

    private Vector3 moveDirection;
    private float damage;
    private float moveSpeed;
    private float maxDistance;
    private float hitRadius;
    private float distanceTraveled;
    private LayerMask enemyLayer;
    private bool initialized;

    public void Initialize(
        Vector3 direction,
        float projectileDamage,
        float speed,
        float distance,
        float radius,
        LayerMask layer)
    {
        moveDirection = direction.normalized;
        damage = projectileDamage;
        moveSpeed = speed;
        maxDistance = distance;
        hitRadius = radius;
        enemyLayer = layer;
        initialized = true;

        transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    private void Update()
    {
        if (!initialized)
            return;

        float moveDistance = moveSpeed * Time.deltaTime;

        if (Physics.SphereCast(
            transform.position,
            hitRadius,
            moveDirection,
            out RaycastHit hit,
            moveDistance,
            enemyLayer,
            QueryTriggerInteraction.Collide))
        {
            transform.position = hit.point;

            EnemyManager enemy = hit.collider.GetComponentInParent<EnemyManager>();

            if (enemy != null)
                enemy.TakeDamage(damage);

            PlayImpactVFX();

            Destroy(gameObject);
            return;
        }

        transform.position += moveDirection * moveDistance;
        distanceTraveled += moveDistance;

        if (distanceTraveled >= maxDistance)
            Destroy(gameObject);
    }

    private void PlayImpactVFX()
    {
        if (impactVFXPrefab == null)
            return;

        GameObject vfx = Instantiate(impactVFXPrefab, transform.position, transform.rotation);
        Destroy(vfx, impactVFXLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}