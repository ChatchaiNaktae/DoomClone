using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("Enemy started Chasing!");
        AudioManager.instance.Play3D($"ImpSight{Random.Range(1, 3)}", enemy.transform.position);
    }

    public void UpdateState(EnemyAI enemy)
    {
        if (enemy.playersTransform != null)
        {
            enemy.enemyNavMeshAgent.SetDestination(enemy.playersTransform.position);
            float dist = Vector3.Distance(enemy.transform.position, enemy.playersTransform.position);
            if (dist <= enemy.attackRange && Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
            {
                enemy.ChangeState(new AttackState());
                return;
            }
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