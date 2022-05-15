using UnityEngine;
using TMPro;

public class fps : MonoBehaviour
{
    public TMP_Text text;
    void Start()
    {
        
    }

    void Update()
    {
        int current;
            current = (int)(1f / Time.unscaledDeltaTime);
        text.text = "Fps: " + current;
    }
}
