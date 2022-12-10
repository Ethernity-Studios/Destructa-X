using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CreateAssetMenu]
public class Map : ScriptableObject
{
    public string MapName;
    public string SceneName;

    public Sprite LobbyMapBackground;
}
