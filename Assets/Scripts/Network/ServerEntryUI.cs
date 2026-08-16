using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerEntryUI : MonoBehaviour
{
    public TextMeshProUGUI serverNameText;
    public TextMeshProUGUI playersCountText;
    public TextMeshProUGUI statusText;
    public Button joinButton;
    
    private NetworkDiscoveryManager.DiscoveredServer serverData;
    
    public void Setup(NetworkDiscoveryManager.DiscoveredServer server, System.Action<string, ushort> onJoinCallback)
    {
        serverData = server;
        if (serverNameText != null) serverNameText.text = server.serverName;
        if (playersCountText != null) playersCountText.text = $"{server.currentPlayers}/{server.maxPlayers}";
        
        if (statusText != null)
        {
            statusText.text = server.gameStatus;
            statusText.color = (server.gameStatus == "In Lobby") ? new Color(0.2f, 0.9f, 0.3f) : new Color(0.9f, 0.5f, 0.1f);
        }
        
        // QOL: Disable button if server is completely full
        bool isFull = server.currentPlayers >= server.maxPlayers;
        if (joinButton != null)
        {
            joinButton.interactable = !isFull;
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => onJoinCallback?.Invoke(serverData.ipAddress, serverData.port));
        }
    }
}