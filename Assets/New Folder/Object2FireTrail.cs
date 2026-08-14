using UnityEngine;

/// <summary>Animated fire sprite that follows Object2 and stretches behind its movement.</summary>
public class Object2FireTrail : MonoBehaviour
{
    [Header("Visual")]
    public Sprite fireSprite;
    public Color fireColor = Color.white;
    public int sortingOrder = 0;

    [Header("Shape")]
    [Min(0.01f)] public float width = 0.45f;
    [Min(0.01f)] public float minimumLength = 0.45f;
    [Min(0.01f)] public float maximumLength = 1.15f;
    [Min(0f)] public float distanceBehindObject = 0.35f;
    [Min(0f)] public float lengthResponse = 10f;

    [Header("Animation")]
    [Min(0f)] public float pulseAmount = 0.12f;
    [Min(0f)] public float pulseSpeed = 12f;
    [Min(0f)] public float directionSmoothness = 14f;

    private Transform trailTransform;
    private SpriteRenderer trailRenderer;
    private Vector3 previousPosition;
    private Vector2 smoothedDirection = Vector2.right;
    private float currentLength;

    private void Awake()
    {
        GameObject visual = new GameObject(name + " Fire Trail");
        trailTransform = visual.transform;
        trailRenderer = visual.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = fireSprite;
        trailRenderer.color = fireColor;
        trailRenderer.sortingOrder = sortingOrder;
        previousPosition = transform.position;
        currentLength = minimumLength;
    }

    private void LateUpdate()
    {
        if (trailTransform == null)
        {
            return;
        }
//让他们给竖直向上的素材
//有一个distance参数可以改fire和牛之间的距离

        Vector2 displacement = transform.position - previousPosition;
        previousPosition = transform.position;
        float speed = Time.deltaTime > 0f ? displacement.magnitude / Time.deltaTime : 0f;

        if (displacement.sqrMagnitude > 0.000001f)
        {
            Vector2 desiredDirection = displacement.normalized;
            float directionBlend = 1f - Mathf.Exp(-directionSmoothness * Time.unscaledDeltaTime);
            smoothedDirection = Vector2.Lerp(smoothedDirection, desiredDirection, directionBlend).normalized;
        }

        float speedRatio = Mathf.Clamp01(speed / Mathf.Max(0.01f, lengthResponse));
        float targetLength = Mathf.Lerp(minimumLength, maximumLength, speedRatio);
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        currentLength = Mathf.Lerp(
            currentLength, targetLength, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));

        Vector2 trailDirection = -smoothedDirection;
        trailTransform.position = (Vector2)transform.position +
                                  trailDirection * (distanceBehindObject + currentLength * 0.5f);
        trailTransform.rotation = Quaternion.FromToRotation(Vector3.up, trailDirection);
        trailTransform.localScale = new Vector3(width / pulse, currentLength * pulse, 1f);

        bool gameRunning = StartMenuController.Instance != null && StartMenuController.Instance.IsPlaying;
        trailRenderer.enabled = gameRunning && fireSprite != null && speed > 0.01f;
    }

    private void OnDisable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
    }

    private void OnDestroy()
    {
        if (trailTransform != null)
        {
            Destroy(trailTransform.gameObject);
        }
    }
}
