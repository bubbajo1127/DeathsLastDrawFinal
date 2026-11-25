using System.Collections.Generic;
using UnityEngine;

public class PlayerBlastAbility : PlayerAbility
{
    [Header("References")]
    [SerializeField] private BlastAbilityData blastData;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAnimator playerAnimation;
    [SerializeField] private Transform blastHitOrigin;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Hit Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("VFX")]
    [SerializeField] private GameObject blastVFXPrefab;
    [SerializeField] private float blastVFXLifetime = 0.5f;
    [SerializeField] private Transform blastVFXOrigin;

    private float nextUseTime;
    private bool blastPending;
    private Vector3 pendingDirection;

    public override AbilityData Data => blastData;

    private void Update()
    {
        if (blastPending)
            FaceBlastDirection();
    }

    public override bool TryUse()
    {
        if (!IsUnlocked)
            return false;

        if (playerStats.IsDead())
            return false;

        if (Time.time < nextUseTime)
            return false;

        if (blastPending)
            return false;

        if (!inputManager.HasMouseHit)
            return false;

        EnemyManager target = inputManager.MouseHit.collider.GetComponentInParent<EnemyManager>();

        if (target == null)
            return false;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (!playerMovement.IsTargetInRange(target.transform, blastData.CastRange))
            return false; 

        if (direction.sqrMagnitude <= 0.001f)
            return false;

        pendingDirection = direction.normalized;
        blastPending = true;

        playerMovement.StopMovement();
        FaceBlastDirection();

        playerAnimation.PlayBlast();

        nextUseTime = Time.time + blastData.Cooldown;

        return true;
    }

    private void FaceBlastDirection()
    {
        if (pendingDirection.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(pendingDirection);
    }

    public void FireBlast()
    {
        if (!blastPending)
            return;

        blastPending = false;

        PerformBlast(pendingDirection);
    }

    private void PerformBlast(Vector3 direction)
    {
        Vector3 hitOrigin = blastHitOrigin.position;

        PlayBlastVFX(blastVFXOrigin.position, direction);

        Vector3 center = hitOrigin + direction * (blastData.BlastLength * 0.5f);
        center.y += blastData.HitHeight * 0.5f;

        Vector3 halfExtents = new Vector3(
            blastData.BlastWidth * 0.5f,
            blastData.HitHeight * 0.5f,
            blastData.BlastLength * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            Quaternion.LookRotation(direction),
            enemyLayer
        );

        HashSet<EnemyManager> damagedEnemies = new HashSet<EnemyManager>();

        foreach (Collider hit in hits)
        {
            EnemyManager enemy = hit.GetComponentInParent<EnemyManager>();

            if (enemy == null)
                continue;

            if (!damagedEnemies.Add(enemy))
                continue;

            float damage = playerStats.AttackDamage * blastData.DamageMultiplier;
            enemy.TakeDamage(damage);
        }
    }

    private void PlayBlastVFX(Vector3 origin, Vector3 direction)
    {
        if (blastVFXPrefab == null)
            return;

        GameObject vfx = Instantiate(
            blastVFXPrefab,
            origin,
            Quaternion.LookRotation(direction)
        );

        Destroy(vfx, blastVFXLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        if (blastHitOrigin == null || blastData == null)
            return;

        Vector3 direction = transform.forward;

        Vector3 center = blastHitOrigin.position + direction * (blastData.BlastLength * 0.5f);
        center.y += blastData.HitHeight * 0.5f;

        Gizmos.matrix = Matrix4x4.TRS(
            center,
            Quaternion.LookRotation(direction),
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(
                blastData.BlastWidth,
                blastData.HitHeight,
                blastData.BlastLength
            )
        );
    }
}