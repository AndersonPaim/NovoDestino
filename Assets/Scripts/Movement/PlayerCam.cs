using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Mirror;
using Mirror.Examples.Pong;
using UnityEngine;

public class PlayerCam : NetworkBehaviour
{
    [SerializeField] private PlayerConnection _connection;
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation = 0;
    float yRotation = 0;
    private Vector2 _recoilOffset;

    public void SetRecoilOffset(Vector2 recoilOffset)
    {
        _recoilOffset += recoilOffset;
    }


    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void Update()
    {
        if (_connection.isLocalPlayer)
        {
            UpdateRotation();
        }
    }
    private void UpdateRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f - _recoilOffset.x, 90f - _recoilOffset.x);

        transform.rotation = Quaternion.Euler(xRotation + _recoilOffset.x, yRotation + _recoilOffset.y, transform.rotation.z);
        orientation.rotation = Quaternion.Euler(transform.rotation.x, yRotation + _recoilOffset.y, transform.rotation.z);
    }
}