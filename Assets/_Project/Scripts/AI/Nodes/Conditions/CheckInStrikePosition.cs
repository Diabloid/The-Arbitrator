using UnityEngine;
using BehaviorTree;

public class CheckInStrikePosition : Node
{
    private Transform _transform;
    private Vector2 _flankPosition;
    private float _threshold;

    public CheckInStrikePosition(Transform transform, Vector2 flankPosition, float threshold = 0.5f)
    {
        _transform = transform;
        _flankPosition = flankPosition;
        _threshold = threshold; // Допуск похибки
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if (t == null) return NodeState.FAILURE;

        Transform target = (Transform)t;

        Vector2 targetPos = (Vector2)target.position + _flankPosition;

        if (Vector2.Distance(_transform.position, targetPos) <= _threshold)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}