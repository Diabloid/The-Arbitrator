using UnityEngine;

public static class AIUtils
{
    // Цей метод повертає TRUE, якщо попереду прірва
    public static bool IsLedgeAhead(Transform transform, LayerMask groundLayer, float lookAheadDist = 1.0f)
    {
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        Vector2 origin = transform.position;
        Vector2 checkPoint = origin + new Vector2(facingDir * lookAheadDist, 0);

        Debug.DrawRay(checkPoint, Vector2.down * 2f, Color.red, 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(checkPoint, Vector2.down, 2f, groundLayer);

        return hit.collider == null;
    }

    public static bool IsWallAhead(Transform transform, LayerMask wallLayer, float checkDist = 0.5f)
    {
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.8f;

        origin += Vector2.right * facingDir * 0.1f;

        Debug.DrawRay(origin, Vector2.right * facingDir * checkDist, Color.blue, 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * facingDir, checkDist, wallLayer);

        return hit.collider != null;
    }
}