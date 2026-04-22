using UnityEngine;
using BehaviorTree;

public class TaskEvade : Node
{
    private Transform _transform;
    private EnemyCombat _combat;
    private Animator _animator;
    private CloakController _cloak;
    private float _evadeSpeed;

    public TaskEvade(Transform transform, EnemyCombat combat, Animator animator, CloakController cloak, float evadeSpeed)
    {
        _transform = transform;
        _combat = combat;
        _animator = animator;
        _cloak = cloak;
        _evadeSpeed = evadeSpeed;
    }

    public override NodeState Evaluate()
    {
        if (_combat != null && _combat.isEvading)
        {
            Rigidbody2D rb = _transform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (_cloak != null) _cloak.SetCloak(true);

                float facingDir = _transform.localScale.x > 0 ? 1f : -1f;

                rb.linearVelocity = new Vector2(-facingDir * _evadeSpeed, rb.linearVelocity.y);
            }

            if (_animator != null) _animator.SetBool("isMoving", true);

            state = NodeState.RUNNING;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}