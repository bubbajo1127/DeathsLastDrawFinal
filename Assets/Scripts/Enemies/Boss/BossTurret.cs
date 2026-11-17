using System;
using System.Collections;
using UnityEngine;

public class BossTurret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private EnemyStats turretStats;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BossTurretData turretData;

    private PlayerStats playerTarget;
    private Coroutine attackRoutine;

    public event Action<BossTurret> OnDestroyed;

    private void OnEnable()
    {
        if (enemyManager != null)
            enemyManager.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (enemyManager != null)
            enemyManager.OnDied -= HandleDeath;
    }

    public void Activate(PlayerStats target, float initialDelay)
    {
        playerTarget = target;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackLoop(initialDelay));
    }

    private IEnumerator AttackLoop(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        while (playerTarget != null && !playerTarget.IsDead())
        {
            float cycleStartTime = Time.time;

            yield return FireBurst();

            float elapsedTime = Time.time - cycleStartTime;
            float remainingTime = Mathf.Max(0f, turretData.AttackInterval - elapsedTime);

            yield return new WaitForSeconds(remainingTime);
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < turretData.BurstCount; i++)
        {
            if (playerTarget == null || playerTarget.IsDead())
                yield break;

            FireProjectile();

            if (i < turretData.BurstCount - 1)
                yield return new WaitForSeconds(turretData.BurstShotDelay);
        }
    }

    private void FireProjectile()
    {
        Vector3 direction = playerTarget.transform.position - projectileSpawnPoint.position;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.LookRotation(direction)
        );

        float damage = turretStats.AttackDamage * turretData.DamageMultiplier;

        projectile.Initialize(direction, damage);
    }

    private void HandleDeath(EnemyManager enemy)
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        OnDestroyed?.Invoke(this);
    }

    public void StopAttacking()
    {
        if (attackRoutine == null)
            return;

        StopCoroutine(attackRoutine);
        attackRoutine = null;
    }
}