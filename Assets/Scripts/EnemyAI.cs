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
    
    private IEnemyState currentState;
    
    private void Start()
    {
        enemyAwareness = GetComponent<EnemyAwareness>();
        playersTransform = FindObjectOfType<PlayerMovement>().transform;
        enemyNavMeshAgent = GetComponent<NavMeshAgent>();

        ChangeState(new IdleState());
    }
    
    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
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
