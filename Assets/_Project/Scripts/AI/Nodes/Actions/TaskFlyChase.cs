using UnityEngine;
using BehaviorTree;

public class TaskFlyChase : Node
{
    private Animator _animator;
    private Transform _transform;
    private float _speed;

    private Vector2 _baseFlankPosition;
    private float _randomPhase;

    public TaskFlyChase(Transform transform, float speed, Animator animator, Vector2 baseFlankPosition)
    {
        _transform = transform;
        _speed = speed;
        _animator = animator;
        _randomPhase = Random.Range(0f, 100f);
        _baseFlankPosition = baseFlankPosition;
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if (t == null) return NodeState.FAILURE;

        Transform target = (Transform)t;

        // 1. Мікро-тремтіння
        float hoverX = Mathf.Sin(Time.time * 3f + _randomPhase) * 0.3f;
        float hoverY = Mathf.Cos(Time.time * 4f + _randomPhase) * 0.3f;

        Vector2 targetPos = (Vector2)target.position + _baseFlankPosition + new Vector2(hoverX, hoverY);
        Vector2 direction = (targetPos - (Vector2)_transform.position).normalized;

        float distanceToTarget = Vector2.Distance(_transform.position, targetPos);
        float dynamicSpeed = Mathf.Clamp(distanceToTarget * 2.5f, _speed * 0.5f, _speed * 3.0f);

        // 2. Фліп
        float distanceToPlayer = target.position.x - _transform.position.x;
        if (Mathf.Abs(distanceToPlayer) > 0.1f)
        {
            Vector3 scaler = _transform.localScale;
            scaler.x = distanceToPlayer > 0 ? Mathf.Abs(scaler.x) : -Mathf.Abs(scaler.x);
            _transform.localScale = scaler;
        }

        // 3. Сам рух
        Rigidbody2D rb = _transform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * dynamicSpeed;
        }

        if (_animator != null) _animator.SetBool("isMoving", true);

        state = NodeState.RUNNING;
        return state;
    }
}