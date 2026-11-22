using System.Collections.Generic;
using UnityEngine;

public class PlayerBlastAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BlastAbilityData blastData;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAnimator playerAnimation;
    [SerializeField] private Transform blastOrigin;

    [Header("Hit Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("VFX")]
    [SerializeField] private GameObject blastVFXPrefab;
    [SerializeField] private float blastVFXLifetime = 2f;

    private float nextUseTime;
    private bool blastPending;
    private Vector3 pendingDirection;

    private void Update()
    {
        if (!inputManager.Ability2Pressed)
            return;

        TryBlast();
    }

    private void TryBlast()
    {
        if (playerStats.IsDead())
            return;

        if (Time.time < nextUseTime)
            return;

        if (blastPending)
            return;

        if (!inputManager.HasMouseHit)
            return;

        EnemyManager target = inputManager.MouseHit.collider.GetComponentInParent<EnemyManager>();

        if (target == null)
            return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > blastData.CastRange)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        pendingDirection = direction.normalized;
        blastPending = true;

        transform.rotation = Quaternion.LookRotation(pendingDirection);

        playerAnimation.PlayBlast();

        nextUseTime = Time.time + blastData.Cooldown;
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
        Vector3 origin = blastOrigin.position;

        PlayBlastVFX(origin, direction);

        Vector3 center = origin + direction * (blastData.BlastLength * 0.5f);
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
}