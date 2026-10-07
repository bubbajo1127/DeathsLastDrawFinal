using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private EnemyCombat enemyCombat;

    [Header("Target")]
    [SerializeField] private PlayerStats playerTarget;

    private EnemyState currentState = EnemyState.Idle;

    private bool isDead;

    public EnemyState CurrentState => currentState;

    private void Start()
    {
        // Temporary convenience.
        // Later the enemy spawner can assign the player.
        if (playerTarget == null)
        {
            playerTarget =
                FindFirstObjectByType<PlayerStats>();
        }

        ChangeState(EnemyState.Idle);
    }

    private void Update()
    {
        if (isDead)
            return;

        if (playerTarget == null)
            return;

        if (playerTarget.IsDead())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        UpdateState();
        HandleCurrentState();
    }

    private void UpdateState()
    {
        float distance = Vector3.Distance(
            transform.position,
            playerTarget.transform.position
        );

        // Player is close enough to attack.
        if (distance <= enemyStats.AttackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // Player is close enough to chase.
        if (distance <= enemyStats.DetectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        ChangeState(EnemyState.Idle);
    }

    private void HandleCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                break;

            case EnemyState.Chase:

                enemyMovement.MoveTo(
                    playerTarget.transform
                );

                break;

            case EnemyState.Attack:

                enemyMovement.FaceTarget(
                    playerTarget.transform
                );

                enemyCombat.TryAttack(
                    playerTarget
                );

                break;

            case EnemyState.Dead:
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        ExitState(currentState);

        currentState = newState;

        EnterState(currentState);
    }

    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:

                enemyMovement.StopMovement();

                break;

            case EnemyState.Chase:

                break;

            case EnemyState.Attack:

                enemyMovement.StopMovement();

                break;

            case EnemyState.Dead:

                enemyMovement.DisableMovement();

                break;
        }
    }

    private void ExitState(EnemyState state)
    {
        // Nothing required yet.
        // Useful later for animations and special attacks.
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        enemyStats.TakeDamage(damage);

        if (enemyStats.IsDead())
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        ChangeState(EnemyState.Dead);

        Debug.Log(gameObject.name + " died.");

        // Temporary.
        // Later this is where the death animation will happen.
        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return enemyStats.CurrentHealth;
    }

    public float GetMaxHealth()
    {
        return enemyStats.MaxHealth;
    }

    public void SetTarget(PlayerStats target)
    {
        playerTarget = target;
    }
}