using UnityEngine;

/// <summary>Object2: the single moving target, constrained to an editable polygon.</summary>
public class RandomSnailMove : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 8f;
    [SerializeField, Min(0.05f)] private float targetReachDistance = 0.15f;
    [SerializeField] private Vector2 polygonOffset;
    [SerializeField] private Vector2[] movementPolygon =
    {
        new Vector2(-13.6f, -3.7f),
        new Vector2(13.6f, -3.7f),
        new Vector2(13.6f, 3.7f),
        new Vector2(-13.6f, 3.7f)
    };

    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private bool hasTarget;
    private bool destroyed;

    private void Awake()
    {
        initialPosition = transform.position;
        ChooseTarget();
    }

    private void Update()
    {
        if (destroyed || StartMenuController.Instance == null || !StartMenuController.Instance.IsPlaying)
        {
            return;
        }

        if (!hasTarget || Vector2.Distance(transform.position, targetPosition) <= targetReachDistance)
        {
            ChooseTarget();
        }

        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void DestroyByObject1()
    {
        if (destroyed)
        {
            return;
        }

        destroyed = true;
        StartMenuController.Instance?.Object2Destroyed();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed || StartMenuController.Instance == null || !StartMenuController.Instance.IsPlaying)
        {
            return;
        }

        Object3Target object3 = other.GetComponentInParent<Object3Target>();
        if (object3 != null)
        {
            object3.RegisterHit(this);
            ChooseTarget();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed || StartMenuController.Instance == null || !StartMenuController.Instance.IsPlaying)
        {
            return;
        }

        Object3Target object3 = collision.collider.GetComponentInParent<Object3Target>();
        if (object3 != null)
        {
            object3.RegisterHit(this);
            ChooseTarget();
        }
    }

    private void ChooseTarget()
    {
        if (movementPolygon == null || movementPolygon.Length < 3)
        {
            targetPosition = initialPosition;
            hasTarget = true;
            return;
        }

        Vector2 min = movementPolygon[0];
        Vector2 max = movementPolygon[0];
        for (int i = 1; i < movementPolygon.Length; i++)
        {
            min = Vector2.Min(min, movementPolygon[i]);
            max = Vector2.Max(max, movementPolygon[i]);
        }

        for (int attempt = 0; attempt < 50; attempt++)
        {
            Vector2 localPoint = new Vector2(
                Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (IsInsidePolygon(localPoint))
            {
                targetPosition = initialPosition + polygonOffset + localPoint;
                hasTarget = true;
                return;
            }
        }

        targetPosition = initialPosition + polygonOffset;
        hasTarget = true;
    }

    private bool IsInsidePolygon(Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = movementPolygon.Length - 1; i < movementPolygon.Length; j = i++)
        {
            Vector2 a = movementPolygon[i];
            Vector2 b = movementPolygon[j];
            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void OnDrawGizmosSelected()
    {
        if (movementPolygon == null || movementPolygon.Length < 2)
        {
            return;
        }

        Vector2 origin = Application.isPlaying ? initialPosition : (Vector2)transform.position;
        origin += polygonOffset;
        Gizmos.color = Color.green;
        for (int i = 0; i < movementPolygon.Length; i++)
        {
            Vector2 a = origin + movementPolygon[i];
            Vector2 b = origin + movementPolygon[(i + 1) % movementPolygon.Length];
            Gizmos.DrawLine(a, b);
        }
    }
}
