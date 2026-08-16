using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    public string CurrentJoinCode { get; private set; } = "";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // Initialize Unity Gaming Services and sign in anonymously
    public async Task<bool> InitializeServicesAsync()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[Relay] Signed in anonymously. PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Initialization error: {ex.Message}");
            return false;
        }
    }
    
    // Host allocates a relay server and retrieves a 6-character Join Code
    public async Task<string> CreateRelayHostAsync(int maxPlayers)
    {
        bool isReady = await InitializeServicesAsync();
        if (!isReady) return null;
        
        try
        {
            // Reserve allocation slot on Unity Relay Server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            CurrentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // Bind Relay Server Data directly into UnityTransport
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            Debug.Log($"[Relay] Host created successfully. Join Code: {CurrentJoinCode}");
            return CurrentJoinCode;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Failed to create Relay Host: {ex.Message}");
            return null;
        }
    }
    
    // Client connects to the host using the 6-character Join Code
    public async Task<bool> JoinRelayAsync(string joinCode)
    {
        bool isReady = await InitializeServicesAsync();
        if (!isReady) return false;
        
        try
        {
            // Resolve Join Code through Unity Relay Service
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            // Bind Client Relay Data directly into UnityTransport
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            CurrentJoinCode = joinCode;
            Debug.Log($"[Relay] Connected to Join Code: {joinCode}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Failed to join Relay with code [{joinCode}]: {ex.Message}");
            return false;
        }
    }
}