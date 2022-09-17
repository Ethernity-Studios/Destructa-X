using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerRoom : NetworkRoomManager
{
    RoomManager roomManager;

    public string SelectedMap;

    public int LoadedPlayers = 0;

    [SerializeField] GameObject loadingScreen;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public static event Action<NetworkConnection> OnServerReadied;

    public bool GameReady;

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();
    }

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }

        base.OnStartClient();
    }

    public override void OnRoomServerConnect(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);
        base.OnRoomServerConnect(conn);
    }

    public override void OnRoomServerPlayersReady()
    {
        roomManager = FindObjectOfType<RoomManager>();
        roomManager.RpcCountdown(1);
    }

    public void StartGame(string mapName)
    {
        ServerChangeScene(mapName);
    }

    /*public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (newSceneName == "Quark")
        {
            Debug.Log("Loading map scene");
            NetworkClient.isLoadingScene = true;
            StartCoroutine(LoadScene(newSceneName));
        }

    }*/

    /*IEnumerator LoadScene(string sceneName)
    {
        Debug.Log("LoadScene");
        loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName);
        //loadingSceneAsync.allowSceneActivation = false;
        //loadingScreen.SetActive(true);

        do
        {
            Debug.Log("Loading... " + loadingSceneAsync.progress);
            yield return null;
        } while (loadingSceneAsync.progress < 0.9f);

        LoadedPlayers++;
        Debug.Log("Adding loaded player: " + LoadedPlayers);

        while (LoadedPlayers != roomSlots.Count)
        {
            Debug.Log("Waiting for other players!");
            yield return null;
        }
        Debug.Log("Everyone is ready!");
        loadingSceneAsync.allowSceneActivation = loadingSceneAsync.isDone;
    }
    */
    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("OnServerReady");
        base.OnServerReady(conn);

       OnServerReadied?.Invoke(conn);
    }
}
