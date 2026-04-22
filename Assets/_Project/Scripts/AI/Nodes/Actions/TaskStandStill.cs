using UnityEngine;
using BehaviorTree;

public class TaskStandStill : Node
{
    private Animator _animator;
    private Transform _transform;

    public TaskStandStill(Transform transform, Animator animator)
    {
        _transform = transform;
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        Rigidbody2D rb = _transform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (_animator != null)
        {
            _animator.SetBool("isMoving", false);
        }

        state = NodeState.RUNNING;
        return state;
    }
}