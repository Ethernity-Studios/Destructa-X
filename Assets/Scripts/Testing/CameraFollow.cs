using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    
    
    [SerializeField] float mouseX;
    [SerializeField] float rotX;
    
    public float MouseSens = 1.4f;

    private Vector3 t;

    void Update()
    {
        if (player == null) return;
        //transform.SetParent(player.transform);

        
        //transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + new Vector3(0f, .6f, 0f),ref t, .05f);
        
        mouseX = Input.GetAxis("Mouse X") * MouseSens;
        rotX += Input.GetAxis("Mouse Y") * MouseSens;
        
        rotX = Mathf.Clamp(rotX, -90f, 90f);
        transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + mouseX, 0f);
    }

    private void LateUpdate()
    {
        if (player == null) return;
        //transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + new Vector3(0f, .6f, 0f),ref t, .001f);
        //transform.position = player.transform.position  + new Vector3(0f, .6f, 0f);;
    }
}