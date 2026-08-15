using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("Enemy is Attacking!");
        
        enemy.enemyNavMeshAgent.isStopped = true;
        if (enemy.enemyAnimator != null)
        {
            enemy.enemyAnimator.SetTrigger("AttackTrigger");
        }
        
        if (enemy.playersTransform != null)
        {
            PlayerHealth playerHealth = enemy.playersTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer(15); 
            }
        }
        
        enemy.lastAttackTime = Time.time;
    }
    
    public void UpdateState(EnemyAI enemy)
    {
        if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            enemy.ChangeState(new ChaseState());
        }
    }
    
    public void ExitState(EnemyAI enemy)
    {
        enemy.enemyNavMeshAgent.isStopped = false;
    }
}