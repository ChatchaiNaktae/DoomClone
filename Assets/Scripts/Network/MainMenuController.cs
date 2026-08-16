using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("The exact name of your gameplay scene")]
    public string gameplaySceneName = "DoomClone";
    
    [Header("Menu Buttons")]
    public Button hostButton;
    public Button joinButton;
    public Button singleplayerButton;
    public Button quitButton;
    
    private void Start()
    {
        // Unlock cursor in main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (hostButton != null) hostButton.onClick.AddListener(StartHostGame);
        if (joinButton != null) joinButton.onClick.AddListener(StartJoinGame);
        if (singleplayerButton != null) singleplayerButton.onClick.AddListener(StartSingleplayerGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }
    
    public void StartHostGame()
    {
        if (NetworkManager.Singleton != null)
        {
            // Start hosting session
            NetworkManager.Singleton.StartHost();
            
            // Load gameplay scene synchronized across all connected players
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }
    
    public void StartSingleplayerGame()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }
    
    public void StartJoinGame()
    {
        if (NetworkManager.Singleton != null)
        {
            // Start client and automatically load whatever scene the host is currently in
            NetworkManager.Singleton.StartClient();
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit();
    }
}