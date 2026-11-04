using UnityEngine;

public abstract class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected EnemyStats enemyStats;

    protected PlayerStats currentTarget;

    private float nextAttackTime;
    public virtual bool ShouldTrackTargetRotation => true;

    public bool IsAttacking { get; protected set; }

    public bool TryAttack(PlayerStats target)
    {
        if (target == null)
            return false;

        if (target.IsDead())
            return false;

        if (IsAttacking)
            return false;

        if (Time.time < nextAttackTime)
            return false;

        currentTarget = target;
        IsAttacking = true;

        float cooldown =
            1f / Mathf.Max(enemyStats.AttackSpeed, 0.01f);

        nextAttackTime =
            Time.time + cooldown;

        StartAttack(target);

        return true;
    }

    protected abstract void StartAttack(PlayerStats target);

    protected void FinishAttack()
    {
        IsAttacking = false;
    }

    public bool IsAttackReady()
    {
        return
            !IsAttacking &&
            Time.time >= nextAttackTime;
    }
}
