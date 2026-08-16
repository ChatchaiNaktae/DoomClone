using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class Enemy : NetworkBehaviour, IDamageable
{
    private EnemyManager enemyManager;
    private Animator spriteAnim;
    private AngleToPlayer angleToPlayer;
    
    private float enemyHealth = 2f;
    public GameObject gunHitEffect;
    
    // Synced death state across all clients and late joiners
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        spriteAnim = GetComponentInChildren<Animator>();
        angleToPlayer = GetComponent<AngleToPlayer>();
        enemyManager = FindObjectOfType<EnemyManager>();
        
        // Listen for death state changes
        isDead.OnValueChanged += OnDeathStateChanged;
        
        // If late-joining client enters and enemy is already dead
        if (isDead.Value)
        {
            ApplyDeathState(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isDead.OnValueChanged -= OnDeathStateChanged;
    }
    
    void Update()
    {
        if (isDead.Value || spriteAnim == null || angleToPlayer == null) return;
        
        spriteAnim.SetFloat("spriteRot", angleToPlayer.lastIndex);
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead.Value) return;
        
        RequestDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(float damage)
    {
        if (isDead.Value) return;
        
        enemyHealth -= damage;
        PlayHitEffectClientRpc();
        
        if (enemyHealth <= 0)
        {   
            isDead.Value = true; // Server updates NetworkVariable -> triggers on all clients
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
    
    private void OnDeathStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            ApplyDeathState(true);
        }
    }
    
    private void ApplyDeathState(bool playAudio)
    {
        if (playAudio && AudioManager.instance != null)
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