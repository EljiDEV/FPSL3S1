using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerMouvements : MonoBehaviour
{
    private CharacterController _characterController;
    
    [Header("Movements")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _gravity;
    private float _verticalVelocity;
    private bool _isSprinting;
    
    private float _horizontalInput;
    private float _verticalInput;

    private Vector3 _moveDirection;
    [SerializeField] private Transform _orientation;
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        GetInput();
        _verticalVelocity -= _gravity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && _characterController.isGrounded)
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _isSprinting = true;
        }
        else
        {
            _isSprinting = false;
        }
    }
    
    private void MovePlayer()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        Vector3 _targetVelocity = _moveDirection.normalized * _moveSpeed;
        if (_isSprinting)
        {
            _targetVelocity = _moveDirection.normalized * (_moveSpeed * _sprintSpeed);
        }
        else
        {
            _targetVelocity = _moveDirection.normalized * _moveSpeed;
        }
        
        _targetVelocity.y = _verticalVelocity;
        
        _characterController.Move(_targetVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        _verticalVelocity = Mathf.Sqrt(_jumpForce * 2f * _gravity);
    }
}
