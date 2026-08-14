using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        enemy.enemyNavMeshAgent.isStopped = true;
        Debug.Log("Enemy is now Idle.");
    }

    public void UpdateState(EnemyAI enemy)
    {
        if (enemy.enemyAwareness.isAggro)
        {
            enemy.ChangeState(new ChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.enemyNavMeshAgent.isStopped = false;
    }
}
