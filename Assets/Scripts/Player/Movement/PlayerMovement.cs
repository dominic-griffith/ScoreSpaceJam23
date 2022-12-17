using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform _orientation;
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private float _maxSpeed = 50f;
    [SerializeField] private float _drag = 5f; 
    [Header("Ground Check")]
    [SerializeField] private float _playerHeight = 2f;
    [SerializeField] LayerMask _groundMask;
    [Header("Jump Check")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpCooldown = 0.25f;
    [SerializeField] private float _airMultiplier = 0.4f;

    private float _horizontalInput;
    private float _verticalInput;
    private float _jumpInput;
    private Vector3 _moveDirection;
    private Rigidbody _rb;
    private bool _isGrounded;
    private bool _canJump;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _canJump = true;
    }

    private void Update()
    {
        GroundCheck();
        GetInput();
        SpeedControl();
        HandleDrag();
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _groundMask);
    }

    private void HandleDrag()
    {
        if (_isGrounded)
            _rb.drag = _drag;
        else
            _rb.drag = 0;
    }

    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _jumpInput = Input.GetAxisRaw("Jump");

        if ((_jumpInput > 0f) && _canJump && _isGrounded)
        {
            _canJump = false;
            Jump();
            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    private void UpdatePosition()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        if(_isGrounded)
            _rb.AddForce(_moveDirection * _moveSpeed, ForceMode.Force);
        else
            _rb.AddForce(_moveDirection * _moveSpeed * _airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        if (flatVelocity.magnitude > _maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * _maxSpeed;
            _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        _canJump = true;
    }
}
