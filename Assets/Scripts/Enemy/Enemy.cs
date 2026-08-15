using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Enemy implements IDamageable to receive hits from weapons
public class Enemy : MonoBehaviour, IDamageable
{
    private EnemyManager enemyManager;
    private Animator spriteAnim;
    private AngleToPlayer angleToPlayer;
    
    private float enemyHealth = 2f;
    public GameObject gunHitEffect;
    
    private bool isDead = false;
    
    void Start()
    {
        spriteAnim = GetComponentInChildren<Animator>();
        angleToPlayer = GetComponent<AngleToPlayer>();
        enemyManager = FindObjectOfType<EnemyManager>();
    }
    
    void Update()
    {
        if (isDead || spriteAnim == null || angleToPlayer == null) return;
        
        // beginning of update set the animations rotational index
        spriteAnim.SetFloat("spriteRot", angleToPlayer.lastIndex);
        
        if (enemyHealth <= 0)
        {   
            Die();
        }
    }
    
    // This method is required by the IDamageable interface
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        if (gunHitEffect != null)
        {
            Instantiate(gunHitEffect, transform.position, Quaternion.identity);
        }
        enemyHealth -= damage;
    }

    private void Die()
    {
        isDead = true;
        AudioManager.instance.Play3D($"ImpDeath{Random.Range(1, 3)}", transform.position);
        spriteAnim.SetTrigger("DeathTrigger");
        if (enemyManager != null)
        {
            enemyManager.RemoveEnemy(this);
        }
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }
}