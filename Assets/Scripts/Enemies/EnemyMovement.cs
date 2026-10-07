using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats enemyStats;

    [Header("Rotation")]
    [SerializeField] private float attackRotationSpeed = 720f;

    private NavMeshAgent navMeshAgent;

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

        navMeshAgent.updateRotation = true;

        navMeshAgent.speed = enemyStats.MovementSpeed;

        navMeshAgent.stoppingDistance =
            enemyStats.AttackRange * 0.8f;

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

        navMeshAgent.updateRotation = false;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            attackRotationSpeed * Time.deltaTime
        );
    }

    public void DisableMovement()
    {
        if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
        }

        navMeshAgent.enabled = false;
    }
}
