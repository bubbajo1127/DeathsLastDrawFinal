using UnityEngine;

public abstract class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected EnemyStats enemyStats;

    private float nextAttackTime;

    public bool TryAttack(PlayerStats target)
    {
        if (target == null)
            return false;

        if (target.IsDead())
            return false;

        if (Time.time < nextAttackTime)
            return false;

        PerformAttack(target);

        float attackCooldown =
            1f / Mathf.Max(enemyStats.AttackSpeed, 0.01f);

        nextAttackTime =
            Time.time + attackCooldown;

        return true;
    }

    protected abstract void PerformAttack(PlayerStats target);

    public bool IsAttackReady()
    {
        return Time.time >= nextAttackTime;
    }
}
