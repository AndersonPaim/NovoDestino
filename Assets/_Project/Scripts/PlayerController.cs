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

    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private GameObject _pivot;
    [SerializeField] private Transform _throwKnifePos;
    [SerializeField] private Knife _knife;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _airMultiplier;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _lightGlideDrag;
    [SerializeField] private float _heavyGlideDrag;
    [SerializeField] private float _throwKnifeForce;
    [SerializeField] private float _sensitivity;
    [SerializeField] private JumpTypes _currentJumpType;
    [SerializeField] private Rigidbody _rb;

    private Animator _animator;
    private NewControls _input;

    [SerializeField] private float _playerAcceleration;
    private float _maxSpeed;
    private bool _isGrounded;
    private bool _hasExtrajump;
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
        _input.Player.Dance1.performed += _ => Dance(Dances.DANCE1);
        _input.Player.Dance2.performed += _ => Dance(Dances.DANCE2);
        _input.Player.Knife.performed += _ => Knife();

        /*QUANDO FOR FAZER UM PULO DECENTE

        _jumpSequence = DOTween.Sequence();
        _jumpSequence.SetAutoKill(false);
        _jumpSequence.InsertCallback(0, () =>
        {
            _isGrounded = false;
            _anim.SetTrigger(JumpHash);
        });
        _jumpSequence.Insert(0, _transform.DOLocalMoveY(5f, _jumpDuration).SetEase(_startJump));
        _jumpSequence.Insert(_jumpDuration, _transform.DOLocalMoveY(0, _landingDuration).SetEase(_landingJump));
        _jumpSequence.AppendCallback(() =>
        {
            _isGrounded = true;
        });*/
    }

    private void Update()
    {
        Movement();
        SpeedControl();
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        if (_isGrounded)
        {
            _rb.drag = 0;
        }
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
            _rb.AddForce(direction.normalized * _playerAcceleration, ForceMode.Force);
        }
        else
        {
            _rb.AddForce(direction.normalized * _playerAcceleration * _airMultiplier, ForceMode.Force);
        }

        if (horizontalInput == 0 && verticalInput == 0 && _isGrounded)
        {
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
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

    // private void Movement()
    // {
    //     _animator.SetFloat("Speed", _rb.velocity.magnitude);
    //     _animator.SetFloat("Direction", _rb.velocity.z);
    //     Vector3 oldVelocity = _rb.velocity;
    //     Vector3 newVelocity = new Vector3();
    //     newVelocity.y = oldVelocity.y;


    //     //TODO ATUALIZAR PARA O NOVO INPUT
    // if (Input.GetKeyDown(KeyCode.LeftShift))
    // {
    //     _currentSpeed = _runSpeed;
    //     _animator.SetBool("IsRunning", true);
    //     _weaponController.StopAim();
    // }
    // if (Input.GetKeyUp(KeyCode.LeftShift))
    // {
    //     _currentSpeed = _walkSpeed;
    //     _animator.SetBool("IsRunning", false);
    // }

    //     if (Input.GetKey(KeyCode.W))
    //     {
    //         Quaternion playerRotation = transform.rotation;
    //         Vector3 movementDirection = playerRotation * Vector3.forward;
    //         newVelocity += movementDirection * _currentSpeed * Time.deltaTime;
    //     }
    //     if (Input.GetKey(KeyCode.S))
    //     {
    //         Quaternion playerRotation = transform.rotation;
    //         Vector3 movementDirection = playerRotation * Vector3.forward;
    //         newVelocity += -movementDirection * _walkSpeed * Time.deltaTime;
    //     }
    //     if (Input.GetKey(KeyCode.A))
    //     {
    //         Quaternion playerRotation = transform.rotation;
    //         Vector3 movementDirection = playerRotation * Vector3.right;
    //         newVelocity += -movementDirection * _currentSpeed * Time.deltaTime;
    //     }
    //     if (Input.GetKey(KeyCode.D))
    //     {
    //         Quaternion playerRotation = transform.rotation;
    //         Vector3 movementDirection = playerRotation * Vector3.right;
    //         newVelocity += movementDirection * _currentSpeed * Time.deltaTime;
    //     }

    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         Jump(_currentJumpType);
    //     }

    //     _rb.velocity = newVelocity;
    // }

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
            _rb.drag = 0;
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
