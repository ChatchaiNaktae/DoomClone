using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public GameObject lobbyCamera;
    
    // Flag to check if we should auto-host after reload
    private static bool shouldAutoStartHost = false;
    
    private void Start()
    {
        // Subscribe to connection/disconnection events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
        
        // If flagged from host disconnect, start host automatically
        if (shouldAutoStartHost)
        {
            shouldAutoStartHost = false;
            StartHostMode();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
    
    private void OnGUI()
    {
        // If game is already running (Host or Client), hide the GUI buttons
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            return;
        }
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (GUILayout.Button("Start Host (Player 1)", GUILayout.Height(40)))
        {
            StartHostMode();
        }
        
        if (GUILayout.Button("Start Client (Player 2)", GUILayout.Height(40)))
        {
            NetworkManager.Singleton.StartClient();
            if (lobbyCamera != null) lobbyCamera.SetActive(false);
        }
        
        GUILayout.EndArea();
    }
    
    public void StartHostMode()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            if (lobbyCamera != null) lobbyCamera.SetActive(false);
        }
    }
    
    private void HandleClientDisconnected(ulong clientId)
    {
        // If we are a client and the host disconnected
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Host disconnected! Switching to Singleplayer mode seamlessly...");
            
            shouldAutoStartHost = true;
            
            // 1. Shut down client network connection
            NetworkManager.Singleton.Shutdown();
            
            // 2. Reload active scene fresh
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}