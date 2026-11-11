using UnityEngine;
using UnityEngine.AI;

public class BruteEnemyAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Settings")]
    [SerializeField] private float movementDampTime = 0.1f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int PrepareChargeHash = Animator.StringToHash("PrepareCharge");

    private static readonly int ChargeHash = Animator.StringToHash("Charge");

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        float normalizedSpeed = 0f;

        if (navMeshAgent.enabled &&
            navMeshAgent.speed > 0f)
        {
            normalizedSpeed =
                navMeshAgent.velocity.magnitude /
                navMeshAgent.speed;
        }

        animator.SetFloat(
            SpeedHash,
            normalizedSpeed,
            movementDampTime,
            Time.deltaTime
        );
    }

    public void PlayPrepareCharge()
    {
        animator.SetTrigger(PrepareChargeHash);
    }

    public void PlayCharge()
    {
        animator.SetTrigger(ChargeHash);
    }
}