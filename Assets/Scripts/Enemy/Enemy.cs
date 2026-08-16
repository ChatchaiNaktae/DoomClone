using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

// Enemy implements IDamageable to receive hits from weapons
public class Enemy : NetworkBehaviour, IDamageable
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
    }
    
    // This method is required by the IDamageable interface
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        RequestDamageServerRpc(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(float damage)
    {
        if (isDead) return;
        
        enemyHealth -= damage;
        
        PlayHitEffectClientRpc();
        
        if (enemyHealth <= 0)
        {   
            Die();
        }
    }
    
    [ClientRpc]
    private void PlayHitEffectClientRpc()
    {
        if (gunHitEffect != null)
        {
            Instantiate(gunHitEffect, transform.position, Quaternion.identity);
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        DieClientRpc();
    }
    
    [ClientRpc]
    private void DieClientRpc()
    {
        isDead = true;
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play3D($"ImpDeath{Random.Range(1, 3)}", transform.position);
        }
        
        if (spriteAnim != null)
        {
            spriteAnim.SetTrigger("DeathTrigger");
        }
        
        if (enemyManager != null)
        {
            enemyManager.RemoveEnemy(this);
        }
        
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;
        
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }
}