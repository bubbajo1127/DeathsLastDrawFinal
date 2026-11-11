using UnityEngine;

public class RangedEnemyCombat : EnemyCombat
{
    [Header("References")]
    [SerializeField] private EnemyAnimation enemyAnimation;

    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Cooldown")]
    [SerializeField] private float attackCooldown = 5f;

    protected override void StartAttack(PlayerStats target)
    {
        enemyAnimation.PlayAttack();
    }

    protected override float GetAttackCooldown()
    {
        return attackCooldown;
    }

    public void FireProjectile()
    {
        if (!IsAttacking)
            return;

        if (currentTarget == null || currentTarget.IsDead())
            return;

        Vector3 direction = currentTarget.transform.position - projectileSpawnPoint.position;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.LookRotation(direction)
        );

        projectile.Initialize(direction, enemyStats.AttackDamage);
    }

    public void EndAttack()
    {
        FinishAttack();
    }
}
