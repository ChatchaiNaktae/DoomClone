using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    
    // Static flag indicating whether the current session is strictly singleplayer
    public static bool isSingleplayerMode = false;
    
    private void Start()
    {
        // Unlock cursor for menu interactions
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (hostButton != null) hostButton.onClick.AddListener(StartHostGame);
        if (joinButton != null) joinButton.onClick.AddListener(StartJoinGame);
        if (singleplayerButton != null) singleplayerButton.onClick.AddListener(StartSingleplayerGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        
        // Listen for client disconnect/rejection events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleFailedConnection;
        }
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleFailedConnection;
        }
    }
    
    public void StartHostGame()
    {
        isSingleplayerMode = false;
        StartNetworkHost();
    }
    
    public void StartSingleplayerGame()
    {
        isSingleplayerMode = true;
        StartNetworkHost();
    }
    
    private void StartNetworkHost()
    {
        if (NetworkManager.Singleton != null)
        {
            // Clean up any stale active sessions
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            // Assign connection approval check before starting host
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            
            // Load synchronized gameplay scene for all players
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
    
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Always approve the host player
        if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            return;
        }
        
        // Reject incoming clients if host is in Singleplayer mode
        if (isSingleplayerMode)
        {
            Debug.LogWarning($"[Netcode] Connection rejected for Client {request.ClientNetworkId}: Host is in Singleplayer mode.");
            response.Approved = false;
            response.Reason = "Host is currently in Singleplayer mode.";
            return;
        }
        
        // Approve clients during Co-op / Host mode
        response.Approved = true;
        response.CreatePlayerObject = true;
    }
    
    public void StartJoinGame()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            NetworkManager.Singleton.StartClient();
            StartCoroutine(CheckConnectionTimeout(4f));
        }
    }
    
    private IEnumerator CheckConnectionTimeout(float timeoutDuration)
    {
        float timer = 0f;
        while (timer < timeoutDuration)
        {
            // Break if client has successfully connected to the host
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                yield break;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Clean up socket state if timed out without establishing connection
        if (!NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("[Netcode] Connection attempt timed out. Cleaning up socket...");
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    private void HandleFailedConnection(ulong clientId)
    {
        // Triggered when client fails handshake, gets rejected, or host closes session
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[Netcode] Disconnected from server or join attempt was rejected.");
            NetworkManager.Singleton.Shutdown();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Exiting game application...");
        Application.Quit();
    }
}