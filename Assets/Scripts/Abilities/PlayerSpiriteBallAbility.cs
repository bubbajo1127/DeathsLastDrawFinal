using UnityEngine;

public class PlayerSpiritBallAbility : PlayerAbility
{
    [Header("References")]
    [SerializeField] private SpiritBallAbilityData spiritBallData;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Transform projectileOrigin;

    [Header("Projectile")]
    [SerializeField] private SpiritBallProjectile projectilePrefab;
    [SerializeField] private LayerMask enemyLayer;

    private float nextUseTime;
    private bool throwPending;
    private Vector3 pendingTargetPosition;
    private Vector3 pendingDirection;

    public override AbilityData Data => spiritBallData;

    private void Update()
    {
        if (throwPending)
            FaceThrowDirection();
    }

    public override bool TryUse()
    {
        if (!IsUnlocked)
            return false;

        if (playerStats.IsDead())
            return false;

        if (Time.time < nextUseTime)
            return false;

        if (throwPending)
            return false;

        Vector3 mousePosition;

        if (inputManager.HasMouseHit)
        {
            mousePosition = inputManager.MouseWorldPosition;
        }
        else if (!inputManager.TryGetMousePositionOnPlane(transform.position.y, out mousePosition))
        {
            return false;
        }

        Vector3 direction = mousePosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return false;

        float distance = Mathf.Min(
            direction.magnitude,
            spiritBallData.MaxThrowDistance
        );

        pendingDirection = direction.normalized;
        pendingTargetPosition = transform.position + pendingDirection * distance;
        pendingTargetPosition.y = mousePosition.y;

        throwPending = true;

        playerMovement.StopMovement();
        FaceThrowDirection();

        playerAnimator.PlaySpiritBall();

        nextUseTime = Time.time + spiritBallData.Cooldown;

        return true;
    }

    private void FaceThrowDirection()
    {
        if (pendingDirection.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(pendingDirection);
    }

    public void ThrowSpiritBall()
    {
        if (!throwPending)
            return;

        throwPending = false;

        float damage = playerStats.AttackDamage * spiritBallData.DamageMultiplier;

        SpiritBallProjectile projectile = Instantiate(
            projectilePrefab,
            projectileOrigin.position,
            Quaternion.LookRotation(pendingDirection)
        );

        projectile.Initialize(
            pendingTargetPosition,
            spiritBallData.ProjectileSpeed,
            spiritBallData.ArcHeight,
            damage,
            spiritBallData.ExplosionRadius,
            spiritBallData.KnockbackDistance,
            spiritBallData.KnockbackDuration,
            enemyLayer
        );
    }
}