using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy implements IDamageable to receive hits from weapons
public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyManager enemyManager;
    private float enemyHealth = 2f;
    
    public GameObject gunHitEffect;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        if (enemyHealth <= 0)
        {   
            enemyManager.RemoveEnemy(this);
            Destroy(gameObject);
        }
    }
    
    // This method is required by the IDamageable interface
    public void TakeDamage(float damage)
    {
        if (gunHitEffect != null)
        {
            Instantiate(gunHitEffect, transform.position, Quaternion.identity);
        }
        enemyHealth -= damage;
    }
}