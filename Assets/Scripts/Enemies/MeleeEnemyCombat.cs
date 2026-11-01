using System.Collections;
using UnityEngine;

public class MeleeEnemyCombat : EnemyCombat
{
    [Header("References")]
    [SerializeField] private EnemyAnimation enemyAnimation;

    [Header("Attack Timing")]
    [SerializeField] private float attackWindup = 0.15f;

    [Header("Hit Detection")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private LayerMask playerLayer;

    protected override void StartAttack(PlayerStats target)
    {
        StartCoroutine(AttackWindup());
    }

    private IEnumerator AttackWindup()
    {
        // Small pause before the enemy commits to the swing.
        yield return new WaitForSeconds(attackWindup);

        if (currentTarget == null || currentTarget.IsDead())
        {
            FinishAttack();
            yield break;
        }

        // Cancel if player escaped before the attack animation began.
        float distance = Vector3.Distance(
            transform.position,
            currentTarget.transform.position
        );

        if (distance > enemyStats.AttackRange)
        {
            FinishAttack();
            yield break;
        }

        enemyAnimation.PlayAttack();
    }

    // Called by an Animation Event on the impact frame.
    public void PerformHit()
    {
        if (!IsAttacking)
            return;

        if (currentTarget == null)
            return;

        Collider[] hits = Physics.OverlapSphere(
            hitPoint.position,
            hitRadius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            PlayerStats player =
                hit.GetComponentInParent<PlayerStats>();

            if (player == currentTarget)
            {
                player.TakeDamage(
                    enemyStats.AttackDamage
                );

                return;
            }
        }
    }

    // Called by an Animation Event near the end of the animation.
    public void EndAttack()
    {
        FinishAttack();
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null)
            return;

        Gizmos.DrawWireSphere(
            hitPoint.position,
            hitRadius
        );
    }
}
