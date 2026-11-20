using System.Collections;
using UnityEngine;

public class BossProgressiveAOEAttack : BossAttack
{
    [Header("AOE")]
    [SerializeField] private BossProgressiveAOEData aoeData;
    [SerializeField] private RectAttackTelegraph telegraphPrefab;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask playerLayer;

    [Header("VFX")]
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private float hitVFXLifetime = 2f;

    private bool aoeTriggered;

    protected override IEnumerator ExecuteAttack(BossController boss, PlayerStats target)
    {
        aoeTriggered = false;

        boss.Animation.PlayAttack(Data);

        // Wait for the animation event while continuing to face the player.
        while (!aoeTriggered)
        {
            if (target == null || target.IsDead())
                yield break;

            boss.Movement.FaceTarget(target.transform);
            yield return null;
        }

        // Lock the attack direction when the animation event occurs.
        Vector3 attackDirection = target.transform.position - attackOrigin.position;
        attackDirection.y = 0f;

        if (attackDirection.sqrMagnitude <= 0.001f)
            yield break;

        attackDirection.Normalize();

        yield return StartCoroutine(PerformProgressiveAOE(boss, attackDirection));

        boss.Animation.ReturnToLocomotion();

        yield return new WaitForSeconds(Data.RecoveryTime);
    }

    public void StartProgressiveAOE()
    {
        aoeTriggered = true;
    }

    private IEnumerator PerformProgressiveAOE(BossController boss, Vector3 direction)
    {
        for (int i = 0; i < aoeData.LayerCount; i++)
        {
            float layerWidth = aoeData.StartingWidth + aoeData.WidthIncreasePerLayer * i;

            Vector3 layerPosition = attackOrigin.position + direction * aoeData.LayerLength * i;

            RectAttackTelegraph telegraph = Instantiate(
                telegraphPrefab,
                layerPosition,
                Quaternion.LookRotation(direction)
            );

            StartCoroutine(telegraph.Play(
                layerWidth,
                aoeData.LayerLength,
                aoeData.LayerTelegraphTime
            ));

            StartCoroutine(DamageLayerAfterTelegraph(
                boss,
                layerPosition,
                direction,
                layerWidth
            ));

            yield return new WaitForSeconds(aoeData.LayerInterval);
        }

        float finalWait = Mathf.Max(aoeData.LayerTelegraphTime - aoeData.LayerInterval, 0f);
        yield return new WaitForSeconds(finalWait);
    }

    private IEnumerator DamageLayerAfterTelegraph(BossController boss, Vector3 position, Vector3 direction, float width)
    {
        yield return new WaitForSeconds(aoeData.LayerTelegraphTime);

        Vector3 center = position + direction * (aoeData.LayerLength * 0.5f);

        if (hitVFXPrefab != null)
        {
            GameObject hitVFX = Instantiate(hitVFXPrefab, center, Quaternion.LookRotation(direction));
            Destroy(hitVFX, hitVFXLifetime);
        }

        center.y += aoeData.HitHeight * 0.5f;

        Vector3 halfExtents = new Vector3(
            width * 0.5f,
            aoeData.HitHeight * 0.5f,
            aoeData.LayerLength * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            Quaternion.LookRotation(direction),
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            PlayerStats player = hit.GetComponentInParent<PlayerStats>();

            if (player == null)
                continue;

            float damage = boss.Stats.AttackDamage * Data.DamageMultiplier * aoeData.DamageMultiplier;
            player.TakeDamage(damage);
            break;
        }
    }
}