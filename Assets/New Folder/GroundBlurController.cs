using UnityEngine;

/// <summary>
/// Advances the ground visual every interval. A blur shader can expose the configured
/// property; until then, a subtle desaturation/alpha change provides visible feedback.
/// </summary>
public class GroundBlurController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] groundRenderers;
    [SerializeField, Min(0.1f)] private float intervalSeconds = 5f;
    [SerializeField, Min(0f)] private float blurPerStep = 0.15f;
    [SerializeField] private string blurProperty = "_BlurAmount";
    [SerializeField, Range(0f, 1f)] private float minimumFallbackBrightness = 0.7f;

    private float elapsed;
    private int appliedSteps;
    private Color[] originalColors;

    private void Awake()
    {
        if (groundRenderers == null || groundRenderers.Length == 0)
        {
            SpriteRenderer found = GetComponent<SpriteRenderer>();
            groundRenderers = found != null ? new[] { found } : new SpriteRenderer[0];
        }

        originalColors = new Color[groundRenderers.Length];
        for (int i = 0; i < groundRenderers.Length; i++)
        {
            if (groundRenderers[i] != null)
            {
                originalColors[i] = groundRenderers[i].color;
            }
        }
    }

    private void Update()
    {
        if (StartMenuController.Instance == null || !StartMenuController.Instance.IsPlaying)
        {
            return;
        }

        elapsed += Time.deltaTime;
        int expectedSteps = Mathf.FloorToInt(elapsed / intervalSeconds);
        while (appliedSteps < expectedSteps)
        {
            appliedSteps++;
            ApplyStep();
        }
    }

    public void ResetBlur()
    {
        elapsed = 0f;
        appliedSteps = 0;
        for (int i = 0; i < groundRenderers.Length; i++)
        {
            if (groundRenderers[i] != null)
            {
                groundRenderers[i].color = originalColors[i];
                Material material = groundRenderers[i].material;
                if (material != null && material.HasProperty(blurProperty))
                {
                    material.SetFloat(blurProperty, 0f);
                }
            }
        }
    }

    private void ApplyStep()
    {
        float blur = appliedSteps * blurPerStep;
        for (int i = 0; i < groundRenderers.Length; i++)
        {
            SpriteRenderer renderer = groundRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material material = renderer.material;
            if (material != null && material.HasProperty(blurProperty))
            {
                material.SetFloat(blurProperty, blur);
            }
            else
            {
                float brightness = Mathf.Max(minimumFallbackBrightness, 1f - blur * 0.25f);
                Color original = originalColors[i];
                renderer.color = new Color(
                    original.r * brightness,
                    original.g * brightness,
                    original.b * brightness,
                    original.a);
            }
        }
    }
}
