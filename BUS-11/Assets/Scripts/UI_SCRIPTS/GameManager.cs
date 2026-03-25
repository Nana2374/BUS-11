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
    public GameObject dialogueUI;
    public GameObject choicesUI;
    public GameObject monologueUI;

    [Header("Player Controls")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public CameraInteraction cameraInteraction;
    public SnaptoSeat snaptoSeat;
    public BusController busController;
    public DialogueManager dialogueManager;

    [Header("Blur Effect")]
    public Volume postProcessVolume;

    private bool isPaused = false;
    private bool gameStarted = false;

    private bool wasDrivingUIActive = false;
    private bool wasDriverSeatUIActive = false;
    private bool wasDialogueUIActive = false;
    private bool wasChoicesUIActive = false;
    private bool wasMonologueUIActive = false;

    private bool wasDialogueActive = false;

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

        HandleDialogueUI();
    }

    void HandleDialogueUI()
    {
        bool isDialogueActive = (dialogueUI != null && dialogueUI.activeSelf) ||
                                (choicesUI != null && choicesUI.activeSelf);

        // Dialogue just started (transition from inactive to active)
        if (isDialogueActive && !wasDialogueActive)
        {
            // Save UI state
            wasDrivingUIActive = drivingUI != null && drivingUI.activeSelf;
            wasDriverSeatUIActive = driverSeatUI != null && driverSeatUI.activeSelf;

            // Hide driving UI
            if (drivingUI != null) drivingUI.SetActive(false);
            if (driverSeatUI != null) driverSeatUI.SetActive(false);

            DialogueActive();

            Debug.Log("Dialogue started - disabled driving UI");
        }
        // Dialogue just ended (transition from active to inactive)
        else if (!isDialogueActive && wasDialogueActive)
        {
            // Restore UI state
            if (drivingUI != null) drivingUI.SetActive(wasDrivingUIActive);
            if (driverSeatUI != null) driverSeatUI.SetActive(wasDriverSeatUIActive);

            EnablePlayerControls();

            Debug.Log("Dialogue ended - restored driving UI");
        }

        // Update previous state for next frame
        wasDialogueActive = isDialogueActive;
    }

    public void ShowStartScreen()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (drivingUI != null) drivingUI.SetActive(false);
        if (driverSeatUI != null) driverSeatUI.SetActive(false);
        if (endDemoUI != null) endDemoUI.SetActive(false);
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesUI != null) choicesUI.SetActive(false);
        if (monologueUI != null) monologueUI.SetActive(false);

        AudioManager.Instance.PlayMenuMusic();

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cameraInteraction.showCrosshair = false;

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
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesUI != null) choicesUI.SetActive(false);
        if (monologueUI != null) monologueUI.SetActive(false);

        AudioManager.Instance.PlayGameMusic();
        AudioManager.Instance.ResumeSFX();

        // Unfreeze time and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraInteraction.showCrosshair = true;

        EnablePlayerControls();

        gameStarted = true;
        isPaused = false;

        Debug.Log("Game started!");
    }

    public void PauseGame()
    {
        wasDrivingUIActive = drivingUI != null && drivingUI.activeSelf;
        wasDriverSeatUIActive = driverSeatUI != null && driverSeatUI.activeSelf;
        wasDialogueUIActive = dialogueUI != null && dialogueUI.activeSelf;
        wasChoicesUIActive = choicesUI != null && choicesUI.activeSelf;
        wasMonologueUIActive = monologueUI != null && monologueUI.activeSelf;

        if (startScreen != null) startScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(true);
        if (drivingUI != null) drivingUI.SetActive(false);
        if (driverSeatUI != null) driverSeatUI.SetActive(false);
        if (endDemoUI != null) endDemoUI.SetActive(false);
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesUI != null) choicesUI.SetActive(false);
        if (monologueUI != null) monologueUI.SetActive(false);

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cameraInteraction.showCrosshair = false;

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

        // RESTORE UIs that were active before pausing
        if (drivingUI != null) drivingUI.SetActive(wasDrivingUIActive);
        if (driverSeatUI != null) driverSeatUI.SetActive(wasDriverSeatUIActive);
        if (dialogueUI != null) dialogueUI.SetActive(wasDialogueUIActive);
        if (choicesUI != null) choicesUI.SetActive(wasChoicesUIActive);
        if (monologueUI != null) monologueUI.SetActive(wasMonologueUIActive);

        // Unfreeze time and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraInteraction.showCrosshair = true;

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
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesUI != null) choicesUI.SetActive(false);
        if (monologueUI != null) monologueUI.SetActive(false);

        AudioManager.Instance.StopSFX();
        AudioManager.Instance.PlayMenuMusic();

        // Freeze time and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraInteraction.showCrosshair = false;

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

        if (cameraInteraction != null)
        {
            cameraInteraction.enabled = true;
        }

        if (snaptoSeat != null)
        {
            snaptoSeat.enabled = true;
        }

        if (busController != null)
        {
            busController.enabled = true;
        }
    }

    void DisablePlayerControls()
    {
        // Only disable movement if NOT seated (sitting animations stay active)
        if (playerMovement != null)
        {
            SnaptoSeat seatController = playerMovement.GetComponent<SnaptoSeat>();

            // Only disable movement if not sitting in driver's seat
            if (seatController == null || !seatController.isSeated)
            {
                playerMovement.enabled = false;
            }
            // If seated, leave movement script enabled but it won't work due to Time.timeScale = 0
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = false;
        }

        if (cameraInteraction != null)
        {
            cameraInteraction.enabled = false;
        }

        if (snaptoSeat != null)
        {
            snaptoSeat.enabled = false;
        }

        if (busController != null)
        {
            busController.enabled = false;
        }
    }

    void DialogueActive()
    {
        // Only disable movement if NOT seated (sitting animations stay active)
        if (playerMovement != null)
        {
            SnaptoSeat seatController = playerMovement.GetComponent<SnaptoSeat>();

            // Only disable movement if not sitting in driver's seat
            if (seatController == null || !seatController.isSeated)
            {
                playerMovement.enabled = false;
            }
            // If seated, leave movement script enabled but it won't work due to Time.timeScale = 0
        }

        if (cameraInteraction != null)
        {
            cameraInteraction.enabled = false;
        }

        if (snaptoSeat != null)
        {
            snaptoSeat.enabled = false;
        }

        if (busController != null)
        {
            busController.enabled = false;
        }
    }
}
