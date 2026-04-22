using UnityEngine;
using BehaviorTree;

public class CheckBlockState : Node
{
    private EnemyCombat _combat;
    private Rigidbody2D _rb;

    public CheckBlockState(EnemyCombat combat, Rigidbody2D rb)
    {
        _combat = combat;
        _rb = rb;
    }

    public override NodeState Evaluate()
    {
        if (_combat != null && _combat.isBlocking)
        {
            if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

            return NodeState.RUNNING;
        }

        return NodeState.FAILURE;
    }
}