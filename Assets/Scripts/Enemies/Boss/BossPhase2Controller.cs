using System.Collections;
using UnityEngine;

public class BossPhaseTwoController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BossPhaseTwoData phaseData;
    [SerializeField] private BossTurretData turretData;

    [Header("Turrets")]
    [SerializeField] private BossTurret[] turrets;

    [Header("VFX")]
    [SerializeField] private GameObject bossTransitionVFXPrefab;
    [SerializeField] private Transform bossTransitionVFXPoint;
    [SerializeField] private float bossTransitionVFXLifetime = 3f;

    [SerializeField] private GameObject turretSpawnVFXPrefab;
    [SerializeField] private float turretSpawnVFXLifetime = 2f;

    [SerializeField] private float turretSpawnDelay = 0.5f;

    private BossController boss;
    private int turretsRemaining;

    private void Awake()
    {
        foreach (BossTurret turret in turrets)
        {
            if (turret != null)
                turret.gameObject.SetActive(false);
        }
    }

    public IEnumerator BeginPhaseTwo(BossController controller)
    {
        boss = controller;

        boss.Animation.PlayPhaseTransition();

        PlayBossTransitionVFX();

        yield return new WaitForSeconds(phaseData.TransitionDuration);

        PlayTurretSpawnVFX();

        yield return new WaitForSeconds(turretSpawnDelay);

        ActivateTurrets();
    }

    private void ActivateTurrets()
    {
        turretsRemaining = 0;

        for (int i = 0; i < turrets.Length; i++)
        {
            BossTurret turret = turrets[i];

            if (turret == null)
                continue;

            turret.gameObject.SetActive(true);
            turret.OnDestroyed += HandleTurretDestroyed;

            float startDelay = i * turretData.TurretStaggerDelay;

            turret.Activate(
                boss.PlayerTarget,
                startDelay
            );

            turretsRemaining++;
        }
    }

    private void HandleTurretDestroyed(BossTurret turret)
    {
        turret.OnDestroyed -= HandleTurretDestroyed;

        turretsRemaining--;

        if (turretsRemaining <= 0)
            boss.CompletePhaseTwo();
    }

    private void PlayBossTransitionVFX()
    {
        if (bossTransitionVFXPrefab == null || bossTransitionVFXPoint == null)
            return;

        GameObject vfx = Instantiate(
            bossTransitionVFXPrefab,
            bossTransitionVFXPoint.position,
            bossTransitionVFXPoint.rotation
        );

        Destroy(vfx, bossTransitionVFXLifetime);
    }

    private void PlayTurretSpawnVFX()
    {
        if (turretSpawnVFXPrefab == null)
            return;

        foreach (BossTurret turret in turrets)
        {
            if (turret == null)
                continue;

            GameObject vfx = Instantiate(
                turretSpawnVFXPrefab,
                turret.transform.position,
                turret.transform.rotation
            );

            Destroy(vfx, turretSpawnVFXLifetime);
        }
    }

    public void StopPhaseTwo()
    {
        foreach (BossTurret turret in turrets)
        {
            if (turret != null)
                turret.StopAttacking();
        }
    }
}