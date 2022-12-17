using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform _orientation;
    [SerializeField] private float _mouseSensitivity = 500f;
    [SerializeField] private float _clampAngle = 90f;

    private float _rotY = 0f;
    private float _rotX = 0f;
    private float _mouseX;
    private float _mouseY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        GetInput();
        UpdateCamera();
    }

    private void GetInput()
    {
        _mouseX = Input.GetAxisRaw("Mouse X");
        _mouseY = Input.GetAxisRaw("Mouse Y");
    }

    private void UpdateCamera()
    {
        _rotY += _mouseX * _mouseSensitivity * Time.deltaTime;
        _rotX -= _mouseY * _mouseSensitivity * Time.deltaTime;

        _rotX = Mathf.Clamp(_rotX, -_clampAngle, _clampAngle);

        transform.rotation = Quaternion.Euler(_rotX, _rotY, 0f);
        _orientation.rotation = Quaternion.Euler(0, _rotY, 0f);
    }
}
