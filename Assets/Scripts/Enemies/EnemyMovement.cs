using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats enemyStats;

    [Header("Rotation")]
    [SerializeField] private float attackRotationSpeed = 720f;

    private NavMeshAgent navMeshAgent;
    private Coroutine knockbackRoutine;

    public bool IsKnockedBack { get; private set; }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = enemyStats.MovementSpeed;
    }

    public void MoveTo(Transform target)
    {
        if (target == null)
            return;

        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        if (IsKnockedBack)
            return;

        navMeshAgent.updateRotation = true;
        navMeshAgent.speed = enemyStats.MovementSpeed;
        navMeshAgent.stoppingDistance = enemyStats.AttackRange * 0.8f;
        navMeshAgent.SetDestination(target.position);
    }

    public void MoveTo(Transform target, float stoppingDistance)
    {
        if (target == null)
            return;

        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        if (IsKnockedBack)
            return;

        navMeshAgent.updateRotation = true;
        navMeshAgent.speed = enemyStats.MovementSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.SetDestination(target.position);
    }

    public void StopMovement()
    {
        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        navMeshAgent.ResetPath();
    }

    public void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        if (IsKnockedBack)
            return;

        navMeshAgent.updateRotation = false;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            attackRotationSpeed * Time.deltaTime
        );
    }

    public void MoveInDirection(Vector3 direction, float speed)
    {
        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        if (IsKnockedBack)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        navMeshAgent.Move(direction * speed * Time.deltaTime);
    }

    public void DisableMovement()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        IsKnockedBack = false;

        if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            navMeshAgent.ResetPath();

        navMeshAgent.enabled = false;
    }

    public void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction, distance, duration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float distance, float duration)
    {
        IsKnockedBack = true;

        StopMovement();

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            IsKnockedBack = false;
            knockbackRoutine = null;
            yield break;
        }

        direction.Normalize();

        float speed = distance / Mathf.Max(duration, 0.01f);
        float timer = 0f;

        while (timer < duration)
        {
            if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
                break;

            navMeshAgent.Move(direction * speed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        IsKnockedBack = false;
        knockbackRoutine = null;
    }
}