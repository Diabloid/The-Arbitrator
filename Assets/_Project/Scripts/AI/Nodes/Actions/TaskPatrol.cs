using UnityEngine;
using BehaviorTree;

public class TaskPatrol : Node
{
    private Transform _transform;   // Посилання на трансформ ворога (для позиції та розвороту)
    private Rigidbody2D _rb;        // Для руху
    private Animator _animator;     // Для анімацій
    private float _speed;           // Швидкість руху

    // Шари для перевірки
    private LayerMask _groundLayer;
    private LayerMask _wallLayer;

    // Логіка руху
    private bool _movingRight;
    private Vector2 _startPosition; // Точка, звідки почали (Якір)
    private float _patrolRange;     // Максимальна відстань (-1 = безкінечно)

    // Таймери
    private float _waitTime = 2f;
    private float _waitCounter = 0f;
    private bool _waiting = false;

    // Конструктор
    public TaskPatrol(Transform transform, float speed, LayerMask groundLayer, LayerMask wallLayer, bool startFacingRight, float patrolRange, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _groundLayer = groundLayer;
        _wallLayer = wallLayer;
        _movingRight = startFacingRight;
        _patrolRange = patrolRange;
        _animator = animator;

        _rb = transform.GetComponent<Rigidbody2D>();
        _startPosition = transform.position; // Запам'ятовуємо ізначальну позицію

        // Якщо треба стартувати вліво - розвертаємо спрайт одразу
        if (!_movingRight)
        {
            FlipSprite();
        }
    }

    public override NodeState Evaluate()
    {
        _movingRight = _transform.localScale.x > 0;
        
        if (_waiting)
        {
            StopMovement();
            _waitCounter += Time.deltaTime;
            if (_waitCounter >= _waitTime)
            {
                _waiting = false;
                Flip();
            }
            state = NodeState.RUNNING;
            return state;
        }


        // Перевірка 1
        bool isLedge = AIUtils.IsLedgeAhead(_transform, _groundLayer, 0.5f);

        // Перевірка 2
        bool isWall = AIUtils.IsWallAhead(_transform, _wallLayer, 0.6f);

        // Перевірка 3
        float dist = Vector2.Distance(_transform.position, _startPosition);
        bool isTooFar = (_patrolRange > 0) && (dist >= _patrolRange);


        // Приймаємо рішення
        if (isLedge || isWall || isTooFar)
        {
            if (isLedge) Debug.Log($"[{_transform.name}] STOP: Яма попереду!");
            if (isWall) Debug.Log($"[{_transform.name}] STOP: Стіна!");
            if (isTooFar) Debug.Log($"[{_transform.name}] STOP: Ліміт дистанції ({dist})!");

            StopMovement();
            _waiting = true;
            _waitCounter = 0f;
        }
        else
        {
            Move();
        }

        state = NodeState.RUNNING;
        return state;
    }

    private void Move()
    {
        float moveX = _movingRight ? _speed : -_speed;
        _rb.linearVelocity = new Vector2(moveX, _rb.linearVelocity.y);

        if (_animator != null) _animator.SetBool("isMoving", true);
    }

    private void StopMovement()
    {
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

        if (_animator != null) _animator.SetBool("isMoving", false);
    }

    private void Flip()
    {
        _movingRight = !_movingRight;
        FlipSprite();
    }

    private void FlipSprite()
    {
        Vector3 scaler = _transform.localScale;
        scaler.x = _movingRight ? Mathf.Abs(scaler.x) : -Mathf.Abs(scaler.x);
        _transform.localScale = scaler;
    }
}