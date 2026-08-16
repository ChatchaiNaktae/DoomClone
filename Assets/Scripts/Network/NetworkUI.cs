using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public GameObject lobbyCamera;
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host (Player 1)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartHost();
                if (lobbyCamera != null)
                {
                    lobbyCamera.SetActive(false);
                }
            }
            
            if (GUILayout.Button("Start Client (Player 2)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartClient();
                if (lobbyCamera != null)
                {
                    lobbyCamera.SetActive(false);
                }
            }
        }
        
        GUILayout.EndArea();
    }
}