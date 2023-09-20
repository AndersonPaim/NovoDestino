using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public enum Dances
    {
        DANCE1, DANCE2
    }
    public enum JumpTypes
    {
        Double, Triple, lightGlide, heavyGlide
    }

    [SerializeField] private WeaponController _weaponController;

    [Header("Movement")]
    [SerializeField] private JumpTypes _currentJumpType;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _grappleSpeed;
    [SerializeField] private float _playerAcceleration;
    [SerializeField] private float _groundedDrag;

    [Header("Jump")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _lightGlideDrag;
    [SerializeField] private float _heavyGlideDrag;
    [SerializeField] private float _airMultiplier;
    [SerializeField] private Animator _animator;

    [Header("Grapple")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _grapplePosition;
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleForce;
    [SerializeField] private LayerMask _grappableAreas;

    private bool _isGrounded;
    private bool _hasExtrajump;
    private bool _isSwinging;
    private Vector3 _swingPoint;
    private Vector3 _currentGrapplePosition;
    private SpringJoint _joint;
    private float _maxSpeed;
    private int _jumpCount = 1;
    private Rigidbody _rb;
    private NewControls _input;

    public Transform orientation;
    Vector3 moveDirection;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _maxSpeed = _walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        _input = new NewControls();
        _input.Enable();
        //_input.Player.Dance1.performed += _ => Dance(Dances.DANCE1);
        //_input.Player.Dance2.performed += _ => Dance(Dances.DANCE2);
        _input.Player.Knife.performed += _ => Knife();
    }

    private void Update()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        Movement();
        SpeedControl();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Knife()
    {
        _animator.SetTrigger("Knife");
    }

    private void Movement()
    {
        if (_isGrounded)
        {
            _rb.drag = _groundedDrag;
        }
        else
        {
            _rb.drag = 0;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartGrapple();
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            StopGrapple();
        }

        if(_isSwinging)
        {
            Vector3 grappleDirection = _swingPoint - transform.position;
            _rb.AddForce(grappleDirection.normalized * _grappleForce * Time.deltaTime);
            float distanceFromPoint = Vector3.Distance(transform.position, _swingPoint);

            _joint.maxDistance = distanceFromPoint * 0.8f;
            _joint.minDistance = distanceFromPoint * 0.25f;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump(_currentJumpType);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _maxSpeed = _runSpeed;
            _animator.SetBool("IsRunning", true);
            _weaponController.StopAim();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _maxSpeed = _walkSpeed;
            _animator.SetBool("IsRunning", false);
        }
    }

    private void MovePlayer()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(_isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * _playerAcceleration * 10f, ForceMode.Force);
        }
        else if(!_isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * _playerAcceleration * 10f * _airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (flatVel.magnitude > _maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _maxSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
    }

    private void DrawRope()
    {
        if(!_joint)
        {
            return;
        }

        _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, _swingPoint, Time.deltaTime * 8);

        _lineRenderer.SetPosition(0, _grapplePosition.position);
        _lineRenderer.SetPosition(1, _swingPoint);
    }

    private void StartGrapple()
    {
        _isSwinging = true;
        _maxSpeed = _grappleSpeed;
        RaycastHit hit;
        Transform cam = Camera.main.gameObject.transform;

        if(Physics.Raycast(cam.position, cam.forward, out hit, _maxGrappleDistance, _grappableAreas))
        {
            _swingPoint = hit.point;
            _joint = gameObject.AddComponent<SpringJoint>();
            _joint.autoConfigureConnectedAnchor = false;
            _joint.connectedAnchor = _swingPoint;

            float distanceFromPoint = Vector3.Distance(transform.position, _swingPoint);

            _joint.maxDistance = distanceFromPoint * 0.8f;
            _joint.minDistance = distanceFromPoint * 0.25f;

            _joint.spring = 4.5f;
            _joint.damper = 7;
            _joint.massScale = 4.5f;

            _lineRenderer.positionCount = 2;
        }
    }

    private void StopGrapple()
    {
        _isSwinging = false;
        _maxSpeed = _walkSpeed;
        _lineRenderer.positionCount = 0;
        Destroy(_joint);
    }

    private void Jump(JumpTypes jumpType)
    {
        if (_isGrounded)
        {
            _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
            _hasExtrajump = true;
            _jumpCount = 0;
        }
        else if (_hasExtrajump)
        {
            switch (jumpType)
            {
                case JumpTypes.Double:
                    _rb.AddForce(transform.up * _jumpForce * 1.5f, ForceMode.Impulse);
                    _hasExtrajump = false;
                    break;
                case JumpTypes.Triple:
                    _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
                    _jumpCount++;
                    if (_jumpCount == 2)
                    {
                        _hasExtrajump = false;
                    }
                    break;
                case JumpTypes.lightGlide:
                    _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
                    _rb.drag = _lightGlideDrag;
                    _hasExtrajump = false;
                    break;
                case JumpTypes.heavyGlide:
                    _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
                    _rb.drag = _heavyGlideDrag;
                    _hasExtrajump = false;
                    break;
                default:
                    break;
            }
        }
        else
        {
            //_rb.drag = 0;
            _hasExtrajump = false;
        }
    }
}