using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkDisconnectHandler : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Host disconnected! Performing Seamless Host Migration to Singleplayer...");
            StartCoroutine(SeamlessHostMigrationRoutine());
        }
    }
    
    private IEnumerator SeamlessHostMigrationRoutine()
    {
        NetworkManager.Singleton.Shutdown();
        
        while (NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        NetworkManager.Singleton.StartHost();
        
        Scene currentScene = SceneManager.GetActiveScene();
        NetworkManager.Singleton.SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
    }
}