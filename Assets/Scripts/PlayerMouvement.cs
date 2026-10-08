using System;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpCooldown;
    private Vector3 _moveDirection;

    [Header("Check Ground")]
    [SerializeField] private float _playerHeight;
    [SerializeField] private LayerMask _ground;
    [SerializeField] private Transform _orientation;
    private bool _canJump;
    private bool _grounded;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    [SerializeField] private Rigidbody _rb;
    void Start()
    {
        _rb.GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _canJump = true;
    }
    void Update()
    {
        _grounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _ground);
        GetInput();
        
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && _canJump && _grounded)
        {
            Debug.Log("saute");
            _canJump = false;
            Jump();
            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        Vector3 targetVelocity = _moveDirection.normalized * _moveSpeed;
        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
    }
    
    private void Jump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        _canJump = true;
    }
}
