using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] NetworkManagerRoom networkManagerLobby;
    [SerializeField] GameObject MainMenuUI;

    [SerializeField] TMP_InputField NetworkAdressInputField;

    public static string CustomNetworkAdress;

    private void Start()
    {
        NetworkAdressInputField.text = "localhost";
    }

    public void SetNetworkAdress(string adress)
    {
        CustomNetworkAdress = adress;
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

    public void Vassa(string name)
    {

    }
}
