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
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _lightGlideDrag;
    [SerializeField] private float _heavyGlideDrag;
    [SerializeField] private float _shootForce;
    [SerializeField] private float _sensitivity;
    [SerializeField] private JumpTypes _currentJumpType;
    [SerializeField] private Rigidbody _rb;

    private Animator _animator;
    private NewControls _input;

    private float _currentSpeed;
    private bool _isGrounded;
    private bool _hasExtrajump;
    private int _jumpCount = 1;

    private Dances _currentDance;

    public float Speed => _currentSpeed;
    //private Sequence _jumpSequence;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponent<Animator>();
        _currentSpeed = _walkSpeed;
        _input = new NewControls();
        _input.Enable();
        _input.Player.Look.performed += _ => Rotate();
        _input.Player.Dance1.performed += _ => Dance(Dances.DANCE1);
        _input.Player.Dance2.performed += _ => Dance(Dances.DANCE2);

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
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        if (_isGrounded)
        {
            _rb.drag = 0;
        }
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
        _animator.SetFloat("Speed", _rb.velocity.magnitude);
        _animator.SetFloat("Direction", _rb.velocity.z);
        Vector3 oldVelocity = _rb.velocity;
        Vector3 newVelocity = new Vector3();
        newVelocity.y = oldVelocity.y;


        //TODO ATUALIZAR PARA O NOVO INPUT
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _currentSpeed = _runSpeed;
            _animator.SetBool("IsRunning", true);
            _weaponController.StopAim();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _currentSpeed = _walkSpeed;
            _animator.SetBool("IsRunning", false);
        }

        if (Input.GetKey(KeyCode.W))
        {
            Quaternion playerRotation = transform.rotation;
            Vector3 movementDirection = playerRotation * Vector3.forward;
            newVelocity += movementDirection * _currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Quaternion playerRotation = transform.rotation;
            Vector3 movementDirection = playerRotation * Vector3.forward;
            newVelocity += -movementDirection * _walkSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Quaternion playerRotation = transform.rotation;
            Vector3 movementDirection = playerRotation * Vector3.right;
            newVelocity += -movementDirection * _currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Quaternion playerRotation = transform.rotation;
            Vector3 movementDirection = playerRotation * Vector3.right;
            newVelocity += movementDirection * _currentSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump(_currentJumpType);
        }

        _rb.velocity = newVelocity;
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
