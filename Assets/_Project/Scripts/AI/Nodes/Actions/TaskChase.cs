using UnityEngine;
using BehaviorTree;

public class TaskChase : Node
{
    private Transform _transform;
    private Rigidbody2D _rb;
    private float _speed;
    private Animator _animator;

    private LayerMask _groundLayer;
    private LayerMask _wallLayer;

    public TaskChase(Transform transform, float speed, Animator animator, LayerMask groundLayer, LayerMask wallLayer)
    {
        _transform = transform;
        _speed = speed;
        _animator = animator;
        _rb = transform.GetComponent<Rigidbody2D>();
        _groundLayer = groundLayer;
        _wallLayer = wallLayer;
    }

    public override NodeState Evaluate()
    {
        // 1. Кого доганяти?
        object t = GetData("target");
        if (t == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)t;

        // 2. Куди бігти?
        float direction = target.position.x - _transform.position.x;
        float moveX = (direction > 0) ? _speed : -_speed;

        // 3. Поворот спрайта
        if ((moveX > 0 && _transform.localScale.x < 0) ||
            (moveX < 0 && _transform.localScale.x > 0))
        {
            Flip();
        }

        // 4. Перевірка на обрив та стіну
        bool isLedge = AIUtils.IsLedgeAhead(_transform, _groundLayer, 0.5f);
        bool isWall = AIUtils.IsWallAhead(_transform, _wallLayer, 0.6f);

        if (isLedge || isWall)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            if (_animator != null) _animator.SetBool("isMoving", false);

            state = NodeState.RUNNING;
            return state;
        }

        _rb.linearVelocity = new Vector2(moveX, _rb.linearVelocity.y);
        if (_animator != null) _animator.SetBool("isMoving", true);

        state = NodeState.RUNNING;
        return state;
    }

    private void Flip()
    {
        Vector3 scaler = _transform.localScale;
        scaler.x *= -1;
        _transform.localScale = scaler;
    }
}