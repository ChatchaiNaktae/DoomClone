using UnityEngine;

// Interface for Enemy AI State Machine (FSM)
public interface IEnemyState
{
    void EnterState(EnemyAI enemy);
    void UpdateState(EnemyAI enemy);
    void ExitState(EnemyAI enemy);
}