using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float hitDistance = 0.5f;

    private EnemyManager target;
    private float damage;

    public void Initialize(EnemyManager newTarget, float newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardsTarget();
        CheckForHit();
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
    }

    private void CheckForHit()
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance <= hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target == null) return;

        target.TakeDamage(damage);

        Destroy(gameObject);
    }
}
