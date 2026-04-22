using BehaviorTree;
using UnityEngine;

public class CheckAwakeState : Node
{
    private EnemyCombat _combat;

    public CheckAwakeState(EnemyCombat combat)
    {
        _combat = combat;
    }

    public override NodeState Evaluate()
    {
        // Якщо миша ще спить (галочка isAwake = false)
        if (_combat != null && !_combat.isAwake)
        {
            return NodeState.FAILURE;
        }

        // Якщо прокинулась (лампа увімкнулась + пройшла анімація)
        return NodeState.SUCCESS;
    }
}