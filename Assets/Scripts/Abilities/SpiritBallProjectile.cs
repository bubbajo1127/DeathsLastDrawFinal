using System.Collections.Generic;
using UnityEngine;

public class SpiritBallProjectile : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private float explosionVFXLifetime = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float moveSpeed;
    private float arcHeight;
    private float damage;
    private float explosionRadius;
    private float knockbackDistance;
    private float knockbackDuration;
    private float travelProgress;

    private LayerMask enemyLayer;
    private bool initialized;

    public void Initialize(
        Vector3 target,
        float speed,
        float height,
        float projectileDamage,
        float radius,
        float knockback,
        float knockbackTime,
        LayerMask layer)
    {
        startPosition = transform.position;
        targetPosition = target;

        moveSpeed = speed;
        arcHeight = height;
        damage = projectileDamage;
        explosionRadius = radius;
        knockbackDistance = knockback;
        knockbackDuration = knockbackTime;
        enemyLayer = layer;

        travelProgress = 0f;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        float distance = Vector3.Distance(startPosition, targetPosition);

        if (distance <= 0.01f)
        {
            Explode();
            return;
        }

        travelProgress += (moveSpeed / distance) * Time.deltaTime;
        travelProgress = Mathf.Clamp01(travelProgress);

        Vector3 position = Vector3.Lerp(startPosition, targetPosition, travelProgress);

        float arc = 4f * arcHeight * travelProgress * (1f - travelProgress);
        position.y += arc;

        transform.position = position;

        if (travelProgress >= 1f)
            Explode();
    }

    private void Explode()
    {
        initialized = false;

        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(
                explosionVFXPrefab,
                targetPosition,
                Quaternion.identity
            );

            Destroy(vfx, explosionVFXLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(
            targetPosition,
            explosionRadius,
            enemyLayer
        );

        HashSet<EnemyManager> hitEnemies = new HashSet<EnemyManager>();

        foreach (Collider hit in hits)
        {
            EnemyManager enemy = hit.GetComponentInParent<EnemyManager>();

            if (enemy == null)
                continue;

            if (!hitEnemies.Add(enemy))
                continue;

            enemy.TakeDamage(damage);

            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();

            if (enemyMovement != null)
            {
                Vector3 knockbackDirection = enemy.transform.position - targetPosition;
                knockbackDirection.y = 0f;

                enemyMovement.ApplyKnockback(
                    knockbackDirection,
                    knockbackDistance,
                    knockbackDuration
                );
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}