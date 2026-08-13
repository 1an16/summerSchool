using UnityEngine;

/// <summary>Editable world-space movement area shared with Object2.</summary>
public class MovementPolygonArea : MonoBehaviour
{
    [SerializeField] private Vector2[] vertices =
    {
        new Vector2(-10f, -4f),
        new Vector2(10f, -4f),
        new Vector2(10f, 4f),
        new Vector2(-10f, 4f)
    };

    public bool TryGetRandomPoint(out Vector2 worldPoint, int maximumAttempts = 100)
    {
        worldPoint = transform.position;
        if (vertices == null || vertices.Length < 3)
        {
            return false;
        }

        Vector2 min = vertices[0];
        Vector2 max = vertices[0];
        for (int i = 1; i < vertices.Length; i++)
        {
            min = Vector2.Min(min, vertices[i]);
            max = Vector2.Max(max, vertices[i]);
        }

        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            Vector2 localPoint = new Vector2(
                Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (ContainsLocalPoint(localPoint))
            {
                worldPoint = transform.TransformPoint(localPoint);
                return true;
            }
        }

        return false;
    }

    private bool ContainsLocalPoint(Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            Vector2 a = vertices[i];
            Vector2 b = vertices[j];
            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null || vertices.Length < 2)
        {
            return;
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 a = transform.TransformPoint(vertices[i]);
            Vector3 b = transform.TransformPoint(vertices[(i + 1) % vertices.Length]);
            Gizmos.DrawLine(a, b);
        }
    }
}
