using UnityEngine;
using BehaviorTree;

public class CheckAttackBox : Node
{
    private Transform _transform;
    private Vector2 _boxSize;
    private Vector2 _offset;
    private LayerMask _targetLayer;

    public CheckAttackBox(Transform transform, Vector2 boxSize, Vector2 offset, LayerMask targetLayer)
    {
        _transform = transform;
        _boxSize = boxSize;
        _offset = offset;
        _targetLayer = targetLayer;
    }

    public override NodeState Evaluate()
    {
        float facingDir = _transform.localScale.x > 0 ? 1 : -1;
        Vector2 centerPoint = (Vector2)_transform.position + new Vector2(_offset.x * facingDir, _offset.y);

        Collider2D collider = Physics2D.OverlapBox(centerPoint, _boxSize, 0, _targetLayer);

        if (collider != null)
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}