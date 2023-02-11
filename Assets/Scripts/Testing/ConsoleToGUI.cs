using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ConsoleToGUI : MonoBehaviour
{
    private PlayerInput playerInput;
    
    string myLog = "*begin log";
    string filename = "";
    bool doShow = false;
    int kChars = 700;

    private void Awake()
    {
        playerInput = new();
        playerInput.Enable();

        playerInput.Console.Open.performed += toggleConsole;
    }

    void OnEnable()
    {
        playerInput.Console.Enable();
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        playerInput.Console.Disable();
        Application.logMessageReceived -= Log;
    }

    void toggleConsole(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        doShow = !doShow;
    }

    private void Start() => DontDestroyOnLoad(gameObject);

    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        myLog = myLog + "\n" + logString;
        if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }

        // for the file ...
        if (filename == "")
        {
            string d = System.Environment.GetFolderPath(
               System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            System.IO.Directory.CreateDirectory(d);
            string r = Random.Range(1000, 9999).ToString();
            filename = d + "/log-" + r + ".txt";
        }
        try { System.IO.File.AppendAllText(filename, logString + "\n"); }
        catch { }
    }

    void OnGUI()
    {
        if (!doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
    }
}