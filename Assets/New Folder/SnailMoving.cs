using UnityEngine;

/// <summary>Object2: starts randomly and moves between random points in a separate area.</summary>
public class RandomSnailMove : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 8f;
    [SerializeField, Min(0.05f)] private float targetReachDistance = 0.15f;
    [SerializeField] private MovementPolygonArea movementArea;

    private Vector2 targetPosition;
    private bool hasTarget;
    private bool destroyed;

    private void Awake()
    {
        if (movementArea == null)
        {
            movementArea = FindObjectOfType<MovementPolygonArea>(true);
        }

        // Object2 no longer uses its scene position as the starting point.
        if (TryChoosePoint(out Vector2 spawnPosition))
        {
            transform.position = spawnPosition;
        }

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
        if (CanReactToObject3(other.GetComponentInParent<Object3Target>()))
        {
            ChooseTarget();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CanReactToObject3(collision.collider.GetComponentInParent<Object3Target>()))
        {
            ChooseTarget();
        }
    }

    private bool CanReactToObject3(Object3Target object3)
    {
        return !destroyed &&
               StartMenuController.Instance != null &&
               StartMenuController.Instance.IsPlaying &&
               object3 != null;
    }

    private void ChooseTarget()
    {
        hasTarget = TryChoosePoint(out targetPosition);
    }

    private bool TryChoosePoint(out Vector2 point)
    {
        point = transform.position;
        return movementArea != null && movementArea.TryGetRandomPoint(out point);
    }
}
