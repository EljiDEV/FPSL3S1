using System;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Transform _orientation;
    private float _horizontalInput;
    private float _verticalInput;
    private Vector3 _moveDirection;
    [SerializeField] private Rigidbody _rb;
    void Start()
    {
        _rb.GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }
    void Update()
    {
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
    }

    private void MovePlayer()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
    }
}
