using UnityEngine;
using BehaviorTree;

public class TaskFlankTeleport : Node
{
    private Transform _transform;
    private EnemyCombat _combat;
    private LayerMask _groundLayer;
    private float _teleportDistance;
    private float _teleportChance;

    public TaskFlankTeleport(Transform transform, EnemyCombat combat, LayerMask groundLayer, float teleportDistance = 6f, float teleportChance = 0.5f)
    {
        _transform = transform;
        _combat = combat;
        _groundLayer = groundLayer;
        _teleportDistance = teleportDistance;
        _teleportChance = teleportChance;
    }

    public override NodeState Evaluate()
    {
        if (_combat == null || !_combat.readyToTeleport)
        {
            return NodeState.FAILURE;
        }
        _combat.readyToTeleport = false;

        if (Random.value > _teleportChance)
        {
            return NodeState.FAILURE;
        }

        // Якщо шанс спрацював в пользу телепорту, виконується:
        object t = GetData("target");
        if (t == null) return NodeState.FAILURE;
        Transform target = (Transform)t;

        float currentSide = Mathf.Sign(_transform.position.x - target.position.x);
        float targetX = target.position.x + (-currentSide * _teleportDistance);

        RaycastHit2D currentGround = Physics2D.Raycast(_transform.position, Vector2.down, 5f, _groundLayer);
        if (currentGround.collider == null) return NodeState.FAILURE;

        float currentGroundY = currentGround.point.y;

        Vector2 rayOrigin = new Vector2(targetX, target.position.y + 5f);
        RaycastHit2D newGround = Physics2D.Raycast(rayOrigin, Vector2.down, 10f, _groundLayer);

        if (newGround.collider != null && Mathf.Abs(newGround.point.y - currentGroundY) < 0.5f)
        {
            // Сама телепортація
            _transform.position = new Vector2(targetX, newGround.point.y + 1f);

            float facingDir = target.position.x - _transform.position.x;
            Vector3 scaler = _transform.localScale;
            scaler.x = facingDir > 0 ? Mathf.Abs(scaler.x) : -Mathf.Abs(scaler.x);
            _transform.localScale = scaler;

            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}