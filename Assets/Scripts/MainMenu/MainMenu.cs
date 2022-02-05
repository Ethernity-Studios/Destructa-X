using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] NetworkManagerLobby networkManager;

    [SerializeField] GameObject MainMenuUI;
    public void HostGame()
    {
        networkManager.StartHost();
        MainMenuUI.SetActive(false);
    }
}
