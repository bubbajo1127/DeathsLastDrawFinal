using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats enemyStats;

    public void TakeDamage(float damage)
    {
        enemyStats.TakeDamage(damage);

        if (enemyStats.IsDead())
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");

        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return enemyStats.CurrentHealth;
    }

    public float GetMaxHealth()
    {
        return enemyStats.MaxHealth;
    }
}