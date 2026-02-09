using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;
using Steamworks;

namespace SteamLobbyTutorial
{
    public class SteamLobby : NetworkBehaviour
    {
        public static SteamLobby Instance;
        public GameObject hostButton = null;
        public ulong lobbyID;
        public NetworkManager networkManager;
        public PanelSwap panelSwap;
        
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEnter;
        protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;
        
        private const string HostAdressKey = "HostAddress";

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            networkManager = GetComponent<NetworkManager>();
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam Manager not initialized");
                return;
            }
            panelSwap.gameObject.SetActive(true);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

        }

        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
        }

        void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Failed to create lobby: " + callback.m_eResult);
                return;
            }
            
            Debug.Log("Lobby Successfully Created. LobbyID: " + callback.m_ulSteamIDLobby);
            networkManager.StartHost();
            
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey, SteamUser.GetSteamID().ToString());
            lobbyID = callback.m_ulSteamIDLobby;
        }

        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("Join request received for Lobby: " + callback.m_steamIDLobby);
            if (NetworkClient.isConnected || NetworkClient.active)
            {
                Debug.Log("Network Client is active. Disconnecting...");
                NetworkManager.singleton.StopClient();
                NetworkClient.Shutdown();
            }
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active)
            {
                Debug.Log("Already in lobby as Host. Ignoring request...");
                return;
            }
            lobbyID = callback.m_ulSteamIDLobby;
            string _hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey);
            networkManager.networkAddress = _hostAdress;
            Debug.Log("Entered Lobby: " + callback.m_ulSteamIDLobby);
            networkManager.StartClient();
            panelSwap.SwapPanels("LobbyPanel");
            
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            
        }

        private IEnumerator DelayedNameUpdate(float delay)
        {
            if (LobbyUIManager.Instance != null)
            {
                Debug.LogWarning("Lobby UI Manager is null");
                yield break;
            }
            
            yield return new WaitForSeconds(delay);
            LobbyUIManager.Instance?.UpdatePlayerLobbyUI();
        }

        public void LeaveLobby()
        {
            CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
            CSteamID me = SteamUser.GetSteamID();
            var lobby = new CSteamID(lobbyID);
            List<CSteamID> members = new List<CSteamID>();
            
            int count = SteamMatchmaking.GetNumLobbyMembers(lobby);

            for (int i = 0; i < count; i++)
            {
                members.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
            }

            if (lobbyID != 0)
            {
                SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
                lobbyID = 0;
            }

            if (NetworkServer.active && currentOwner == me)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            
            panelSwap.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            panelSwap.SwapPanels("MainPanel");
        }
    }
    
}

