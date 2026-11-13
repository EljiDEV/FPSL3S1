using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class PlayerMouvements : MonoBehaviour
{
    [Header("Data")] 
    [SerializeField] private float _damageTakenQuantity;
    
    [Header("Movements")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _gravity;
    [SerializeField] private float _coyotteTime;
    
    [Header("Air & Ground Control")]
    [SerializeField] private float _groundAcceleration;
    [SerializeField] private float _airAcceleration;

    [Header("Life Data")]
    [SerializeField] private float _lifeHealAmount;
    [SerializeField] private float _lifeHealTime;
    public GameObject DamageUI;
    
    [Header("Other Settings")]
    [SerializeField] private Transform _orientation;
    
    private float _verticalVelocity;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private bool _isSprinting;
    private bool _canJump;
    private bool _inAir;

    private bool _hasHeal;
    private bool _isDead;
    private bool _hasRun;
    
    private CharacterController _characterController;

    private Vector3 _moveDirection;
    private Vector3 _currentHorizontalVelocity;
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _canJump = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    void Update()
    {
        GetInput();
        Gravity();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SearchToHeal();
    }
    
    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && _canJump && _inAir == false)
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.LeftShift) && _inAir == false)
        {
            _isSprinting = true;
        }
        else
        {
            if (_inAir == false)
            {
                _isSprinting = false;
            }
        }
    }
    
    private void MovePlayer()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        
        float _targetSpeed = _isSprinting ? (_moveSpeed * _sprintSpeed) : _moveSpeed;
        float acceleration = _inAir ? _airAcceleration : _groundAcceleration;
        
        Vector3 _targetHorizontalVelocity = _moveDirection.normalized * _targetSpeed;
        
        _currentHorizontalVelocity = Vector3.MoveTowards(
            _currentHorizontalVelocity, 
            _targetHorizontalVelocity, 
            acceleration * Time.deltaTime);

        Vector3 _targetVelocity = new Vector3(
            _currentHorizontalVelocity.x, 
            _verticalVelocity, 
            _currentHorizontalVelocity.z);
        
        if ((_characterController.collisionFlags & CollisionFlags.Above) != 0)
        {
            if (_verticalVelocity > 0)
            {
                _verticalVelocity = -0f; 
            }
        }
        
        _characterController.Move(_targetVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        _verticalVelocity = Mathf.Sqrt(_jumpForce * 2f * _gravity);
        _canJump = false;
        _inAir = true;
        Invoke(nameof(ResetJump), 0.25f);
    }
    
    private void ResetJump()
    {
        _canJump = true;
    }

    private void Gravity()
    {
        if (_characterController.isGrounded && _canJump)
        {
            _verticalVelocity = -0.2f;
            _inAir = false;
        }
        
        else
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
            if (_hasRun == false && _inAir == false)
            {
                StartCoroutine(CoyotteTime());
                _hasRun = true;
            }
        }
    }

    IEnumerator CoyotteTime()
    {
        yield return new WaitForSeconds(_coyotteTime);
        if (_inAir == false)
        {
            _inAir = true;
        }
        _hasRun = false;
    }
    
    public void TakeDamage(float DamageTaken)
    {
        for (int i = 0; i < DamageUI.transform.childCount; i++)
        {
            Color _color = DamageUI.transform.GetChild(i).gameObject.GetComponent<Image>().color;
            _color.a += DamageTaken;
            DamageUI.transform.GetChild(i).gameObject.GetComponent<Image>().color = _color;
            if (_color.a >= 1)
            {
                _isDead = true;
                Debug.Log("Mort");
            }
        }
    }

    private void SearchToHeal()
    {
        Color _color = DamageUI.transform.GetChild(0).gameObject.GetComponent<Image>().color;
        if (_color.a > 0 && _hasHeal == false && _isDead == false)
        {
            StartCoroutine(HealPlayer());
            _hasHeal = true;
        }
    }

    IEnumerator HealPlayer()
    {
        yield return new WaitForSeconds(_lifeHealTime);
        if (_isDead == false)
        {
            for (int i = 0; i < DamageUI.transform.childCount; i++)
            {
                Color _color = DamageUI.transform.GetChild(i).gameObject.GetComponent<Image>().color;
                if (_color.a >= 0)
                {
                    _color.a -= _lifeHealAmount;
                }
                DamageUI.transform.GetChild(i).gameObject.GetComponent<Image>().color = _color;
            
                _hasHeal = false;
            }
        }
    }
}