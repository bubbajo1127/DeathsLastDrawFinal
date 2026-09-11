using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement movement;
    private EnemyManager enemy;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleEnemyCheck()
    {
        if(inputManager.MouseHit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            enemy = inputManager.MouseHit.transform.gameObject.GetComponent<EnemyManager>();
        }
    }

    private void Attack(EnemyManager enemy)
    {
        
    }
}
