using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject startScreen;
    public GameObject pauseScreen;
    public GameObject drivingUI;
    public GameObject driverSeatUI;
    public GameObject endDemoUI;

    [Header("Player Controls")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public BusController busController;

    [Header("Blur Effect")]
    public Volume postProcessVolume;

    private bool isPaused = false;
    private bool gameStarted = false;

    [Header("SFX")]
    public AudioClip clickSound;

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
        // Auto-find player controls if not assigned
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (mouseLook == null)
        {
            mouseLook = FindObjectOfType<MouseLook>();
        }

        if (busController == null)
        {
            busController = FindObjectOfType<BusController>();
        }

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
        // Press ESC to pause/unpause (only after game has started)
        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.Instance.PlaySFX(clickSound);
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (endDemoUI != null && endDemoUI.activeSelf && Input.GetKeyDown(KeyCode.F))
        {
            AudioManager.Instance.PlaySFX(clickSound);
            RestartGame();
        }
    }

    public void ShowStartScreen()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (drivingUI != null) drivingUI.SetActive(false);
        if (driverSeatUI != null) driverSeatUI.SetActive(false);
        if (endDemoUI != null) endDemoUI.SetActive(false);

        AudioManager.Instance.PlayMenuMusic();

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisablePlayerControls();

        gameStarted = false;
    }

    public void StartGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (drivingUI != null) drivingUI.SetActive(false);
        if (driverSeatUI != null) driverSeatUI.SetActive(false);
        if (endDemoUI != null) endDemoUI.SetActive(false);

        AudioManager.Instance.PlayGameMusic();
        AudioManager.Instance.ResumeSFX();

        // Unfreeze time and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EnablePlayerControls();

        gameStarted = true;
        isPaused = false;

        Debug.Log("Game started!");
    }

    public void PauseGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(true);
        if (drivingUI != null) drivingUI.SetActive(false);
        if (driverSeatUI != null) driverSeatUI.SetActive(false);
        if (endDemoUI != null) endDemoUI.SetActive(false);

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisablePlayerControls();

        isPaused = true;

        //AudioManager.Instance.PauseSFX();

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
        if (endDemoUI != null) endDemoUI.SetActive(false);

        // Unfreeze time and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EnablePlayerControls();

        isPaused = false;

        //AudioManager.Instance.ResumeSFX();

        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }

        Debug.Log("Game resumed!");
    }

    public void EndofDemo()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (drivingUI != null) drivingUI.SetActive(false);
        if (driverSeatUI != null) driverSeatUI.SetActive(false);
        if (endDemoUI != null) endDemoUI.SetActive(true);

        AudioManager.Instance.StopSFX();
        AudioManager.Instance.PlayMenuMusic();

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DisablePlayerControls();

        Debug.Log("End of demo reached!");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time before reloading

        // Destroy AudioManager so it recreates fresh
        if (AudioManager.Instance != null)
        {
            Destroy(AudioManager.Instance.gameObject);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void EnablePlayerControls()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = true;
        }

        if (busController != null)
        {
            busController.enabled = true;
        }
    }

    void DisablePlayerControls()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = false;
        }

        if (busController != null)
        {
            busController.enabled = false;
        }
    }
}
