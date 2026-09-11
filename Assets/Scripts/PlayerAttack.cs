using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;

    [Header("Attack")]
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackDamage = 10f;

    private EnemyManager currentEnemy;

    private bool isAttacking;
    private float nextAttackTime;

    private void Update()
    {
        HandleCurrentEnemy();
    }

    public void SetAttackTarget(EnemyManager enemy)
    {
        if (enemy == null)
        {
            Debug.Log("Invalid enemy target.");
            return;
        }

        currentEnemy = enemy;
        isAttacking = false;

        TryAttackingEnemy();
    }

    private void HandleCurrentEnemy()
    {
        if (currentEnemy == null)
            return;

        TryAttackingEnemy();
    }

    private void TryAttackingEnemy()
    {
        if (currentEnemy == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            currentEnemy.transform.position
        );

        // Enemy is outside attack range.
        if (distance > attackRange)
        {
            if (!isAttacking)
            {
                movement.MoveToTarget(
                    currentEnemy.transform,
                    attackRange
                );
            }

            return;
        }

        // Enemy is in range, so stop moving.
        movement.StopMovement();

        // Already preparing an attack.
        if (isAttacking)
            return;

        // Still on attack cooldown.
        if (Time.time < nextAttackTime)
            return;

        BeginAttack();
    }

    private void BeginAttack()
    {
        isAttacking = true;

        // Future:
        // animator.SetTrigger("Attack");

        // Temporary testing behavior:
        // Fire immediately.
        FireProjectile();
    }

    public void FireProjectile()
    {
        if (!isAttacking)
            return;

        if (currentEnemy == null)
        {
            isAttacking = false;
            return;
        }

        // Temporary damage behavior.
        currentEnemy.TakeDamage(attackDamage);

        FinishAttack();
    }

    private void FinishAttack()
    {
        isAttacking = false;

        // attackSpeed = attacks per second.
        float attackCooldown = 1f / attackSpeed;

        nextAttackTime = Time.time + attackCooldown;
    }

    public void CancelAttack()
    {
        currentEnemy = null;
        isAttacking = false;
    }

    public EnemyManager GetCurrentEnemy()
    {
        return currentEnemy;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool IsTargetInRange()
    {
        if (currentEnemy == null)
            return false;

        float distance = Vector3.Distance(
            transform.position,
            currentEnemy.transform.position
        );

        return distance <= attackRange;
    }
}
