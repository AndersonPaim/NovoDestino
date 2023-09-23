using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    private Vector2 _recoilOffset;

    private int _newOffsetX;
    private int _newOffsetY;
    private bool _resettingRecoil = false;

    public void SetRecoilOffset(Vector2 recoilOffset)
    {
        _recoilOffset += recoilOffset;
        // _resettingRecoil = false;
    }

    public void ResetRecoilOffset()
    {
        // _resettingRecoil = true;
        _recoilOffset = new Vector2(0, 0);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // ====================== NAO FUNCIONA MAS TALVEZ SEJA ERRO EM COMO EU FIZ
        // if (_resettingRecoil)
        // {
        //     if (_recoilOffset.x < 0)
        //     {
        //         _newOffsetX = 1;
        //         Debug.Log(_recoilOffset.x);
        //     }
        //     else
        //     {
        //         _newOffsetX = 0;
        //     }

        //     if (_recoilOffset.y > 0)
        //     {
        //         _newOffsetY = -1;
        //     }
        //     else if (_recoilOffset.y < 0)
        //     {
        //         _newOffsetY = 1;
        //     }
        //     else
        //     {
        //         _newOffsetY = 0;
        //     }

        //     if (_recoilOffset.y == 0 && _recoilOffset.x == 0)
        //     {
        //         _resettingRecoil = false;
        //         _newOffsetX = 0;
        //         _newOffsetY = 0;
        //     }

        //     _recoilOffset = new Vector2(_newOffsetX, _newOffsetY);
        // }

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation + _recoilOffset.x, yRotation + _recoilOffset.y, transform.rotation.z);
        orientation.rotation = Quaternion.Euler(transform.rotation.x, yRotation, transform.rotation.z);
    }
}