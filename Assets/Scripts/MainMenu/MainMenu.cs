using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField]NetworkManagerLobby networkManagerLobby;
    [SerializeField] GameObject MainMenuUI;

    [SerializeField] TMP_InputField NetworkAdressInputField;

    public void SetNetworkAdress(string adress)
    {
        networkManagerLobby.networkAddress = adress;
    }

    public void HostLobby()
    {
        if (string.IsNullOrEmpty(NetworkAdressInputField.text)) return;
        networkManagerLobby.StartHost();
    }

    public void JoinLobby()
    {
        if (string.IsNullOrEmpty(NetworkAdressInputField.text)) return;
        networkManagerLobby.StartClient();
    }
}
