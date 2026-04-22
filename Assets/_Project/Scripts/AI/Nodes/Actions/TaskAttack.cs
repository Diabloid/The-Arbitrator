using UnityEngine;
using BehaviorTree;

public class TaskAttack : Node
{
    private Animator _animator;
    private Transform _lastTarget;

    private float _cooldownTime;
    private float _lastAttackTime;
    private int _attackVariations;
    private float _firstAttackDelay;
    private bool _hasAttackedOnce = false;
    private float _timeInAttackRange = 0f;

    public TaskAttack(Transform transform, CharacterStats stats, Animator animator, float cooldownTime, int attackVariations = 1, float firstAttackDelay = 0.4f)
    {
        _animator = animator;
        _cooldownTime = cooldownTime;
        _attackVariations = attackVariations;
        _firstAttackDelay = firstAttackDelay;
        _lastAttackTime = -999f;
    }

    public override NodeState Evaluate()
    {
        Rigidbody2D rb = _animator.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (rb.gravityScale == 0)
                rb.linearVelocity = Vector2.zero;
            else
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (_animator != null)
        {
            _animator.SetBool("isMoving", false);
        }

        object t = GetData("target");
        if (t == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)t;
        _lastTarget = target;

        // РОЗВОРОТ
        float direction = target.position.x - _animator.transform.position.x;
        if ((direction > 0 && _animator.transform.localScale.x < 0) ||
            (direction < 0 && _animator.transform.localScale.x > 0))
        {
            Vector3 scaler = _animator.transform.localScale;
            scaler.x *= -1;
            _animator.transform.localScale = scaler;
        }

        // Логіка перезарядки
        if (!_hasAttackedOnce)
        {
            // Для першого удару перевіряємо, чи достатньо часу моб "подумав"
            _timeInAttackRange += Time.deltaTime;
            if (_timeInAttackRange >= _firstAttackDelay)
            {
                ExecuteAttack();
            }
        }
        else
        {
            // Для наступних ударів перевіряємо глобальний кулдаун
            if (Time.time >= _lastAttackTime + _cooldownTime)
            {
                ExecuteAttack();
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

    private void ExecuteAttack()
    {
        _hasAttackedOnce = true;
        _lastAttackTime = Time.time;

        if (_animator != null)
        {
            if (_attackVariations > 1)
            {
                int randomAttack = Random.Range(1, _attackVariations + 1);
                _animator.SetInteger("AttackIndex", randomAttack);
            }

            _animator.ResetTrigger("Attack");
            _animator.SetTrigger("Attack");

            EnemyCombat combat = _animator.GetComponent<EnemyCombat>();
            if (combat != null)
            {
                combat.StartAttackWithFailsafe();
            }
        }
    }
}