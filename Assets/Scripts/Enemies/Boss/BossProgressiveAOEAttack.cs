using System.Collections;
using UnityEngine;

public class BossProgressiveAOEAttack : BossAttack
{
    [Header("References")]
    [SerializeField] private BossProgressiveAOEData aoeData;
    [SerializeField] private RectAttackTelegraph telegraphPrefab;
    [SerializeField] private Transform attackOrigin;

    [Header("Hit Detection")]
    [SerializeField] private LayerMask playerLayer;

    protected override IEnumerator ExecuteAttack(BossController boss, PlayerStats target)
    {
        boss.Animation.PlayAttack(Data);

        float timer = 0f;

        // Initial wind-up. Boss keeps looking at player.
        while (timer < Data.TelegraphTime)
        {
            if (target == null || target.IsDead())
                yield break;

            boss.Movement.FaceTarget(target.transform);

            timer += Time.deltaTime;
            yield return null;
        }

        // Lock direction once the attack begins.
        Vector3 attackDirection = target.transform.position - attackOrigin.position;
        attackDirection.y = 0f;
        attackDirection.Normalize();

        Quaternion attackRotation = Quaternion.LookRotation(attackDirection);

        float totalSequenceTime =
            (aoeData.LayerCount - 1) * aoeData.LayerInterval +
            aoeData.LayerTelegraphTime;

        float sequenceTimer = 0f;
        int nextTelegraph = 0;
        int nextHit = 0;

        while (sequenceTimer <= totalSequenceTime)
        {
            // Spawn new layers as the wave progresses.
            while (
                nextTelegraph < aoeData.LayerCount &&
                sequenceTimer >= nextTelegraph * aoeData.LayerInterval)
            {
                SpawnTelegraph(
                    attackDirection,
                    attackRotation,
                    nextTelegraph
                );

                nextTelegraph++;
            }

            // Damage layers when their individual telegraph finishes.
            while (
                nextHit < aoeData.LayerCount &&
                sequenceTimer >=
                nextHit * aoeData.LayerInterval +
                aoeData.LayerTelegraphTime)
            {
                PerformLayerHit(
                    boss,
                    attackDirection,
                    attackRotation,
                    nextHit
                );

                nextHit++;
            }

            sequenceTimer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(Data.RecoveryTime);
    }

    private void SpawnTelegraph(
        Vector3 direction,
        Quaternion rotation,
        int layerIndex)
    {
        Vector3 position =
            attackOrigin.position +
            direction * (aoeData.LayerLength * layerIndex);

        RectAttackTelegraph telegraph = Instantiate(
            telegraphPrefab,
            position,
            rotation
        );

        StartCoroutine(
            telegraph.Play(
                aoeData.Width,
                aoeData.LayerLength,
                aoeData.LayerTelegraphTime
            )
        );
    }

    private void PerformLayerHit(
        BossController boss,
        Vector3 direction,
        Quaternion rotation,
        int layerIndex)
    {
        Vector3 startPosition =
            attackOrigin.position +
            direction * (aoeData.LayerLength * layerIndex);

        Vector3 center =
            startPosition +
            direction * (aoeData.LayerLength * 0.5f) +
            Vector3.up * (aoeData.HitHeight * 0.5f);

        Vector3 halfExtents = new Vector3(
            aoeData.Width * 0.5f,
            aoeData.HitHeight * 0.5f,
            aoeData.LayerLength * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            rotation,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            PlayerStats player = hit.GetComponentInParent<PlayerStats>();

            if (player == null)
                continue;

            float damage =
                boss.Stats.AttackDamage *
                Data.DamageMultiplier *
                aoeData.DamageMultiplier;

            player.TakeDamage(damage);

            return;
        }
    }
}