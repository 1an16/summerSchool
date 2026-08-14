using UnityEngine;

/// <summary>Attaches to the main camera. Call Shake() to trigger a screen shake.</summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField, Min(0f)] private float duration = 0.15f;
    [SerializeField, Min(0f)] private float magnitude = 0.3f;

    private Vector3 originalPosition;
    private float shakeTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        originalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            float t = Mathf.Max(0f, shakeTimer / duration);
            Vector2 offset = Random.insideUnitCircle * magnitude * t;
            transform.localPosition = originalPosition + (Vector3)offset;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    public void Shake()
    {
        Shake(duration, magnitude);
    }

    public void Shake(float shakeDuration, float shakeMagnitude)
    {
        shakeTimer = Mathf.Max(shakeTimer, shakeDuration);
        // Update magnitude for this shake if stronger
        magnitude = Mathf.Max(magnitude, shakeMagnitude);
        duration = shakeDuration;
    }
}
