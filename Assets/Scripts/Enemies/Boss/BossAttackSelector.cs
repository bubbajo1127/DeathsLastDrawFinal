using System.Collections.Generic;
using UnityEngine;

public class BossAttackSelector : MonoBehaviour
{
    private BossAttack[] attacks;

    private readonly List<BossAttack> validAttacks = new();

    private void Awake()
    {
        attacks = GetComponents<BossAttack>();
    }

    public BossAttack SelectAttack(
        BossPhaseData phase,
        PlayerStats target,
        BossAttack lastAttack)
    {
        validAttacks.Clear();

        foreach (BossAttack attack in attacks)
        {
            if (attack.CanUse(target, phase))
                validAttacks.Add(attack);
        }

        if (validAttacks.Count == 0)
            return null;

        if (validAttacks.Count > 1 && lastAttack != null)
            validAttacks.Remove(lastAttack);

        float totalWeight = 0f;

        foreach (BossAttack attack in validAttacks)
        {
            phase.TryGetWeight(attack.Data, out float weight);
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);

        foreach (BossAttack attack in validAttacks)
        {
            phase.TryGetWeight(attack.Data, out float weight);

            if (randomValue <= weight)
                return attack;

            randomValue -= weight;
        }

        return validAttacks[0];
    }
}