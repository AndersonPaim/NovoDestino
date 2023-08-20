using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public enum Dances
    {
        DANCE1, DANCE2
    }

    [SerializeField] private GameObject _pivot;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _jumpForce;

    private Animator _animator;
    private Rigidbody _rb;
    private NewControls _input;

    private float _currentSpeed;
    private bool _isGrounded;

    private Dances _currentDance;
    //private Sequence _jumpSequence;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
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
    }


    private void Rotate()
    {
        Vector2 mousePos = _input.Player.Look.ReadValue<Vector2>();

        Vector3 upRotation = Vector3.up * mousePos.x;
        upRotation.z = 0;

        gameObject.transform.Rotate(upRotation);

        float nextPos = _pivot.transform.rotation.eulerAngles.x - mousePos.y;

        if (nextPos >= 280 && nextPos < 360 || nextPos >= 0 && nextPos <= 80 || nextPos < 0)
        {
            Vector3 rightRotation = Vector3.right * -mousePos.y;
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
            newVelocity += -movementDirection * _currentSpeed * Time.deltaTime;
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

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        }

        _rb.velocity = newVelocity;
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
