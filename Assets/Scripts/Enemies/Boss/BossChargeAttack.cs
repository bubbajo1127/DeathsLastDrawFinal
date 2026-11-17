using System.Collections;
using UnityEngine;

public class BossChargeAttack : BossAttack
{
    [Header("Charge")]
    [SerializeField] private string chargeAnimationTrigger = "Charge";
    [SerializeField] private float chargeSpeed = 18f;
    [SerializeField] private float chargeDistance = 12f;

    [Header("Hit Detection")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;

    private bool hasHitPlayer;

    protected override IEnumerator ExecuteAttack(BossController boss, PlayerStats target)
    {
        hasHitPlayer = false;

        // Play the charge preparation / wind-up animation.
        boss.Animation.PlayAttack(Data);

        float timer = 0f;

        // Track the player during the telegraph.
        while (timer < Data.TelegraphTime)
        {
            if (target == null || target.IsDead())
                yield break;

            boss.Movement.FaceTarget(target.transform);

            timer += Time.deltaTime;
            yield return null;
        }

        if (target == null || target.IsDead())
            yield break;

        // Lock the charge direction here.
        Vector3 chargeDirection = target.transform.position - transform.position;
        chargeDirection.y = 0f;

        if (chargeDirection.sqrMagnitude <= 0.001f)
            yield break;

        chargeDirection.Normalize();

        // Once this starts, the boss no longer tracks the player.
        boss.Animation.PlayTrigger(chargeAnimationTrigger);

        Vector3 startPosition = transform.position;
        float maximumChargeTime = chargeDistance / chargeSpeed + 1f;
        float chargeTimer = 0f;

        while (
            Vector3.Distance(startPosition, transform.position) < chargeDistance &&
            chargeTimer < maximumChargeTime)
        {
            boss.Movement.MoveInDirection(chargeDirection, chargeSpeed);

            CheckForPlayerHit(boss, chargeDirection);

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        boss.Movement.StopMovement();

        yield return new WaitForSeconds(Data.RecoveryTime);
    }

    private void CheckForPlayerHit(BossController boss, Vector3 chargeDirection)
    {
        if (hasHitPlayer)
            return;

        Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerStats player = hit.GetComponentInParent<PlayerStats>();

            if (player == null)
                continue;

            float damage = boss.Stats.AttackDamage * Data.DamageMultiplier * boss.CurrentPhase.DamageMultiplier;

            player.TakeDamage(damage);

            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.ApplyKnockback(
                    chargeDirection,
                    knockbackDistance,
                    knockbackDuration
                );
            }

            hasHitPlayer = true;
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null)
            return;

        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
}