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

    [SerializeField] private CinemachineVirtualCamera _eyeCamera;
    [SerializeField] private CinemachineVirtualCamera _scopeCamera;
    [SerializeField] private GameObject _pivot;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _lightGlideDrag;
    [SerializeField] private float _heavyGlideDrag;
    [SerializeField] private float _shootForce;
    [SerializeField] private JumpTypes _currentJumpType;
    [SerializeField] private ParticleSystem _shootParticle;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private AudioSource _bulletAudio;
    [SerializeField] private Transform _bulletPosition;
    [SerializeField] private float _fireRate;
    //[SerializeField] private Ease _jumpEase;

    private Animator _animator;
    private Rigidbody _rb;
    private NewControls _input;

    private float _currentSpeed;
    private bool _isGrounded;
    private bool _hasExtrajump;
    private bool _canShoot = true;
    private int _jumpCount = 1;

    private Dances _currentDance;
    //private Sequence _jumpSequence;

    public void ShootBullet()
    {
        Bullet bullet = Instantiate(_bulletPrefab);
        //_bulletAudio.Play();
        bullet.transform.localPosition = _bulletPosition.transform.position;
        bullet.transform.localRotation = _bulletPosition.transform.rotation;
        bullet.Shoot(_shootForce);
        _shootParticle.Play();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _currentSpeed = _walkSpeed;
        _input = new NewControls();
        _input.Enable();
        _input.Player.Look.performed += _ => Rotate();
        _input.Player.Shoot.started += _ => Shoot();
        _input.Player.Shoot.canceled += _ => StopShoot();
        _input.Player.Aim.performed += _ => Aim();
        _input.Player.Aim.canceled += _ => StopAim();
        _input.Player.Reload.performed += _ => Reload();
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

    private void Reload()
    {
        _animator.SetTrigger("Reload");
    }

    private void Rotate()
    {
        Vector2 mousePos = _input.Player.Look.ReadValue<Vector2>();

        Vector3 upRotation = Vector3.up * mousePos.x;
        upRotation.z = 0;

        gameObject.transform.Rotate(upRotation);

        float nextPos = _pivot.transform.rotation.eulerAngles.x - mousePos.y;

        if (nextPos >= 280 && nextPos < 360 || nextPos >= 0 && nextPos <= 80 || nextPos < 0 || nextPos > 360)
        {
            Vector3 rightRotation = Vector3.right * -mousePos.y;
            rightRotation.z = 0;
            _pivot.transform.Rotate(rightRotation);
        }
    }

    private void Aim()
    {
        _eyeCamera.Priority = 0;
        _scopeCamera.Priority = 1;
    }

    private void StopAim()
    {
        _eyeCamera.Priority = 1;
        _scopeCamera.Priority = 0;
    }

    private async void Shoot()
    {
        if(_canShoot)
        {
            float fireRate = (_fireRate / 60000) * 166;
            _animator.SetFloat("FireRate", fireRate);
            _animator.SetBool("Shoot", true);
        }
    }

    private async void StopShoot()
    {
        _animator.SetBool("Shoot", false);
        _canShoot = false;
        await Task.Delay(100);
        _canShoot = true;
    }

    private void Movement()
    {
        _animator.SetFloat("Speed", _rb.velocity.magnitude);
        _animator.SetFloat("Direction", _rb.velocity.z);
        Vector3 oldVelocity = _rb.velocity;
        Vector3 newVelocity = new Vector3();
        newVelocity.y = oldVelocity.y;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _currentSpeed = _runSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _currentSpeed = _walkSpeed;
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
