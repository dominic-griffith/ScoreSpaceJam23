using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public MovementState State;

    [Header("Movement")]
    [SerializeField] private Transform _orientation;
    [SerializeField] private float _walkSpeed = 70f;
    [SerializeField] private float _sprintSpeed = 100f;
    [SerializeField] private float _drag = 5f;
    [Header("Crouching")]
    [SerializeField] private float _crouchSpeed = 35f;
    [SerializeField] private float _crouchYScale = 0.5f;
    [Header("Ground Check")]
    [SerializeField] private float _playerHeight = 2f;
    [SerializeField] LayerMask _groundMask;
    [Header("Jump Check")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpCooldown = 0.25f;
    [SerializeField] private float _airMultiplier = 0.4f;
    [Header("Slope Check")]
    [SerializeField] private float _maxSlopeAngle = 45f;

    private float _horizontalInput;
    private float _verticalInput;
    private float _moveSpeed;
    private float _startYScale;
    private RaycastHit _slopeHit;
    private Vector3 _moveDirection;
    private Rigidbody _rb;
    private bool _isGrounded;
    private bool _canJump;
    private bool _sprinting;
    private bool _jumping;
    private bool _crouching;
    private bool _exitSlope;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _canJump = true;
        _startYScale = transform.localScale.y;
    }

    private void Update()
    {
        GroundCheck();
        GetInput();
        SpeedControl();
        StateHandler();
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
        _jumping = Input.GetButton("Jump");
        _sprinting = Input.GetButton("Sprint");
        _crouching = Input.GetButton("Crouch");

        if (_jumping && _canJump && _isGrounded)
        {
            _canJump = false;
            Jump();
            Invoke(nameof(ResetJump), _jumpCooldown);
        }
        if(Input.GetButtonDown("Crouch"))
        {
            transform.localScale = new Vector3(transform.localScale.x, _crouchYScale, transform.localScale.z);
            _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if(Input.GetButtonUp("Crouch"))
        {
            transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
        }
    }

    private void UpdatePosition()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;

        if(OnSlope() && !_exitSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection() * _moveSpeed, ForceMode.Force);
            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 20f, ForceMode.Force);
        } 
        else if(_isGrounded)
            _rb.AddForce(_moveDirection * _moveSpeed, ForceMode.Force);
        else
            _rb.AddForce(_moveDirection * _moveSpeed * _airMultiplier, ForceMode.Force);

        _rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if(OnSlope() && !_exitSlope)
        {
            if (_rb.velocity.magnitude > _moveSpeed)
                _rb.velocity = _rb.velocity.normalized * _moveSpeed;
        }
        else
        {
            Vector3 flatVelocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            if (flatVelocity.magnitude > _moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
                _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
            }
        }
        
    }

    private void Jump()
    {
        _exitSlope = true;
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        _canJump = true;
        _exitSlope = false;
    }

    private void StateHandler()
    {
        if(_crouching)
        {
            State = MovementState.crouching;
            _moveSpeed = _crouchSpeed;
        }
        else if(_isGrounded && _sprinting)
        {
            State = MovementState.sprinting;
            _moveSpeed = _sprintSpeed;
        }
        else if(_isGrounded)
        {
            State = MovementState.walking;
            _moveSpeed = _walkSpeed;
        }
        else
        {
            State = MovementState.air;
        }
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < _maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
    }
}
