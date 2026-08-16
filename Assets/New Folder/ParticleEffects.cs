using UnityEngine;

/// <summary>Spawns particle effects at runtime. No prefab or editor setup needed.</summary>
public static class ParticleEffects
{
    /// <summary>Glass shard burst for bottle breaking.</summary>
    public static void SpawnBottleBreak(Vector3 position, Color shardColor)
    {
        GameObject go = new GameObject("BottleBreakParticles");
        go.transform.position = position;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ConfigureBottleBreak(ps, shardColor);
        ps.Play();
        go.AddComponent<AutoDestroyParticle>();
    }

    /// <summary>Burst effect for snail destruction.</summary>
    public static void SpawnSnailDestroy(Vector3 position)
    {
        GameObject go = new GameObject("SnailDestroyParticles");
        go.transform.position = position;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ConfigureSnailDestroy(ps);
        ps.Play();
        go.AddComponent<AutoDestroyParticle>();
    }

    private static void ConfigureBottleBreak(ParticleSystem ps, Color shardColor)
    {
        var main = ps.main;
        main.startColor = shardColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startLifetime = 0.8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.maxParticles = 25;
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new[] { new Keyframe(0f, 1f), new Keyframe(1f, 0f) }));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(shardColor, 0f), new GradientColorKey(shardColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }
    }

    private static void ConfigureSnailDestroy(ParticleSystem ps)
    {
        Color snailColor = new Color(0.4f, 0.8f, 0.3f);

        var main = ps.main;
        main.startColor = snailColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startLifetime = 0.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.maxParticles = 20;
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.4f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new[] { new Keyframe(0f, 1f), new Keyframe(1f, 0f) }));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(snailColor, 0f), new GradientColorKey(snailColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }
    }
}

/// <summary>Auto-destroys the GameObject once its ParticleSystem finishes.</summary>
public class AutoDestroyParticle : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (ps != null && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
