using UnityEngine;
using UnityEngine.AI;

public class BossAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Settings")]
    [SerializeField] private float movementDampTime = 0.1f;
    [SerializeField] private string phaseTransitionTrigger = "PhaseTransition";

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Update()
    {
        float normalizedSpeed = 0f;

        if (navMeshAgent.enabled && navMeshAgent.speed > 0f)
            normalizedSpeed = navMeshAgent.velocity.magnitude / navMeshAgent.speed;

        animator.SetFloat(SpeedHash, normalizedSpeed, movementDampTime, Time.deltaTime);
    }

    public void PlayAttack(BossAttackData attack)
    {
        PlayTrigger(attack.AnimationTrigger);
    }

    public void PlayTrigger(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName))
            return;

        animator.SetTrigger(Animator.StringToHash(triggerName));
    }

    public void PlayPhaseTransition()
    {
        PlayTrigger(phaseTransitionTrigger);
    }
}
