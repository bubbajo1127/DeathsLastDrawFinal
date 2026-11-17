using System.Collections;
using UnityEngine;

public class BossPhaseTwoController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BossPhaseTwoData phaseData;
    [SerializeField] private BossTurretData turretData;

    [Header("Turrets")]
    [SerializeField] private BossTurret[] turrets;

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

        yield return new WaitForSeconds(phaseData.TransitionDuration);

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

    public void StopPhaseTwo()
    {
        foreach (BossTurret turret in turrets)
        {
            if (turret != null)
                turret.StopAttacking();
        }
    }
}