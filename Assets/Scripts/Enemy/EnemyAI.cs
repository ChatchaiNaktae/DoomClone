using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
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
    
    private void Start()
    {
        enemyAwareness = GetComponent<EnemyAwareness>();
        playersTransform = FindObjectOfType<PlayerMovement>().transform;
        enemyNavMeshAgent = GetComponent<NavMeshAgent>();
        
        enemyAnimator = GetComponentInChildren<Animator>();
        
        nextActivityTime = Time.time + UnityEngine.Random.Range(2f, 5f);

        ChangeState(new IdleState());
    }
    
    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }

        if (Time.time >= nextActivityTime)
        {
            AudioManager.instance.Play3D("ImpActivity", transform.position);
            nextActivityTime = Time.time + UnityEngine.Random.Range(4f, 8f);
        }
    }

    public void ChangeState(IEnemyState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = newState;
        currentState.EnterState(this);
    }
}
