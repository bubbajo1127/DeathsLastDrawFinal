using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhaseTwoController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BossPhaseTwoData phaseData;

    [Header("Telegraph")]
    [SerializeField] private RectAttackTelegraph telegraphPrefab;

    [Header("Hazards")]
    [SerializeField] private BossHazardZone[] hazardZones;
    [SerializeField] private LayerMask playerLayer;

    [Header("Objectives")]
    [SerializeField] private BossAnchor[] anchors;

    private BossController boss;
    private Coroutine hazardRoutine;

    private int anchorsRemaining;
    private bool phaseActive;

    private void Awake()
    {
        foreach (BossAnchor anchor in anchors)
        {
            if (anchor != null)
                anchor.gameObject.SetActive(false);
        }
    }

    public IEnumerator BeginPhaseTwo(BossController controller)
    {
        boss = controller;

        boss.Animation.PlayPhaseTransition();

        yield return new WaitForSeconds(
            phaseData.TransitionDuration
        );

        ActivateAnchors();

        phaseActive = true;

        hazardRoutine =
            StartCoroutine(HazardLoop());
    }

    private void ActivateAnchors()
    {
        anchorsRemaining = anchors.Length;

        foreach (BossAnchor anchor in anchors)
        {
            if (anchor == null)
                continue;

            anchor.gameObject.SetActive(true);
            anchor.OnDestroyed += HandleAnchorDestroyed;
        }
    }

    private IEnumerator HazardLoop()
    {
        yield return new WaitForSeconds(
            phaseData.InitialHazardDelay
        );

        while (phaseActive)
        {
            float volleyStartTime = Time.time;

            yield return FireHazardVolley();

            float elapsed = Time.time - volleyStartTime;
            float remainingDelay =
                Mathf.Max(
                    0f,
                    phaseData.HazardInterval - elapsed
                );

            yield return new WaitForSeconds(
                remainingDelay
            );
        }
    }

    private IEnumerator FireHazardVolley()
    {
        List<BossHazardZone> availableZones =
            new List<BossHazardZone>(hazardZones);

        List<BossHazardZone> selectedZones =
            new List<BossHazardZone>();

        int count =
            Mathf.Min(
                phaseData.HazardsPerVolley,
                availableZones.Count
            );

        for (int i = 0; i < count; i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableZones.Count
                );

            BossHazardZone zone =
                availableZones[randomIndex];

            selectedZones.Add(zone);
            availableZones.RemoveAt(randomIndex);

            RectAttackTelegraph telegraph =
                Instantiate(
                    telegraphPrefab,
                    zone.transform.position,
                    zone.transform.rotation
                );

            StartCoroutine(
                telegraph.Play(
                    zone.Width,
                    zone.Length,
                    phaseData.TelegraphDuration
                )
            );
        }

        yield return new WaitForSeconds(
            phaseData.TelegraphDuration
        );

        HashSet<PlayerStats> damagedPlayers =
            new HashSet<PlayerStats>();

        foreach (BossHazardZone zone in selectedZones)
        {
            DamageZone(
                zone,
                damagedPlayers
            );
        }
    }

    private void DamageZone(
        BossHazardZone zone,
        HashSet<PlayerStats> damagedPlayers)
    {
        Vector3 halfExtents =
            new Vector3(
                zone.Width * 0.5f,
                zone.Height * 0.5f,
                zone.Length * 0.5f
            );

        Collider[] hits =
            Physics.OverlapBox(
                zone.Center,
                halfExtents,
                zone.transform.rotation,
                playerLayer
            );

        foreach (Collider hit in hits)
        {
            PlayerStats player =
                hit.GetComponentInParent<PlayerStats>();

            if (player == null)
                continue;

            if (damagedPlayers.Contains(player))
                continue;

            float damage =
                boss.Stats.AttackDamage *
                phaseData.DamageMultiplier;

            player.TakeDamage(damage);

            damagedPlayers.Add(player);
        }
    }

    private void HandleAnchorDestroyed(
        BossAnchor anchor)
    {
        anchor.OnDestroyed -=
            HandleAnchorDestroyed;

        anchorsRemaining--;

        if (anchorsRemaining <= 0)
            boss.CompletePhaseTwo();
    }

    public void StopPhaseTwo()
    {
        phaseActive = false;

        if (hazardRoutine != null)
            StopCoroutine(hazardRoutine);
    }
}