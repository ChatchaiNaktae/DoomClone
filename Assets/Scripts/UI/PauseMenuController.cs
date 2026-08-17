using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    
    [Header("Pause Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button quitToMenuButton;
    
    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";
    
    public static bool IsPaused { get; private set; } = false;
    
    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(QuitToMainMenu);
        
        IsPaused = false;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        IsPaused = true;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        
        if (restartButton != null)
        {
            bool canRestart = MainMenuController.isSingleplayerMode || 
                              (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer);
            restartButton.gameObject.SetActive(canRestart);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (MainMenuController.isSingleplayerMode)
        {
            Time.timeScale = 0f;
        }
    }
    
    public void ResumeGame()
    {
        IsPaused = false;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (MainMenuController.isSingleplayerMode)
        {
            Time.timeScale = 1f;
        }
    }
    
    public void RestartGame()
    {
        if (!MainMenuController.isSingleplayerMode)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[PauseMenu] Only the Host can restart the level.");
                return;
            }
        }
        
        Time.timeScale = 1f;
        IsPaused = false;
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
        else if (MainMenuController.isSingleplayerMode)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void OpenSettings()
    {
        Debug.Log("[PauseMenu] Opening Settings Panel...");
    }
    
    public void QuitToMainMenu()
    {
        CleanupAndReturnToMenu();
    }
    
    private void CleanupAndReturnToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        
        if (NetworkDiscoveryManager.Instance != null)
        {
            NetworkDiscoveryManager.Instance.StopListening();
        }
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();
        foreach (var p in players)
        {
            Destroy(p.gameObject);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}