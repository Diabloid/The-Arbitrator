using UnityEngine;
using BehaviorTree;

public class CheckEnemyInAttackRange : Node
{
    private Transform _transform;
    private float _attackRange;
    private LayerMask _playerLayer;

    // Для ефекту "!"
    private GameObject _alertPrefab;
    private bool _isAlerted = false;

    public CheckEnemyInAttackRange(Transform transform, float attackRange, LayerMask playerLayer, GameObject alertPrefab)
    {
        _transform = transform;
        _attackRange = attackRange;
        _playerLayer = playerLayer;
        _alertPrefab = alertPrefab;
    }

    public override NodeState Evaluate()
    {
        // Перевіряємо, чи є хтось на шарі "Player" навколо ворога
        Collider2D collider = Physics2D.OverlapCircle(_transform.position, _attackRange, _playerLayer);

        if (collider != null)
        {
            if (!_isAlerted)
            {
                _isAlerted = true;
                if (_alertPrefab != null)
                {
                    Vector3 spawnPos = _transform.position + new Vector3(0, 1.5f, 0);
                    Object.Instantiate(_alertPrefab, spawnPos, Quaternion.identity);
                }
            }

            // Запам'ятовуємо в "пам'яті" дерева, щоб вузол Атаки знав, кого бити
            parent.parent.SetData("target", collider.transform);

            state = NodeState.SUCCESS;
            return state;
        }
        _isAlerted = false;

        state = NodeState.FAILURE;
        return state;
    }
}