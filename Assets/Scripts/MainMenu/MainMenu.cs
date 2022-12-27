using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] NetworkManagerRoom networkManagerLobby;

    [SerializeField] TMP_InputField NetworkAddressInputField;

    private void Start()
    {
        NetworkAddressInputField.text = "localhost";
    }

    public void SetNetworkAddress(string address)
    {
        networkManagerLobby.networkAddress = address;
    }

    public void HostLobby()
    {
        if (string.IsNullOrEmpty(NetworkAddressInputField.text)) return;
        networkManagerLobby.StopClient();
        networkManagerLobby.StartHost();
    }

    public void JoinLobby()
    {
        if (string.IsNullOrEmpty(NetworkAddressInputField.text)) return;
        networkManagerLobby.StartClient();
    }
}
