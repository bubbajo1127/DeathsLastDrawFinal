using UnityEngine;

public class MeleeEnemyCombat : EnemyCombat
{
    protected override void PerformAttack(PlayerStats target)
    {
        target.TakeDamage(enemyStats.AttackDamage);

        Debug.Log(gameObject.name + " attacked the player for " + enemyStats.AttackDamage + " damage.");
    }
}
