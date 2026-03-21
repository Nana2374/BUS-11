using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject startScreen;
    public GameObject pauseScreen;

    [Header("Blur Effect")]
    public Volume postProcessVolume;

    private bool isPaused = false;
    private bool gameStarted = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show start screen, hide everything else
        ShowStartScreen();

        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }
    }

    void Update()
    {
        if (!gameStarted && Input.GetKeyDown(KeyCode.F))
        {
            StartGame();
        }
            // Press ESC to pause/unpause (only after game has started)
            if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ShowStartScreen()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (pauseScreen != null) pauseScreen.SetActive(false);

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameStarted = false;
    }

    public void StartGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(false);

        // Unfreeze time and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameStarted = true;
        isPaused = false;

        Debug.Log("Game started!");
    }

    public void PauseGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(true);

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isPaused = true;

        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = true;
        }

        Debug.Log("Game paused!");
    }

    public void ResumeGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(false);

        // Unfreeze time and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;

        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }

        Debug.Log("Game resumed!");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
