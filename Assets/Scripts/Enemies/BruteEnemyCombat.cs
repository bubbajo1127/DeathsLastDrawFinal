using UnityEngine;
using System.Collections;

public class BruteEnemyCombat : EnemyCombat
{
    [Header("References")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private BruteEnemyAnimation bruteAnimation;

    [Header("Charge")]
    [SerializeField] private float chargeWindup = 0.5f;
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDistance = 10f;
    [SerializeField] private float chargeCooldown = 4f;

    [Header("Charge Hit")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 1.25f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 4f;
    [SerializeField] private float knockbackDuration = 0.25f;

    private bool isCharging;
    private bool hasHitPlayer;

    private Vector3 chargeDirection;

    public override bool ShouldTrackTargetRotation => !isCharging;
    protected override float GetAttackCooldown()
    {
        return chargeCooldown;
    }

    protected override void StartAttack(PlayerStats target)
    {
        StartCoroutine(
            ChargeRoutine(target)
        );
    }

    private IEnumerator ChargeRoutine(PlayerStats target)
    {
        hasHitPlayer = false;

        // WIND UP
        bruteAnimation.PlayPrepareCharge();

        float windupTimer = 0f;

        while (windupTimer < chargeWindup)
        {
            if (target == null || target.IsDead())
            {
                FinishAttack();
                yield break;
            }

            // Keep looking at the player during the entire telegraph.
            enemyMovement.FaceTarget(target.transform);

            windupTimer += Time.deltaTime;

            yield return null;
        }

        if (target == null)
        {
            FinishAttack();
            yield break;
        }

        // LOCK CHARGE DIRECTION
        chargeDirection = target.transform.position - transform.position;

        chargeDirection.y = 0f;
        chargeDirection.Normalize();

        isCharging = true;

        bruteAnimation.PlayCharge();

        Vector3 startPosition = transform.position;

        float safetyTimer = 0f;

        float maximumChargeTime = (chargeDistance / chargeSpeed) + 1f;

        // CHARGE
        while(Vector3.Distance(startPosition,transform.position) < chargeDistance && safetyTimer < maximumChargeTime)
        {
            enemyMovement.MoveInDirection(chargeDirection,chargeSpeed);

            CheckForPlayerHit();

            safetyTimer += Time.deltaTime;

            yield return null;
        }

        // FINISH
        isCharging = false;

        FinishAttack();
    }

    private void CheckForPlayerHit()
    {
        if (hasHitPlayer)
            return;

        Collider[] hits =
            Physics.OverlapSphere(
                hitPoint.position,
                hitRadius,
                playerLayer
            );

        foreach (Collider hit in hits)
        {
            PlayerStats player =
                hit.GetComponentInParent<PlayerStats>();

            if (player == null)
                continue;

            // Damage
            player.TakeDamage(
                enemyStats.AttackDamage
            );

            // Knockback
            PlayerMovement playerMovement =
                player.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.ApplyKnockback(
                    chargeDirection,
                    knockbackDistance,
                    knockbackDuration
                );
            }

            // Only hit once per charge.
            hasHitPlayer = true;

            return;
        }
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
