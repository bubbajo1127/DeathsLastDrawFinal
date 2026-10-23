using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Animation Settings")]
    [SerializeField] private float animationDampTime = 0.1f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int ShootHash =
        Animator.StringToHash("Shoot");

    private static readonly int DashHash =
        Animator.StringToHash("Dash");

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
            animationDampTime,
            Time.deltaTime
        );
    }

    public void PlayShoot()
    {
        animator.SetTrigger(ShootHash);
    }

    public void PlayDash()
    {
        animator.SetTrigger(DashHash);
    }

}
