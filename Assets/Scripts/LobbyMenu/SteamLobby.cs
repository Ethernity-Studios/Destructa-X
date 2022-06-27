using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] Button hostButton;

    NetworkManagerRoom networkManager;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";
    private void Start()
    {
        networkManager = GetComponent<NetworkManagerRoom>();

        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        hostButton.enabled = false;

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,networkManager.maxConnections);
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            hostButton.enabled = true;
            return;
        }

        networkManager.StartHost();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey,SteamUser.GetSteamID().ToString());
    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) => SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }
}
