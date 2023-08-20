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

    private Animator _animator;
    private Rigidbody _rb;
    private NewControls _input;

    private float _currentSpeed;

    private Dances _currentDance;

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
    }

    private void Update()
    {
        //Emotes();
        Movement();
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
        Vector3 newVelocity = new Vector3();

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
