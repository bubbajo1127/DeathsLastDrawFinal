using UnityEngine;

public class PlayerChargedShotAbility : PlayerAbility
{
    [Header("References")]
    [SerializeField] private ChargedShotAbilityData chargedShotData;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Transform projectileOrigin;

    [Header("Projectile")]
    [SerializeField] private ChargedShotProjectile projectilePrefab;
    [SerializeField] private LayerMask enemyLayer;

    private float nextUseTime;
    private bool shotPending;
    private Vector3 pendingDirection;

    public override AbilityData Data => chargedShotData;

    private void Update()
    {
        if (shotPending)
            FaceShotDirection();
    }

    public override bool TryUse()
    {
        if (!IsUnlocked)
            return false;

        if (playerStats.IsDead())
            return false;

        if (Time.time < nextUseTime)
            return false;

        if (shotPending)
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

        pendingDirection = direction.normalized;
        shotPending = true;

        playerMovement.StopMovement();
        FaceShotDirection();

        playerAnimator.PlayChargedShot();

        nextUseTime = Time.time + chargedShotData.Cooldown;

        return true;
    }

    private void FaceShotDirection()
    {
        if (pendingDirection.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(pendingDirection);
    }

    public void FireChargedShot()
    {
        if (!shotPending)
            return;

        shotPending = false;

        float damage = playerStats.AttackDamage * chargedShotData.DamageMultiplier;

        ChargedShotProjectile projectile = Instantiate(
            projectilePrefab,
            projectileOrigin.position,
            Quaternion.LookRotation(pendingDirection)
        );

        projectile.Initialize(
            pendingDirection,
            damage,
            chargedShotData.ProjectileSpeed,
            chargedShotData.MaxDistance,
            chargedShotData.HitRadius,
            enemyLayer
        );
    }
}