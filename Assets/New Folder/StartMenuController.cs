using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GamePhase
{
    StartMenu,
    WaitingForRelease,
    Countdown,
    Playing,
    Success,
    Timeout
}

/// <summary>Owns the complete game flow. There must be exactly one in the scene.</summary>
public class StartMenuController : MonoBehaviour
{
    public static StartMenuController Instance { get; private set; }

    [Header("Start Menu")]
    [SerializeField] private Canvas startMenuCanvas;
    [SerializeField] private GameObject startMenuRoot;
    [SerializeField] private Image holdProgressFill;
    [SerializeField, Min(0.1f)] private float menuHoldDuration = 1f;
    [Header("Input")]
    public KeyCode controlKey = KeyCode.Space;

    [Header("Countdown")]
    [SerializeField] private GameObject countdownRoot;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField, Min(1)] private int countdownFrom = 3;
    [SerializeField, Min(0.1f)] private float countdownStepSeconds = 1f;

    [Header("Game Rules")]
    [SerializeField, Min(1f)] private float gameDuration = 30f;
    [SerializeField] private GroundBlurController groundBlur;

    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource successSoundSource;
    [SerializeField] private AudioSource countdownSoundSource;

    [Header("Ending")]
    [SerializeField] private GameObject successEndingRoot;
    [SerializeField] private GameObject timeoutEndingRoot;
    [SerializeField] private Animator endingAnimator;
    [SerializeField] private string successTrigger = "Success";
    [SerializeField] private string timeoutTrigger = "Timeout";
    [SerializeField] private UnityEvent onSuccess;
    [SerializeField] private UnityEvent onTimeout;

    [Header("Level Flow")]
    [SerializeField] private bool startMenuOnlyOnFirstLevel = true;
    [SerializeField, Min(0f)] private float nextLevelDelay = 2f;

    private float menuHeldTime;
    private float remainingTime;
    private bool transitionStarted;
    private string fallbackCountdown = string.Empty;

    public GamePhase Phase { get; private set; } = GamePhase.StartMenu;
    public bool IsPlaying => Phase == GamePhase.Playing;
    public float RemainingTime => remainingTime;
    public float GameDuration => gameDuration;
    public KeyCode ControlKey => controlKey;

    private GameObject MenuObject => startMenuRoot != null
        ? startMenuRoot
        : startMenuCanvas != null ? startMenuCanvas.gameObject : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one StartMenuController is allowed.", this);
            enabled = false;
            return;
        }

        Instance = this;
        FindSceneReferences();
        Time.timeScale = 0f;
        remainingTime = gameDuration;
        SetActive(countdownRoot, false);
        SetActive(successEndingRoot, false);
        SetActive(timeoutEndingRoot, false);
        SetHoldProgress(0f);

        bool showStartMenu = !startMenuOnlyOnFirstLevel || SceneManager.GetActiveScene().buildIndex == 0;
        SetActive(MenuObject, showStartMenu);
        if (showStartMenu)
        {
            Phase = GamePhase.StartMenu;
        }
        else
        {
            transitionStarted = true;
            StartCoroutine(StartSequence());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (Phase == GamePhase.StartMenu)
        {
            UpdateStartMenu();
        }
        else if (Phase == GamePhase.Playing)
        {
            remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
            if (remainingTime <= 0f)
            {
                EndGame(false);
            }
        }
    }

    private void OnGUI()
    {
        if (Phase == GamePhase.Countdown && countdownText == null && !string.IsNullOrEmpty(fallbackCountdown))
        {
            DrawCenteredLabel(fallbackCountdown, Mathf.RoundToInt(Screen.height * 0.18f));
        }
        else if (Phase == GamePhase.Success && successEndingRoot == null)
        {
            DrawCenteredLabel("SUCCESS", Mathf.RoundToInt(Screen.height * 0.1f));
        }
        else if (Phase == GamePhase.Timeout && timeoutEndingRoot == null)
        {
            DrawCenteredLabel("TIME UP", Mathf.RoundToInt(Screen.height * 0.1f));
        }
    }

    public void Object2Destroyed()
    {
        if (IsPlaying)
        {
            EndGame(true);
        }
    }

    private void UpdateStartMenu()
    {
        if (transitionStarted)
        {
            return;
        }

        if (Input.GetKey(controlKey))
        {
            menuHeldTime += Time.unscaledDeltaTime;
            SetHoldProgress(menuHeldTime / menuHoldDuration);
            if (menuHeldTime >= menuHoldDuration)
            {
                transitionStarted = true;
                StartCoroutine(StartSequence());
            }
        }
        else
        {
            menuHeldTime = 0f;
            SetHoldProgress(0f);
        }
    }

    private IEnumerator StartSequence()
    {
        Phase = GamePhase.WaitingForRelease;
        SetActive(MenuObject, false);
        SetActive(countdownRoot, true);

        // Consume the Space press used by the menu. Gameplay never sees it.
        while (Input.GetKey(controlKey))
        {
            yield return null;
        }

        yield return null;
        Phase = GamePhase.Countdown;

        if (countdownSoundSource != null)
        {
            countdownSoundSource.Play();
        }

        for (int value = countdownFrom; value >= 1; value--)
        {
            fallbackCountdown = value.ToString();
            if (countdownText != null)
            {
                countdownText.text = fallbackCountdown;
            }

            yield return new WaitForSecondsRealtime(countdownStepSeconds);
        }

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
        }

        fallbackCountdown = string.Empty;

        SetActive(countdownRoot, false);
        remainingTime = gameDuration;
        groundBlur?.ResetBlur();
        Time.timeScale = 1f;
        Phase = GamePhase.Playing;
        if (bgmSource != null)
        {
            bgmSource.Play();
        }
    }

    private void EndGame(bool success)
    {
        Phase = success ? GamePhase.Success : GamePhase.Timeout;
        Time.timeScale = 0f;
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
        if (success && successSoundSource != null)
        {
            successSoundSource.Play();
        }
        SetActive(successEndingRoot, success);
        SetActive(timeoutEndingRoot, !success);

        if (endingAnimator != null)
        {
            endingAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            endingAnimator.SetTrigger(success ? successTrigger : timeoutTrigger);
        }

        if (success)
        {
            onSuccess?.Invoke();
            StartCoroutine(LoadNextLevelAfterDelay());
        }
        else
        {
            onTimeout?.Invoke();
        }
    }

    private IEnumerator LoadNextLevelAfterDelay()
    {
        int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(nextLevelDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextBuildIndex);
    }

    private void FindSceneReferences()
    {
        if (startMenuCanvas == null && startMenuRoot == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name == "StartMenuCanvas")
                {
                    startMenuCanvas = canvas;
                    break;
                }
            }
        }

        if (groundBlur == null)
        {
            groundBlur = FindObjectOfType<GroundBlurController>(true);
        }
    }

    private void SetHoldProgress(float value)
    {
        if (holdProgressFill != null && holdProgressFill.type == Image.Type.Filled)
        {
            holdProgressFill.fillAmount = Mathf.Clamp01(value);
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void DrawCenteredLabel(string text, int fontSize)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(32, fontSize),
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), text, style);
    }
}