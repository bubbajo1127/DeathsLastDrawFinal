using System.Collections;
using UnityEngine;

public abstract class BossAttack : MonoBehaviour
{
    [Header("Attack Data")]
    [SerializeField] private BossAttackData attackData;

    private float nextAvailableTime;

    public BossAttackData Data => attackData;

    public bool CanUse(PlayerStats target, BossPhaseData phase)
    {
        if (target == null || target.IsDead())
            return false;

        if (Time.time < nextAvailableTime)
            return false;

        if (!phase.TryGetWeight(attackData, out float weight) || weight <= 0f)
            return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        return distance >= attackData.MinRange && distance <= attackData.MaxRange;
    }

    public IEnumerator Execute(BossController boss, PlayerStats target)
    {
        float cooldownMultiplier = boss.CurrentPhase.CooldownMultiplier;

        yield return ExecuteAttack(boss, target);

        nextAvailableTime = Time.time + attackData.Cooldown * cooldownMultiplier;
    }

    protected abstract IEnumerator ExecuteAttack(BossController boss, PlayerStats target);
}
