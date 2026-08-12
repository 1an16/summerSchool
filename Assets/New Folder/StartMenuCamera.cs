using UnityEngine;
using System.Collections;

public class StartMenuController : MonoBehaviour
{
    public Camera menuCamera;
    public Camera mainCamera;

    public RotateShootReturn2D shootController;

    public KeyCode startKey = KeyCode.JoystickButton1;

    public float holdTime = 1f;
    public float startDelay = 3f;

    private float currentHoldTime = 0f;
    private bool gameStarted = false;


    void Start()
    {
        menuCamera.enabled = true;
        mainCamera.enabled = false;

        // 开始菜单期间完全关闭发射脚本
        shootController.enabled = false;

        Time.timeScale = 0f;
    }


    void Update()
    {
        if (gameStarted)
            return;

        if (Input.GetKey(startKey))
        {
            currentHoldTime += Time.unscaledDeltaTime;

            if (currentHoldTime >= holdTime)
            {
                StartGame();
            }
        }
        else
        {
            currentHoldTime = 0f;
        }
    }


    void StartGame()
    {
        gameStarted = true;

        menuCamera.enabled = false;
        mainCamera.enabled = true;

        StartCoroutine(DelayStart());
    }


    IEnumerator DelayStart()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        Time.timeScale = 1f;

        // 游戏正式开始，再打开控制脚本
        shootController.enabled = true;
    }
}