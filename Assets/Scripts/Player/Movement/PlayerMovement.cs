using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public MovementState State;

    [Header("Movement")]
    [SerializeField] private Transform _orientation;
    [SerializeField] private float _walkSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 10f;
    [SerializeField] private float _drag = 5f;
    [Header("Crouching")]
    [SerializeField] private float _crouchSpeed = 3.5f;
    [SerializeField] private float _crouchYScale = 0.5f;
    [Header("Ground Check")]
    [SerializeField] private float _playerHeight = 2f;
    [SerializeField] LayerMask _groundMask;
    [SerializeField] LayerMask _nonGroundMask;
    [Header("Jump Check")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpCooldown = 0.25f;
    [SerializeField] private float _airMultiplier = 0.4f;
    [Header("Slope Check")]
    [SerializeField] private float _maxSlopeAngle = 45f;
    [Header("Swinging")]
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform _gunTip, _cam, _player, _predictionPoint, _camHook;
    [SerializeField] private LayerMask _swingMask;
    [SerializeField] private float _maxSwingDistance = 25f;
    [SerializeField] private float _swingSpeed = 15f;
    [SerializeField] private float _horizontalThrustForce = 2000f;
    [SerializeField] private float _forwardThrustForce = 3000f;
    [SerializeField] private float _extendCableSpeed = 20f;
    [SerializeField] private float _predictionSphereCastRadius = 1f;

    private float _horizontalInput;
    private float _verticalInput;
    private float _moveSpeed;
    private float _startYScale;
    
    private RaycastHit _predictionHit;
    private RaycastHit _slopeHit;
    private Vector3 _moveDirection;
    private Vector3 _swingPoint;
    private Vector3 _currentSwingPosition;
    private SpringJoint _joint;
    private Rigidbody _rb;
    private bool _isGrounded;
    private bool _isNotGround;
    private bool _canJump;
    private bool _sprinting;
    private bool _jumping;
    private bool _crouching;
    private bool _swinging;
    private bool _exitSlope;

    public enum MovementState
    {
        walking,
        sprinting,
        swinging,
        crouching,
        air
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _canJump = true;
        _startYScale = transform.localScale.y;
        _swingMask = _groundMask;
    }

    private void Update()
    {
        GroundCheck();
        GetInput();
        CheckForSwingPoints();
        if (_joint != null) SwingThrustMovement();
        SpeedControl();
        StateHandler();
        HandleDrag();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _groundMask);
        _isNotGround = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _nonGroundMask);
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
        if (Input.GetButtonDown("Crouch")) StartCrouch();
        if (Input.GetButtonUp("Crouch")) StopCrouch();
        if (Input.GetButtonDown("Swing")) StartSwing();
        if (Input.GetButtonUp("Swing")) StopSwing();
    }

    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, _crouchYScale, transform.localScale.z);
        _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void StopCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
    }

    private void MovePlayer()
    {
        if (_swinging) return;

        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;

        if(OnSlope() && !_exitSlope && _isGrounded)
        {
            _rb.AddForce(GetSlopeMoveDirection() * _moveSpeed * 20f, ForceMode.Force);
            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        } 
        else if(_isGrounded)
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
        else if (_isNotGround) { }
        else
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f * _airMultiplier, ForceMode.Force);

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
        if(_swinging)
        {
            State = MovementState.swinging;
            _moveSpeed = _swingSpeed;
        }
        else if(_crouching)
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

    private void StartSwing()
    {
        if (_predictionHit.point == Vector3.zero) return;

        _swinging = true;
        _camHook.gameObject.SetActive(false);

        _swingPoint = _predictionHit.point;
        _joint = _player.gameObject.AddComponent<SpringJoint>();
        _joint.autoConfigureConnectedAnchor = false;
        _joint.connectedAnchor = _swingPoint;

        float distanceFromPoint = Vector3.Distance(_player.position, _swingPoint);

        _joint.maxDistance = distanceFromPoint * 0.8f;
        _joint.minDistance = distanceFromPoint * 0.25f;

        _joint.spring = 4.5f;
        _joint.damper = 7f;
        _joint.massScale = 4.5f;

        _lr.positionCount = 2;
        _currentSwingPosition = _gunTip.position;
    }

    private void StopSwing()
    {
        _swinging = false;

        _lr.positionCount = 0;
        Destroy(_joint);
    }

    private void DrawRope()
    {
        if (!_joint) return;

        _currentSwingPosition = Vector3.Lerp(_currentSwingPosition, _swingPoint, Time.deltaTime * 8f);

        _lr.SetPosition(0, _gunTip.position);
        _lr.SetPosition(1, _swingPoint);
    }

    private void SwingThrustMovement()
    {
        if(_horizontalInput != 0f)
        {
            if (_horizontalInput > 0f)
                _rb.AddForce(_orientation.right * _horizontalThrustForce * Time.deltaTime);
            else
                _rb.AddForce(-_orientation.right * _horizontalThrustForce * Time.deltaTime);
        }
        if(_verticalInput > 0f)
            _rb.AddForce(_orientation.forward * _horizontalThrustForce * Time.deltaTime);
    }

    private void CheckForSwingPoints()
    {
        if (_joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(_cam.position, _predictionSphereCastRadius, _cam.forward, out sphereCastHit, _maxSwingDistance, _swingMask);

        RaycastHit raycastHit;
        Physics.Raycast(_cam.position, _cam.forward, out raycastHit, _maxSwingDistance, _swingMask);

        Vector3 realHitPoint;

        if (raycastHit.point != Vector3.zero) // Option 1 - Direct Hit
            realHitPoint = raycastHit.point;
        else if (sphereCastHit.point != Vector3.zero) // Option 2 - Indirect (predicted) Hit
            realHitPoint = sphereCastHit.point;
        else // Option 3 - Miss
            realHitPoint = Vector3.zero;


        if (realHitPoint != Vector3.zero)
        {
            _predictionPoint.gameObject.SetActive(true);
            _predictionPoint.position = realHitPoint;
        }
        else
        {
            _camHook.gameObject.SetActive(true);
            _predictionPoint.gameObject.SetActive(false);
        }

        _predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
}
