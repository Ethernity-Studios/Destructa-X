using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
        if (cameraPosition == null) return;
        transform.position = cameraPosition.position;
    }
}
