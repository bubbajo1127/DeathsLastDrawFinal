using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats playerStats;

    private NavMeshAgent navMeshAgent;

    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 0.1f;

    private Coroutine knockbackCoroutine;
    public bool IsKnockedBack { get; private set; }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.speed = playerStats.MovementSpeed;
        navMeshAgent.updateRotation = true;
    }

    private void Update()
    {
        HandleStopInput();
    }

    private void HandleStopInput()
    {
        if (inputManager.StopMovementPressed)
        {
            StopMovement();
        }
    }

    public void MoveToPosition(Vector3 destination)
    {
        if (IsKnockedBack) return;

        if(NavMesh.SamplePosition(
            destination,
            out NavMeshHit navHit,
            2f,
            NavMesh.AllAreas))
        {
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.SetDestination(navHit.position);
        }
    }

    public void MoveToTarget(Transform target, float distance)
    {
        if (target == null || IsKnockedBack) return;

        navMeshAgent.stoppingDistance = distance;
        navMeshAgent.SetDestination(target.position);
    }

    public void StopMovement()
    {
        navMeshAgent.ResetPath();
    }

    public bool IsMoving()
    {
        return navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;
    }

    //PLAYER KNOCKBACK
    public void ApplyKnockback(Vector3 direction,float distance,float duration)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(
            KnockbackRoutine(direction, distance, duration)
        );
    }

    private IEnumerator KnockbackRoutine(Vector3 direction,float distance,float duration)
    {
        IsKnockedBack = true;

        navMeshAgent.ResetPath();

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            IsKnockedBack = false;
            yield break;
        }

        direction.Normalize();

        duration = Mathf.Max(duration, 0.01f);

        float knockbackSpeed = distance / duration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            navMeshAgent.Move(
                direction *
                knockbackSpeed *
                Time.deltaTime
            );

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        IsKnockedBack = false;
        knockbackCoroutine = null;
    }
}