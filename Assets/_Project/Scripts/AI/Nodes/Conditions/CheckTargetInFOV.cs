using UnityEngine;
using BehaviorTree;

public class CheckTargetInFOV : Node
{
    private Transform _transform;
    private Vector2 _boxSize;
    private Vector2 _offset;
    private LayerMask _targetLayer;
    private LayerMask _obstacleLayerMask;


    // Алерт та пам'ять
    private GameObject _alertPrefab;
    private bool _alertTriggered = false;
    
    private float _memoryDuration = 2.5f; 
    private float _memoryTimer = 0f;

    private System.Action _onAggroCallback;

    public CheckTargetInFOV(Transform transform, Vector2 boxSize, Vector2 offset, LayerMask targetLayer, GameObject alertPrefab, LayerMask obstacleLayerMask, System.Action onAggroCallback = null)
    {
        _transform = transform;
        _boxSize = boxSize;
        _offset = offset;
        _targetLayer = targetLayer;
        _alertPrefab = alertPrefab;
        _obstacleLayerMask = obstacleLayerMask;
        _onAggroCallback = onAggroCallback;
    }

    public override NodeState Evaluate()
    {
        float facingDir = _transform.localScale.x > 0 ? 1 : -1;
        Vector2 centerPoint = (Vector2)_transform.position + (new Vector2(_offset.x * facingDir, _offset.y));
        Collider2D collider = Physics2D.OverlapBox(centerPoint, _boxSize, 0, _targetLayer);

        // 1. Гравець у полі зору
        if (collider != null && HasLineOfSight(_transform, collider.transform))
        {
            _memoryTimer = _memoryDuration;

            if (!_alertTriggered)
            {
                _alertTriggered = true;
                SpawnAlert();
                _onAggroCallback?.Invoke();
            }

            parent.parent.SetData("target", collider.transform);
            state = NodeState.SUCCESS;
            return state;
        }

        // 2. Гравець покинув поле зору, але моб його ще помне
        if (_memoryTimer > 0)
        {
            _memoryTimer -= Time.deltaTime;
            state = NodeState.SUCCESS;
            return state;
        }

        // 3. Гравець покинув поле зору, та моб його вже не помне
        _alertTriggered = false; // Скидаємо алерт, щоб наступного разу знову здивуватися
        parent?.parent?.ClearData("target"); // Видаляємо ціль

        state = NodeState.FAILURE;
        return state;
    }

    private void SpawnAlert()
    {
        if (_alertPrefab != null)
        {
            Vector3 spawnPos = _transform.position + new Vector3(0, 1.5f, 0);
            Object.Instantiate(_alertPrefab, spawnPos, Quaternion.identity);
        }
    }

    private bool HasLineOfSight(Transform enemyTransform, Transform playerTransform)
    {
        Vector2 startPos = (Vector2)enemyTransform.position + Vector2.up * 1f;
        Vector2 targetPos = (Vector2)playerTransform.position + Vector2.up * 1f;

        Vector2 direction = targetPos - startPos;
        float distanceToPlayer = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction.normalized, distanceToPlayer, _obstacleLayerMask);

        if (hit.collider != null)
        {
            Debug.DrawRay(startPos, direction.normalized * hit.distance, Color.red);
            return false;
        }

        Debug.DrawRay(startPos, direction, Color.green);
        return true;
    }
}