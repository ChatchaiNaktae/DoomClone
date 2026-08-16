using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    public EnemyAwareness enemyAwareness;
    public Transform playersTransform;
    public NavMeshAgent enemyNavMeshAgent;
    
    public Animator enemyAnimator;
    
    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    [HideInInspector] public float lastAttackTime;
    
    private IEnemyState currentState;
    private float nextActivityTime;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        enemyAwareness = GetComponent<EnemyAwareness>();
        enemyNavMeshAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponentInChildren<Animator>();
        
        if (!IsServer)
        {
            if (enemyNavMeshAgent != null) enemyNavMeshAgent.enabled = false;
            return;
        }
        
        FindTargetPlayer();
        nextActivityTime = Time.time + UnityEngine.Random.Range(2f, 5f);
        ChangeState(new IdleState());
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        if (enemyAwareness != null && enemyAwareness.playerTransform != null)
        {
            playersTransform = enemyAwareness.playerTransform;
        }
        else if (playersTransform == null)
        {
            FindTargetPlayer();
        }
        
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
        
        if (Time.time >= nextActivityTime)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play3D("ImpActivity", transform.position);
            }
            nextActivityTime = Time.time + UnityEngine.Random.Range(4f, 8f);
        }
    }
    
    public void FindTargetPlayer()
    {
        playersTransform = NetworkUtils.GetClosestPlayer(transform.position);
    }
    
    public void ChangeState(IEnemyState newState)
    {
        if (!IsServer) return;

        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = newState;
        currentState.EnterState(this);
    }
}