using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager;

    private NavMeshAgent navMeshAgent;

    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 0.1f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.stoppingDistance = stoppingDistance;
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
        if (target == null) return;

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
}