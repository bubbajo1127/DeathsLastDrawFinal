using System.Collections;
using UnityEngine;

public class BossChargeAttack : BossAttack
{
    [Header("Charge")]
    [SerializeField] private string chargeAnimationTrigger = "Charge";
    [SerializeField] private float chargeSpeed = 18f;
    [SerializeField] private float chargeDistance = 12f;

    [Header("Telegraph")]
    [SerializeField] private ChargeTelegraphVFX chargeTelegraphPrefab;
    [SerializeField] private Transform chargeOrigin;
    [SerializeField] private float chargeWidth = 3f;

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

        boss.Animation.PlayAttack(Data);

        ChargeTelegraphVFX telegraph = null;

        if (chargeTelegraphPrefab != null && chargeOrigin != null)
        {
            telegraph = Instantiate(
                chargeTelegraphPrefab,
                chargeOrigin.position,
                chargeOrigin.rotation
            );

            telegraph.Initialize(chargeWidth, chargeDistance);
        }

        float timer = 0f;

        // Windup / aiming phase.
        while (timer < Data.TelegraphTime)
        {
            if (target == null || target.IsDead())
            {
                DestroyTelegraph(telegraph);
                yield break;
            }

            boss.Movement.FaceTarget(target.transform);

            Vector3 aimDirection = target.transform.position - chargeOrigin.position;
            aimDirection.y = 0f;

            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / Data.TelegraphTime);

            if (telegraph != null)
                telegraph.UpdateTelegraph(chargeOrigin.position, aimDirection, progress);

            yield return null;
        }

        if (target == null || target.IsDead())
        {
            DestroyTelegraph(telegraph);
            yield break;
        }

        // Lock the player's position at the moment the charge begins.
        Vector3 chargeDirection = target.transform.position - transform.position;
        chargeDirection.y = 0f;

        if (chargeDirection.sqrMagnitude <= 0.001f)
        {
            DestroyTelegraph(telegraph);
            yield break;
        }

        chargeDirection.Normalize();

        // Warning disappears once the direction is locked.
        DestroyTelegraph(telegraph);

        boss.Animation.PlayTrigger(chargeAnimationTrigger);

        Vector3 startPosition = transform.position;
        float maximumChargeTime = chargeDistance / chargeSpeed + 1f;
        float chargeTimer = 0f;

        while (Vector3.Distance(startPosition, transform.position) < chargeDistance &&
               chargeTimer < maximumChargeTime)
        {
            boss.Movement.MoveInDirection(chargeDirection, chargeSpeed);
            CheckForPlayerHit(boss, chargeDirection);

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        boss.Movement.StopMovement();
        boss.Animation.ReturnToLocomotion();

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
                playerMovement.ApplyKnockback(chargeDirection, knockbackDistance, knockbackDuration);

            hasHitPlayer = true;
            return;
        }
    }

    private void DestroyTelegraph(ChargeTelegraphVFX telegraph)
    {
        if (telegraph != null)
            Destroy(telegraph.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null)
            return;

        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
}