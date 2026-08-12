using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Object3: every instance owns its own combined Object1/Object2 hit count.</summary>
public class Object3Target : MonoBehaviour
{
    [SerializeField, Min(1)] private int hitsToBreak = 2;
    [SerializeField, Min(0f)] private float hitCooldown = 0.15f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite untouchedSprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Sprite brokenSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string breakTrigger = "Break";
    [SerializeField, Min(0f)] private float destroyDelay;
    [SerializeField] private UnityEvent onHit;
    [SerializeField] private UnityEvent onBroken;

    private int hitCount;
    private float nextAllowedHitTime;
    private bool broken;
    private Object lastSource;

    public int HitCount => hitCount;
    public bool IsBroken => broken;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (untouchedSprite == null && spriteRenderer != null)
        {
            untouchedSprite = spriteRenderer.sprite;
        }
    }

    public void RegisterHit(Object source)
    {
        if (broken || Time.time < nextAllowedHitTime)
        {
            return;
        }

        // Multiple colliders from the same source in one physics step count once.
        if (lastSource == source && Time.time < nextAllowedHitTime)
        {
            return;
        }

        lastSource = source;
        nextAllowedHitTime = Time.time + hitCooldown;
        hitCount++;
        onHit?.Invoke();

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
        {
            animator.SetTrigger(hitTrigger);
        }

        if (hitCount >= hitsToBreak)
        {
            Break();
        }
        else if (spriteRenderer != null && damagedSprite != null)
        {
            spriteRenderer.sprite = damagedSprite;
        }
    }

    private void Break()
    {
        broken = true;
        if (spriteRenderer != null && brokenSprite != null)
        {
            spriteRenderer.sprite = brokenSprite;
        }

        if (animator != null && !string.IsNullOrEmpty(breakTrigger))
        {
            animator.SetTrigger(breakTrigger);
        }

        foreach (Collider2D targetCollider in GetComponentsInChildren<Collider2D>())
        {
            targetCollider.enabled = false;
        }

        onBroken?.Invoke();
        if (destroyDelay > 0f)
        {
            StartCoroutine(DisableAfterDelay());
        }
    }

    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false);
    }
}
