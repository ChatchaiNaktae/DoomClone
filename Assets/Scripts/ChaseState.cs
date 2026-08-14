using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("Enemy started Chasing!");
    }

    public void UpdateState(EnemyAI enemy)
    {
        if (enemy.playersTransform != null)
        {
            enemy.enemyNavMeshAgent.SetDestination(enemy.playersTransform.position);
        }
        
        if (!enemy.enemyAwareness.isAggro)
        {
            enemy.ChangeState(new IdleState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.enemyNavMeshAgent.ResetPath();
    }
}