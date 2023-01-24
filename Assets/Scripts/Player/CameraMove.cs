using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform cameraPosition;
    public bool CanMove = true;

    void Update()
    {
        if (cameraPosition == null) return;
        if (!CanMove) return;
        transform.position = cameraPosition.position;
    }
}
