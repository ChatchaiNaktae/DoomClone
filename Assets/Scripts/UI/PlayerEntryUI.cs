using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEntryUI : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI pingText;
    public Image pingIndicatorImage;
    
    public ulong TargetClientId { get; private set; }
    
    public void Setup(ulong clientId, string playerName, bool isHost)
    {
        TargetClientId = clientId;
        
        if (playerNameText != null)
        {
            playerNameText.text = isHost ? $"{playerName} [HOST]" : playerName;
        }
        
        // Default
        UpdatePingDisplay(0);
    }
    
    public void UpdatePingDisplay(int rttMs)
    {
        if (pingText == null) return;
        
        pingText.text = $"{rttMs} ms";
        
        // Adjust color based on latency level.
        Color pingColor;
        if (rttMs < 60)
        {
            pingColor = new Color(0.2f, 0.9f, 0.3f); // เขียว
        }
        else if (rttMs < 130)
        {
            pingColor = new Color(0.95f, 0.75f, 0.1f); // เหลือง/ส้ม
        }
        else
        {
            pingColor = new Color(0.9f, 0.2f, 0.2f); // แดง
        }
        
        pingText.color = pingColor;
        if (pingIndicatorImage != null) pingIndicatorImage.color = pingColor;
    }
}