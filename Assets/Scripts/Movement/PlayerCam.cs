using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Mirror.Examples.Pong;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private PlayerConnection _connection;
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    private Vector2 _recoilOffset;

    public void SetRecoilOffset(Vector2 recoilOffset)
    {
        _recoilOffset += recoilOffset;
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_connection.isOwned)
        {
            return;
        }

        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f - _recoilOffset.x, 90f - _recoilOffset.x);


        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation + _recoilOffset.x, yRotation + _recoilOffset.y, transform.rotation.z);
        orientation.rotation = Quaternion.Euler(transform.rotation.x, yRotation + _recoilOffset.y, transform.rotation.z);
    }
}