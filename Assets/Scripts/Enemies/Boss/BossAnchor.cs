using System;
using UnityEngine;

public class BossAnchor : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;

    public event Action<BossAnchor> OnDestroyed;

    private void OnEnable()
    {
        enemyManager.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (enemyManager != null) enemyManager.OnDied -= HandleDeath;
    }

    private void HandleDeath(EnemyManager enemy)
    {
        OnDestroyed?.Invoke(this);
    }
}
