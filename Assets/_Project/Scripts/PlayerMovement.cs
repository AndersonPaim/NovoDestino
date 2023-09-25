using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using DG.Tweening;
using System;
using Mirror;
using Mirror.Examples.Pong;

public class PlayerMovement : NetworkBehaviour
{
    public enum Dances
    {
        DANCE1, DANCE2
    }
    public enum JumpTypes
    {
        Double, Triple, lightGlide, heavyGlide
    }

    [SerializeField] private PlayerConnection _connection;
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private float _noiseAmplitude;
    [SerializeField] private float _noiseFrequency;
    //[SerializeField] private GameObject _hipFireCrosshair;

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
    [SerializeField] private Transform _grapplePosition;
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleForce;
    [SerializeField] private LayerMask _grappableAreas;
    [SerializeField] private float _defaultFov;
    [SerializeField] private float _grappleFov;
    [SerializeField] private float _damper;
    [SerializeField] private float _springForce;
    [SerializeField] private float _stopGrappleDistance;

    private bool _isGrounded;
    private bool _hasExtrajump;
    private bool _isSwinging;
    private bool _canDrawRope = false;
    private Vector3 _swingPoint;
    private Vector3 _currentGrapplePosition;
    private SpringJoint _joint;
    private float _maxSpeed;
    private int _jumpCount = 1;
    private Rigidbody _rb;
    private NewControls _input;

    public Transform orientation;
    public bool IsSwinging => _isSwinging;
    public bool CanDrawRope => _canDrawRope;
    public Transform GrapplePos => _grapplePosition;
    public Vector3 GrapplePoint => _swingPoint;
    Vector3 moveDirection;
    private CinemachineBasicMultiChannelPerlin _cameraToShake;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _cameraToShake = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _rb.freezeRotation = true;
        _maxSpeed = _walkSpeed;
        //Cursor.lockState = CursorLockMode.Locked;
        _input = new NewControls();
        _input.Enable();
        //_input.Player.Dance1.performed += _ => Dance(Dances.DANCE1);
        //_input.Player.Dance2.performed += _ => Dance(Dances.DANCE2);
        _input.Player.Knife.performed += _ => Knife();
    }

    private void Update()
    {
        if (_connection.isOwned)
        {
            gameObject.name = "HOST";
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, _groundLayer);
            Movement();
            SpeedControl();
        }
        else
        {
            gameObject.name = "CLIENT";
            _camera.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (_connection.isOwned)
        {
            MovePlayer();
        }
    }

    private void Knife()
    {
        _animator.SetTrigger("Knife");
    }

    private void Movement()
    {
        if (_isGrounded && !_isSwinging)
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

        if (_isSwinging && _canDrawRope)
        {
            Vector3 grappleDirection = _swingPoint - transform.position;
            _rb.AddForce(grappleDirection.normalized * _grappleForce * Time.deltaTime);
            float distanceFromPoint = Vector3.Distance(transform.position, _swingPoint);

            _joint.maxDistance = distanceFromPoint * 0.8f;
            _joint.minDistance = distanceFromPoint * 0.25f;

            if (distanceFromPoint <= _stopGrappleDistance)
            {
                StopGrapple();
            }

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

            DOTween.To(() => _cameraToShake.m_AmplitudeGain, x => _cameraToShake.m_AmplitudeGain = x, _noiseAmplitude, 2.0f)
                .SetEase(Ease.Linear);

            DOTween.To(() => _cameraToShake.m_FrequencyGain, x => _cameraToShake.m_FrequencyGain = x, _noiseFrequency, 2.0f)
                .SetEase(Ease.Linear);

            //_hipFireCrosshair.SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _maxSpeed = _walkSpeed;
            _animator.SetBool("IsRunning", false);

            DOTween.To(() => _cameraToShake.m_AmplitudeGain, x => _cameraToShake.m_AmplitudeGain = x, 0, 2.0f)
                .SetEase(Ease.Linear);

            DOTween.To(() => _cameraToShake.m_FrequencyGain, x => _cameraToShake.m_FrequencyGain = x, 0, 2.0f)
                .SetEase(Ease.Linear);


            //_hipFireCrosshair.SetActive(true);
        }
    }

    private void MovePlayer()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (_isGrounded)
        {
            Debug.Log("MOVE PLAYER: " + gameObject.name);
            _rb.AddForce(moveDirection.normalized * _playerAcceleration * 10f, ForceMode.Force);
        }
        else if (!_isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * _playerAcceleration * 10f * _airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        float vertVel = _grappleSpeed;

        if (_rb.velocity.y < vertVel)
        {
            vertVel = _rb.velocity.y;
        }

        if (flatVel.magnitude > _maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _maxSpeed;

            _rb.velocity = new Vector3(limitedVel.x, vertVel, limitedVel.z);
        }
    }

    private IEnumerator StartDrawRopeDelay()
    {
        yield return new WaitForSeconds(0.25f);
        _canDrawRope = true;
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        Transform initialPosition = Camera.main.transform;
        _animator.SetBool("IsRunning", false);

        if (_isSwinging)
        {
            return;
        }

        if (Physics.Raycast(initialPosition.position, initialPosition.forward, out hit, _maxGrappleDistance, _grappableAreas))
        {
            _swingPoint = hit.point;
        }
        else
        {
            _swingPoint = initialPosition.position + initialPosition.forward * _maxGrappleDistance;
        }

        DOTween.To(() => _camera.m_Lens.FieldOfView, x => _camera.m_Lens.FieldOfView = x, _grappleFov, 0.5f)
                .SetEase(Ease.Linear);

        StartCoroutine(StartDrawRopeDelay());
        _animator.SetBool("IsGrappling", true);
        _maxSpeed = _grappleSpeed;
        _isSwinging = true;

        _joint = gameObject.AddComponent<SpringJoint>();
        _joint.autoConfigureConnectedAnchor = false;
        _joint.connectedAnchor = _swingPoint;

        float distanceFromPoint = Vector3.Distance(transform.position, _swingPoint);

        _joint.maxDistance = distanceFromPoint * 0.8f;
        _joint.minDistance = distanceFromPoint * 0.25f;
        _joint.spring = _springForce;
        _joint.damper = _damper;
        _joint.massScale = 4.5f;

        _currentGrapplePosition = _grapplePosition.position;
    }

    private void StopGrapple()
    {
        if (_isSwinging)
        {
            DOTween.To(() => _camera.m_Lens.FieldOfView, x => _camera.m_Lens.FieldOfView = x, _defaultFov, 0.5f)
                .SetEase(Ease.Linear);

            _animator.SetBool("IsGrappling", false);
            Destroy(_joint);
            _isSwinging = false;
            _canDrawRope = false;
            _maxSpeed = _walkSpeed;
        }
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