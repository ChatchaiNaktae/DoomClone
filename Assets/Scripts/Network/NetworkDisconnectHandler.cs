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
        if (SceneManager.GetActiveScene().name != "MainMenu" && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[Host Migration] Host disconnected! Promoting local player to Host...");
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
        
        string currentScene = SceneManager.GetActiveScene().name;
        NetworkManager.Singleton.SceneManager.LoadScene(currentScene, LoadSceneMode.Single);
    }
}