using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        Combat,
        PhaseTransition,
        Dead
    }

    [Header("References")]
    [SerializeField] private EnemyStats bossStats;
    [SerializeField] private EnemyMovement bossMovement;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private BossAnimation bossAnimation;
    [SerializeField] private BossAttackSelector attackSelector;
    [SerializeField] private PlayerStats playerTarget;

    [Header("Phases")]
    [SerializeField] private BossPhaseData phaseOne;
    [SerializeField] private BossPhaseData phaseTwo;

    [Range(0f, 1f)]
    [SerializeField] private float phaseTwoHealthPercent = 0.5f;

    [Header("Phase Transition")]
    [SerializeField] private float phaseTransitionDuration = 1.5f;

    private BossState currentState;
    private BossPhaseData currentPhase;

    private BossAttack currentAttack;
    private BossAttack lastAttack;

    private bool phaseTransitionPending;
    private bool transitionStarted;

    public BossPhaseData CurrentPhase => currentPhase;
    public EnemyStats Stats => bossStats;
    public EnemyMovement Movement => bossMovement;
    public BossAnimation Animation => bossAnimation;

    private void Start()
    {
        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<PlayerStats>();

        currentPhase = phaseOne;
        currentState = BossState.Combat;

        bossStats.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        if (bossStats != null)
            bossStats.OnHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (bossStats.IsDead())
        {
            currentState = BossState.Dead;
            return;
        }

        if (playerTarget == null || playerTarget.IsDead())
            return;

        if (currentState != BossState.Combat)
            return;

        if (phaseTransitionPending && currentAttack == null)
        {
            if (!transitionStarted)
                StartCoroutine(TransitionToPhaseTwo());

            return;
        }

        if (currentAttack != null)
            return;

        BossAttack selectedAttack = attackSelector.SelectAttack(
            currentPhase,
            playerTarget,
            lastAttack
        );

        if (selectedAttack != null)
        {
            StartCoroutine(PerformAttack(selectedAttack));
            return;
        }

        Reposition();
    }

    private IEnumerator PerformAttack(BossAttack attack)
    {
        currentAttack = attack;

        bossMovement.StopMovement();

        yield return attack.Execute(this, playerTarget);

        lastAttack = attack;
        currentAttack = null;
    }

    private void Reposition()
    {
        bossMovement.MoveTo(
            playerTarget.transform,
            currentPhase.PreferredDistance
        );
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (currentPhase != phaseOne)
            return;

        if (currentHealth <= maxHealth * phaseTwoHealthPercent)
            phaseTransitionPending = true;
    }

    private IEnumerator TransitionToPhaseTwo()
    {
        transitionStarted = true;
        currentState = BossState.PhaseTransition;

        bossMovement.StopMovement();

        enemyManager.SetInvulnerable(true);

        bossAnimation.PlayPhaseTransition();

        yield return new WaitForSeconds(phaseTransitionDuration);

        currentPhase = phaseTwo;

        enemyManager.SetInvulnerable(false);

        phaseTransitionPending = false;
        transitionStarted = false;
        currentState = BossState.Combat;
    }
}