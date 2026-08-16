using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public GameObject lobbyCamera;
    
    private void Start()
    {
        // Subscribe to connection/disconnection events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
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
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host (Player 1)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartHost();
                if (lobbyCamera != null) lobbyCamera.SetActive(false);
            }
            
            if (GUILayout.Button("Start Client (Player 2)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartClient();
                if (lobbyCamera != null) lobbyCamera.SetActive(false);
            }
        }
        
        GUILayout.EndArea();
    }
    
    private void HandleClientDisconnected(ulong clientId)
    {
        // If we are a client and the host disconnected (clientId 0 is always Host/Server)
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Host disconnected! Switching to Singleplayer mode...");
            
            // 1. Shut down client network connection
            NetworkManager.Singleton.Shutdown();
            
            // 2. Restart as Host instantly or Reload current scene to play singleplayer
            // Option A: Reload scene fresh for clean singleplayer
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
            
            // Note: After reload, player can immediately click "Start Host" to play solo or invite new friends.
        }
    }
}