using UnityEngine;

public class MeleeEnemyCombat : EnemyCombat
{
    protected override void PerformAttack(PlayerStats target)
    {
        if (target == null) return;

        if (target.IsDead()) return;

        target.TakeDamage(enemyStats.AttackDamage);

        Debug.Log(gameObject.name + " attacked the player for " + enemyStats.AttackDamage + " damage.");
    }
}
