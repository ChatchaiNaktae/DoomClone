using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string gameplaySceneName = "DoomClone";
    
    [Header("Main Menu UI")]
    public GameObject menuContainer;
    public Button hostButton;
    public Button joinButton;
    public Button singleplayerButton;
    public Button quitButton;
    
    [Header("Host Alert Popup")]
    public GameObject hostAlertPopup;
    public Button alertYesButton;
    public Button alertNoButton;
    
    [Header("Host Config Panel")]
    public GameObject hostConfigPanel;
    public Slider maxPlayersSlider;
    public TextMeshProUGUI maxPlayersText;
    public Toggle friendlyFireToggle;
    public Slider monsterDamageSlider;
    public TextMeshProUGUI monsterDamageText;
    public Button launchHostButton;
    public Button configBackButton;
    
    [Header("Lobby Room UI")]
    public GameObject lobbyPanel;
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;
    public Button startGameButton;
    public Button leaveLobbyButton;
    
    [Header("Server List UI")]
    public GameObject serverListPanel;
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public Button connectButton;
    public Button serverListBackButton;
    public Transform serverListContent;
    public GameObject serverEntryPrefab;
    
    public static bool isSingleplayerMode = false;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Main Menu Bindings
        if (hostButton != null) hostButton.onClick.AddListener(OnHostButtonClicked);
        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClicked);
        if (singleplayerButton != null) singleplayerButton.onClick.AddListener(StartSingleplayerGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        
        // Host Alert Popup Bindings
        if (alertYesButton != null) alertYesButton.onClick.AddListener(OnAlertYesClicked);
        if (alertNoButton != null) alertNoButton.onClick.AddListener(OnAlertNoClicked);
        
        // Host Config Bindings
        if (maxPlayersSlider != null) maxPlayersSlider.onValueChanged.AddListener(OnMaxPlayersChanged);
        if (monsterDamageSlider != null) monsterDamageSlider.onValueChanged.AddListener(OnMonsterDamageChanged);
        if (launchHostButton != null) launchHostButton.onClick.AddListener(LaunchHostLobby);
        if (configBackButton != null) configBackButton.onClick.AddListener(OnConfigBackClicked);
        
        // Lobby Room Bindings
        if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameClicked);
        if (leaveLobbyButton != null) leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
        
        if (connectButton != null) connectButton.onClick.AddListener(OnConnectByIPClicked);
        if (serverListBackButton != null) serverListBackButton.onClick.AddListener(OnServerListBackClicked);
        
        if (serverListPanel != null) serverListPanel.SetActive(false);
        
        InitializeConfigUI();
        
        if (hostAlertPopup != null) hostAlertPopup.SetActive(false);
        if (hostConfigPanel != null) hostConfigPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerListUpdated;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerListUpdated;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
            
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallbackHandler;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadedComplete;
            }
        }
    }
    
    private void InitializeConfigUI()
    {
        if (maxPlayersSlider != null)
        {
            maxPlayersSlider.value = GameConfig.maxPlayers;
            OnMaxPlayersChanged(maxPlayersSlider.value);
        }
        if (friendlyFireToggle != null) friendlyFireToggle.isOn = GameConfig.friendlyFire;
        if (monsterDamageSlider != null)
        {
            monsterDamageSlider.value = GameConfig.monsterDamageMultiplier;
            OnMonsterDamageChanged(monsterDamageSlider.value);
        }
    }
    
    private void OnHostButtonClicked() => hostAlertPopup?.SetActive(true);
    private void OnAlertNoClicked() => hostAlertPopup?.SetActive(false);
    private void OnConfigBackClicked() => hostConfigPanel?.SetActive(false);
    
    private void OnAlertYesClicked()
    {
        hostAlertPopup?.SetActive(false);
        hostConfigPanel?.SetActive(true);
    }
    
    private void OnMaxPlayersChanged(float value)
    {
        int count = Mathf.RoundToInt(value);
        GameConfig.maxPlayers = count;
        if (maxPlayersText != null) maxPlayersText.text = $"Max Players: {count}";
    }
    
    private void OnMonsterDamageChanged(float value)
    {
        GameConfig.monsterDamageMultiplier = value;
        if (monsterDamageText != null) monsterDamageText.text = $"Monster Damage: {value:F1}x";
    }
    
    private void LaunchHostLobby()
    {
        if (friendlyFireToggle != null) GameConfig.friendlyFire = friendlyFireToggle.isOn;
        isSingleplayerMode = false;
        
        StartHostSession();
        OpenLobbyUI(isHost: true);
    }
    
    public void StartSingleplayerGame()
    {
        GameConfig.maxPlayers = 1;
        isSingleplayerMode = true;
        
        StartHostSession();
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
    
    private void StartHostSession()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallbackHandler;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallbackHandler;
            
            // Subscribe SceneManager event safely after Host starts
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadedComplete;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadedComplete;
            }
        }
    }
    
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
            return;
        }
        
        if (isSingleplayerMode)
        {
            response.Approved = false;
            response.Reason = "Host is in Singleplayer mode.";
            return;
        }
        
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= GameConfig.maxPlayers)
        {
            response.Approved = false;
            response.Reason = "Server is full.";
            return;
        }
        
        response.Approved = true;
        response.CreatePlayerObject = false;
    }
    
    private void OnJoinButtonClicked()
    {
        if (serverListPanel != null) serverListPanel.SetActive(true);
        if (NetworkDiscoveryManager.Instance != null)
        {
            NetworkDiscoveryManager.Instance.StartListening();
        }
        StartCoroutine(UpdateServerListRoutine());
    }
    
    private void OnServerListBackClicked()
    {
        StopAllCoroutines();
        if (NetworkDiscoveryManager.Instance != null)
        {
            NetworkDiscoveryManager.Instance.StopListening();
        }
        if (serverListPanel != null) serverListPanel.SetActive(false);
    }
    
    private IEnumerator UpdateServerListRoutine()
    {
        while (serverListPanel != null && serverListPanel.activeSelf)
        {
            RefreshServerListDisplay();
            yield return new WaitForSeconds(1.0f);
        }
    }
    
    private void RefreshServerListDisplay()
    {
        if (serverListContent == null || serverEntryPrefab == null || NetworkDiscoveryManager.Instance == null)
            return;
        
        foreach (Transform child in serverListContent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var server in NetworkDiscoveryManager.Instance.discoveredServers.Values)
        {
            GameObject entry = Instantiate(serverEntryPrefab, serverListContent);
            ServerEntryUI entryUI = entry.GetComponent<ServerEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(server, ConnectToServer);
            }
        }
    }
    
    public void ConnectToServer(string ip, ushort port)
    {
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(ip, port);
        }
        
        if (NetworkDiscoveryManager.Instance != null)
        {
            NetworkDiscoveryManager.Instance.StopListening();
        }
        
        if (serverListPanel != null)
            serverListPanel.SetActive(false);
        StartJoinGame();
    }
    
    private void OnConnectByIPClicked()
    {
        string targetIP = string.IsNullOrEmpty(ipInputField.text) ? "127.0.0.1" : ipInputField.text;
        ushort targetPort = 7777;
        
        if (portInputField != null && !string.IsNullOrEmpty(portInputField.text))
        {
            ushort.TryParse(portInputField.text, out targetPort);
        }
        
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(targetIP, targetPort);
        }
        
        if (serverListPanel != null) serverListPanel.SetActive(false);
        StartJoinGame();
    }
    
    public void StartJoinGame()
    {
        if (NetworkDiscoveryManager.Instance != null)
        {
            NetworkDiscoveryManager.Instance.StopListening();
        }
        
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            
            NetworkManager.Singleton.StartClient();
            StartCoroutine(CheckConnectionRoutine(4f));
        }
    }
    
    private IEnumerator CheckConnectionRoutine(float timeout)
    {
        float timer = 0f;
        while (timer < timeout)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                if (SceneManager.GetActiveScene().name == gameplaySceneName)
                {
                    menuContainer?.SetActive(false);
                    hostConfigPanel?.SetActive(false);
                    lobbyPanel?.SetActive(false);
                }
                else
                {
                    OpenLobbyUI(isHost: false);
                }
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (!NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    private void OpenLobbyUI(bool isHost)
    {
        if (SceneManager.GetActiveScene().name != "MainMenu") return;
        
        if (menuContainer != null)
            menuContainer.SetActive(false);
        if (hostConfigPanel != null)
            hostConfigPanel.SetActive(false);
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
        
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(isHost);
        }
        
        RefreshPlayerList();
    }
    
    private void OnStartGameClicked()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }
    
    // Spawn player objects for all clients when scene load finishes
    private void OnSceneLoadedComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        if (sceneName == gameplaySceneName)
        {
            foreach (ulong clientId in clientsCompleted)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                {
                    if (client.PlayerObject == null)
                    {
                        GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
                        if (playerPrefab != null)
                        {
                            Vector3 spawnPos = new Vector3(0f, 2f, 0f);
                            GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                        }
                        else
                        {
                            Debug.LogError("[MainMenuController] Player Prefab is missing in NetworkManager configuration!");
                        }
                    }
                }
            }
        }
    }
    
    private void OnClientConnectedCallbackHandler(ulong clientId)
    {
        // Late-join: If game is already active in DoomClone, spawn the late player immediately
        if (NetworkManager.Singleton.IsServer && SceneManager.GetActiveScene().name == gameplaySceneName)
        {
            GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
            if (playerPrefab != null)
            {
                Vector3 spawnPos = new Vector3(0f, 2f, 0f);
                GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }
    
    private void OnLeaveLobbyClicked()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
        if (menuContainer != null)
            menuContainer.SetActive(true);
        
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenu");
        }
    }
    
    private void OnPlayerListUpdated(ulong clientId) => RefreshPlayerList();
    
    private void OnPlayerDisconnected(ulong clientId)
    {
        // Handle disconnection only when currently in the MainMenu scene
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                OnLeaveLobbyClicked();
            }
            else
            {
                RefreshPlayerList();
            }
        }
        // If inside gameplay scenes like DoomClone, let NetworkDisconnectHandler manage Host Migration
    }
    
    private void RefreshPlayerList()
    {
        if (playerListContainer == null || playerEntryPrefab == null || NetworkManager.Singleton == null) return;
        
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
        
        int playerSlotIndex = 1;
        
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            TextMeshProUGUI nameText = entry.transform.Find("Text_PlayerName")?.GetComponent<TextMeshProUGUI>();
            
            if (nameText != null)
            {
                bool isHostPlayer = (id == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsServer) || id == 0;
                nameText.text = isHostPlayer ? $"Player {playerSlotIndex} [HOST]" : $"Player {playerSlotIndex}";
            }
            
            playerSlotIndex++;
        }
    }
    
    public void QuitGame() => Application.Quit();
}