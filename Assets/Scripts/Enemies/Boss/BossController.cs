using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        PhaseOne,
        PhaseTransition,
        PhaseTwo,
        Dead
    }

    [Header("References")]
    [SerializeField] private EnemyStats bossStats;
    [SerializeField] private EnemyMovement bossMovement;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private BossAnimation bossAnimation;
    [SerializeField] private BossAttackSelector attackSelector;
    [SerializeField] private PlayerStats playerTarget;

    [Header("Phase One")]
    [SerializeField] private BossPhaseData phaseOne;

    [Header("Phase Two")]
    [SerializeField] private BossPhaseTwoController phaseTwoController;
    [SerializeField, Range(0f, 1f)] private float phaseTwoHealthPercent = 0.3f;

    private BossState currentState;
    private BossPhaseData currentPhase;

    private BossAttack currentAttack;
    private BossAttack lastAttack;

    private Coroutine currentAttackRoutine;

    public BossPhaseData CurrentPhase => currentPhase;
    public EnemyStats Stats => bossStats;
    public EnemyMovement Movement => bossMovement;
    public BossAnimation Animation => bossAnimation;

    private void Start()
    {
        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<PlayerStats>();

        currentPhase = phaseOne;
        currentState = BossState.PhaseOne;

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

        if (currentState != BossState.PhaseOne)
            return;

        if (currentAttack != null)
            return;

        BossAttack selectedAttack = attackSelector.SelectAttack(
            currentPhase,
            playerTarget,
            lastAttack
        );

        if (selectedAttack != null)
        {
            currentAttackRoutine = StartCoroutine(PerformAttack(selectedAttack));
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
        currentAttackRoutine = null;
    }

    private void Reposition()
    {
        bossMovement.MoveTo(playerTarget.transform, currentPhase.PreferredDistance);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (currentState != BossState.PhaseOne)
            return;

        if (currentHealth <= maxHealth * phaseTwoHealthPercent)
            StartCoroutine(TransitionToPhaseTwo());
    }

    private IEnumerator TransitionToPhaseTwo()
    {
        currentState = BossState.PhaseTransition;

        enemyManager.SetInvulnerable(true);
        bossMovement.StopMovement();

        CancelCurrentAttack();

        yield return phaseTwoController.BeginPhaseTwo(this);

        currentState = BossState.PhaseTwo;
    }

    private void CancelCurrentAttack()
    {
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }

        currentAttack = null;
    }

    public void CompletePhaseTwo()
    {
        if (currentState != BossState.PhaseTwo)
            return;

        currentState = BossState.Dead;

        phaseTwoController.StopPhaseTwo();

        enemyManager.SetInvulnerable(false);
        enemyManager.ForceKill();
    }
}