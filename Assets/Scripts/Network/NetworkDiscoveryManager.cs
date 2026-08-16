using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkDiscoveryManager : MonoBehaviour
{
    public static NetworkDiscoveryManager Instance { get; private set; }
    
    private const int DISCOVERY_PORT = 47777;
    private Socket broadcastSocket;
    private Socket listenerSocket;
    private byte[] receiveBuffer = new byte[1024];
    private float broadcastTimer = 0f;
    private bool isListening = false;
    
    private readonly Queue<DiscoveredServer> incomingQueue = new Queue<DiscoveredServer>();
    
    [System.Serializable]
    public class DiscoveredServer
    {
        public string serverName;
        public string ipAddress;
        public ushort port;
        public int currentPlayers;
        public int maxPlayers;
        public string gameStatus;
        public string relayCode;
        public float lastSeenTimestamp;
    }
    
    public Dictionary<string, DiscoveredServer> discoveredServers = new Dictionary<string, DiscoveredServer>();
    
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
    
    private void Update()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && !MainMenuController.isSingleplayerMode)
        {
            broadcastTimer += Time.deltaTime;
            if (broadcastTimer >= 1.0f)
            {
                broadcastTimer = 0f;
                BroadcastServerInfo();
            }
        }
        
        lock (incomingQueue)
        {
            while (incomingQueue.Count > 0)
            {
                var server = incomingQueue.Dequeue();
                server.lastSeenTimestamp = Time.time;
                string serverKey = $"{server.serverName}_{server.port}";
                discoveredServers[serverKey] = server;
            }
        }
        
        PruneStaleServers();
    }
    
    public void StartListening()
    {
        discoveredServers.Clear();
        lock (incomingQueue)
        {
            incomingQueue.Clear();
        }
        
        if (isListening) return;
        
        try
        {
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listenerSocket.Bind(new IPEndPoint(IPAddress.Any, DISCOVERY_PORT));
            
            isListening = true;
            EndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);
            listenerSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref senderEP, OnReceiveBroadcast, senderEP);
            Debug.Log("[Discovery] Started Listening on port " + DISCOVERY_PORT);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Discovery] Listener Bind error: {ex.Message}");
        }
    }
    
    private void OnReceiveBroadcast(IAsyncResult ar)
    {
        if (!isListening || listenerSocket == null) return;
        
        try
        {
            EndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = listenerSocket.EndReceiveFrom(ar, ref senderEP);
            string message = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
            
            string[] parts = message.Split('|');
            if (parts.Length >= 5)
            {
                IPEndPoint senderIPEP = (IPEndPoint)senderEP;
                string ip = senderIPEP.Address.ToString();
                if (ip == "::1" || ip == "0.0.0.0") ip = "127.0.0.1";
                
                string relayCode = (parts.Length >= 6) ? parts[5] : "";
                
                DiscoveredServer server = new DiscoveredServer
                {
                    serverName = parts[0],
                    ipAddress = ip,
                    port = ushort.Parse(parts[1]),
                    currentPlayers = int.Parse(parts[2]),
                    maxPlayers = int.Parse(parts[3]),
                    gameStatus = parts[4],
                    relayCode = relayCode,
                    lastSeenTimestamp = 0f
                };
                
                lock (incomingQueue)
                {
                    incomingQueue.Enqueue(server);
                }
            }
            
            if (isListening && listenerSocket != null)
            {
                EndPoint newSenderEP = new IPEndPoint(IPAddress.Any, 0);
                listenerSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref newSenderEP, OnReceiveBroadcast, newSenderEP);
            }
        }
        catch (ObjectDisposedException) { return; }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Discovery] Packet error: {ex.Message}");
        }
    }
    
    public void StopListening()
    {
        isListening = false;
        if (listenerSocket != null)
        {
            try
            {
                listenerSocket.Close();
                listenerSocket.Dispose();
            }
            catch {}
            listenerSocket = null;
        }
    }
    
    private void BroadcastServerInfo()
    {
        if (broadcastSocket == null)
        {
            broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            broadcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }
        
        string currentScene = SceneManager.GetActiveScene().name;
        string status = (currentScene == "MainMenu") ? "In Lobby" : "In Game";
        int currentCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        string relayCode = (RelayManager.Instance != null) ? RelayManager.Instance.CurrentJoinCode : "";
        
        // Format: ServerName|Port|CurrentCount|MaxCount|Status|RelayCode
        string payload = $"{GameConfig.serverName}|7777|{currentCount}|{GameConfig.maxPlayers}|{status}|{relayCode}";
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        
        try
        {
            broadcastSocket.SendTo(bytes, new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Discovery] Broadcast error: {ex.Message}");
        }
    }
    
    private void PruneStaleServers()
    {
        List<string> toRemove = new List<string>();
        foreach (var pair in discoveredServers)
        {
            if (Time.time - pair.Value.lastSeenTimestamp > 4.0f)
            {
                toRemove.Add(pair.Key);
            }
        }
        
        foreach (var key in toRemove)
        {
            discoveredServers.Remove(key);
        }
    }
    
    private void OnDestroy()
    {
        StopListening();
        if (broadcastSocket != null)
        {
            broadcastSocket.Close();
            broadcastSocket.Dispose();
            broadcastSocket = null;
        }
    }
}