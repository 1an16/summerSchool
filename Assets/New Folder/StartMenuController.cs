using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the flow from the start menu into gameplay.
/// The menu and the start delay use unscaled time because gameplay is paused.
/// </summary>
public class StartMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [Tooltip("Optional. If empty, a Canvas named StartMenuCanvas is found automatically.")]
    [SerializeField] private Canvas startMenuCanvas;
    [Tooltip("Optional root panel inside the Canvas. Leave empty to use the Canvas itself.")]
    [SerializeField] private GameObject startMenuRoot;
    [Tooltip("Optional UI shown during the one-second delay. Do not assign the menu root itself.")]
    [SerializeField] private GameObject startingRoot;
    [Tooltip("Optional separate Filled Image used only as the hold progress bar.")]
    [SerializeField] private Image holdProgressFill;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float holdDuration = 1f;
    [SerializeField, Min(0f)] private float startDelay = 1f;

    [Header("Gameplay")]
    [Tooltip("Scripts that must not receive input while the menu is open.")]
    [SerializeField] private Behaviour[] gameplayBehaviours;

    private float heldTime;
    private bool isStarting;
    private bool warnedAboutProgressImage;

    public float HoldProgress => Mathf.Clamp01(heldTime / holdDuration);
    public bool GameStarted { get; private set; }

    private void Awake()
    {
        FindStartMenuCanvas();

        Time.timeScale = 0f;
        SetGameplayEnabled(false);

        GameObject menuObject = GetMenuObject();
        SetActive(menuObject, true);

        if (startingRoot != menuObject)
        {
            SetActive(startingRoot, false);
        }

        UpdateProgress(0f);

        if (menuObject == null)
        {
            Debug.LogWarning(
                "StartMenuController: No Canvas was found. Name your menu Canvas 'StartMenuCanvas'.",
                this);
        }
    }

    private void Update()
    {
        if (isStarting || GameStarted)
        {
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            heldTime += Time.unscaledDeltaTime;
            UpdateProgress(HoldProgress);

            if (heldTime >= holdDuration)
            {
                StartCoroutine(BeginGameAfterDelay());
            }
        }
        else if (heldTime > 0f)
        {
            heldTime = 0f;
            UpdateProgress(0f);
        }
    }

    private IEnumerator BeginGameAfterDelay()
    {
        isStarting = true;
        GameObject menuObject = GetMenuObject();

        if (startingRoot != null && startingRoot != menuObject)
        {
            SetActive(startingRoot, true);
        }

        yield return new WaitForSecondsRealtime(startDelay);

        SetActive(menuObject, false);

        if (startingRoot != menuObject)
        {
            SetActive(startingRoot, false);
        }

        SetGameplayEnabled(true);
        Time.timeScale = 1f;
        GameStarted = true;
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (gameplayBehaviours == null)
        {
            return;
        }

        foreach (Behaviour behaviour in gameplayBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
            }
        }
    }

    private void UpdateProgress(float progress)
    {
        if (holdProgressFill != null)
        {
            if (holdProgressFill.type != Image.Type.Filled)
            {
                if (!warnedAboutProgressImage)
                {
                    Debug.LogWarning(
                        "StartMenuController: Hold Progress Fill must be a separate Image with Image Type set to Filled. " +
                        "A normal menu/background Image will be left unchanged.",
                        holdProgressFill);
                    warnedAboutProgressImage = true;
                }

                return;
            }

            holdProgressFill.fillAmount = progress;
        }
    }

    private GameObject GetMenuObject()
    {
        if (startMenuRoot != null)
        {
            return startMenuRoot;
        }

        return startMenuCanvas != null ? startMenuCanvas.gameObject : null;
    }

    private void FindStartMenuCanvas()
    {
        if (startMenuCanvas != null || startMenuRoot != null)
        {
            return;
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "StartMenuCanvas")
            {
                startMenuCanvas = canvas;
                return;
            }
        }

        if (canvases.Length > 0)
        {
            startMenuCanvas = canvases[0];
            Debug.LogWarning(
                "StartMenuController: Using the first Canvas found. Rename the intended menu Canvas to 'StartMenuCanvas'.",
                startMenuCanvas);
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}
