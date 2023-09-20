using System.Threading.Tasks;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{

    public enum Dances
    {
        DANCE1, DANCE2
    }
    public enum JumpTypes
    {
        Double, Triple, lightGlide, heavyGlide
    }

    [SerializeField] private LayerMask _grappableAreas;
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private GameObject _pivot;
    [SerializeField] private Transform _throwKnifePos;
    [SerializeField] private Transform _grapplePosition;
    [SerializeField] private Knife _knife;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _grappleSpeed;
    [SerializeField] private float _airMultiplier;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _groundedDrag;
    [SerializeField] private float _lightGlideDrag;
    [SerializeField] private float _heavyGlideDrag;
    [SerializeField] private float _throwKnifeForce;
    [SerializeField] private float _sensitivity;
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleForce;
    [SerializeField] private JumpTypes _currentJumpType;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private LineRenderer _lineRenderer;

    private Animator _animator;
    private NewControls _input;

    [SerializeField] private float _playerAcceleration;
    private float _maxSpeed;
    private bool _isGrounded;
    private bool _isSwinging;
    private bool _hasExtrajump;
    private Vector3 _swingPoint;
    private Vector3 _currentGrapplePosition;
    private SpringJoint _joint;
    private int _jumpCount = 1;

    private Dances _currentDance;

    public float Speed => _playerAcceleration;
    //private Sequence _jumpSequence;

    public void ThrowKnife()
    {
        Knife knife = Instantiate(_knife);
        knife.transform.position = _throwKnifePos.transform.position;
        knife.transform.rotation = _throwKnifePos.transform.rotation;
        knife.Throw(_throwKnifeForce);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponent<Animator>();
        _maxSpeed = _walkSpeed;
        _input = new NewControls();
        _input.Enable();
        _input.Player.Look.performed += _ => Rotate();
        //_input.Player.Dance1.performed += _ => Dance(Dances.DANCE1);
        //_input.Player.Dance2.performed += _ => Dance(Dances.DANCE2);
        _input.Player.Knife.performed += _ => Knife();
    }

    private void Update()
    {
        //Movement();
        SpeedControl();
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void Knife()
    {
        _animator.SetTrigger("Knife");
    }

    private void Rotate()
    {
        Vector2 mousePos = _input.Player.Look.ReadValue<Vector2>();

        Vector3 upRotation = Vector3.up * mousePos.x;
        upRotation *= _sensitivity;
        upRotation.z = 0;

        gameObject.transform.Rotate(upRotation);

        float nextPos = _pivot.transform.rotation.eulerAngles.x - mousePos.y;

        if (nextPos >= 280 && nextPos < 360 || nextPos >= 0 && nextPos <= 80 || nextPos < 0 || nextPos > 360)
        {
            Vector3 rightRotation = Vector3.right * -mousePos.y;
            rightRotation *= _sensitivity;
            rightRotation.z = 0;
            _pivot.transform.Rotate(rightRotation);
        }
    }

    private void Movement()
    {
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

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = gameObject.transform.forward * verticalInput + gameObject.transform.right * horizontalInput;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump(_currentJumpType);
        }

        if (_isGrounded)
        {
            _rb.AddForce(direction.normalized * _playerAcceleration * 10, ForceMode.Force);
        }
        else
        {
            _rb.AddForce(direction.normalized * _playerAcceleration * 10 * _airMultiplier, ForceMode.Force);
        }
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

    private void MovePlayer()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        if(_isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * _playerAcceleration * 10f, ForceMode.Force);
        }
        else if(!_isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * _playerAcceleration * 10f * _airMultiplier, ForceMode.Force);
        }

        if (_isGrounded)
        {
            _rb.drag = _groundedDrag;
        }
        else
        {
            _rb.drag = 0;
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
    private void CancelDance()
    {
        _animator.SetBool(_currentDance.ToString(), false);
    }

    private void Dance(Dances dance)
    {
        CancelDance();

        _currentDance = dance;
        _animator.SetBool(dance.ToString(), true);
    }
}
