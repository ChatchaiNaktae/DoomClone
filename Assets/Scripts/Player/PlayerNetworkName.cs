using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerNetworkName : NetworkBehaviour
{
    [Header("UI Reference")]
    public GameObject nameTagCanvas;
    public TextMeshProUGUI nameText;
    
    // Synchronized Network Variable for Player Name
    public NetworkVariable<FixedString64Bytes> NetworkPlayerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    
    private string GetUniquePlayerPrefsKey(string baseKey)
    {
        #if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            return $"{baseKey}_{customArgument}";
        }
        #endif
        return baseKey;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        NetworkPlayerName.OnValueChanged += OnNameChanged;
        
        if (IsOwner)
        {
            string nameKey = GetUniquePlayerPrefsKey("PlayerUsername");
            string savedName = PlayerPrefs.GetString(nameKey, "Player");
            
            NetworkPlayerName.Value = new FixedString64Bytes(savedName);
            
            if (nameTagCanvas != null)
            {
                nameTagCanvas.SetActive(false);
            }
        }
        else
        {
            UpdateNameDisplay(NetworkPlayerName.Value.ToString());
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkPlayerName.OnValueChanged -= OnNameChanged;
    }
    
    private void OnNameChanged(FixedString64Bytes oldVal, FixedString64Bytes newVal)
    {
        UpdateNameDisplay(newVal.ToString());
    }
    
    private void UpdateNameDisplay(string displayName)
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }
    }
    
    private void LateUpdate()
    {
        // Make the nametag face the local active camera (Billboard effect)
        if (nameTagCanvas != null && nameTagCanvas.activeSelf)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                nameTagCanvas.transform.LookAt(nameTagCanvas.transform.position + mainCam.transform.forward);
            }
        }
    }
}