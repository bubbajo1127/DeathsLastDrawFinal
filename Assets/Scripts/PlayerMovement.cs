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
        HandleMovementInput();
        HandleStopInput();
    }

    private void HandleMovementInput()
    {
        if(inputManager.LeftClickPressed && inputManager.HasMouseHit)
        {
            MoveToPosition(inputManager.MouseWorldPosition);
        }
    }

    private void HandleStopInput()
    {
        if (inputManager.StopMovementPressed)
        {
            navMeshAgent.ResetPath();
        }
    }


    private void MoveToPosition(Vector3 destination)
    {
        if(NavMesh.SamplePosition(
            destination,
            out NavMeshHit navHit,
            2f,
            NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(navHit.position);
        }
    }
}