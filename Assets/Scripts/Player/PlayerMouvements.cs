using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
    [Tooltip("Distance supplémentaire du Raycast sous les pieds pour détecter le sol et les plateformes mobiles")]
    [SerializeField] private float _groundCheckDistance = 0.35f;
    [Tooltip("Layers considérés comme du sol (laisse Everything par défaut)")]
    [SerializeField] private LayerMask _groundMask = ~0;

    [Header("Life Data")]
    [SerializeField] private float _lifeHealAmount;
    [SerializeField] private float _lifeHealTime;
    public GameObject DamageUI;
    
    [Header("Effects Data")]
    [SerializeField] private float _walkEffectDuration;
    [SerializeField] private float _walkEffectAmount;
    [SerializeField] private float _runEffectDuration;
    [SerializeField] private float _runEffectAmount;
    
    [Header("Other Settings")]
    [SerializeField] private Transform _orientation;
    
    private PlayerCam _playerCam;
    
    private float _verticalVelocity;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private bool _isSprinting;
    private bool _canJump;
    private bool _inAir;
    private bool _isGroundedRay;
    private bool _effectStarted;

    private bool _hasHeal;
    private bool _isDead;
    private bool _hasRun;
    private string nomDeLaScene;
    
    private CharacterController _characterController;

    private Vector3 _moveDirection;
    private Vector3 _currentHorizontalVelocity;
    
    private Coroutine _effectCoroutine;
    
    void Start()
    {
        nomDeLaScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        _characterController = GetComponent<CharacterController>();
        _canJump = true;
        _effectStarted = false;
        _playerCam = FindAnyObjectByType<PlayerCam>();
        _isDead = false;
        
    }
    
    void Update()
    {
        _isGroundedRay = CheckGrounded();
        GetInput();
        Gravity();
        WalkingEffect();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SearchToHeal();
    }

    
    private bool CheckGrounded()
    {
        if (_characterController.isGrounded) return true;

        Bounds b = _characterController.bounds;
        Vector3 rayOrigin = b.center;
        float rayLength = b.extents.y + _groundCheckDistance;

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayLength, _groundMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            return true;
        }

        return false;
    }
    
    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        
        if (Input.GetKeyDown(KeyCode.Space) && _canJump && (_isGroundedRay || _inAir == false))
        {
            Jump();
        }
        else if (Input.GetKey(KeyCode.Space) && _canJump && (_isGroundedRay || _inAir == false))
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
        if (_isGroundedRay && _canJump)
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
        
        if (!_isGroundedRay)
        {
            _inAir = true;
        }
        _hasRun = false;
    }
    
    public void TakeDamage(float DamageTaken)
    {
        
        if (_isDead) return;

        for (int i = 0; i < DamageUI.transform.childCount; i++)
        {
            Image _image = DamageUI.transform.GetChild(i).GetComponent<Image>();
            Color _color = _image.color;
            
            
            float targetAlpha = Mathf.Clamp01(_color.a + DamageTaken);

            _image.DOKill();
            _image.DOFade(targetAlpha, 0.15f);

            
            if (targetAlpha >= 1f && !_isDead)
            {
                _isDead = true;
                Debug.Log("Mort");
            }
        }

        
        if (_isDead)
        {
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(activeScene);
            }
            else
            {
                
                UnityEngine.SceneManagement.SceneManager.LoadScene(activeScene);
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
                Image _image = DamageUI.transform.GetChild(i).GetComponent<Image>();
                if (_color.a >= 0)
                {
                    _color.a -= _lifeHealAmount;
                }
                _image.DOKill();
                _image.DOFade(_color.a, 0.25f);
                
                _hasHeal = false;
            }
        }
    }

    private void WalkingEffect()
    {
        if (_horizontalInput != 0 && _effectStarted == false || _verticalInput != 0 && _effectStarted == false)
        {
            if (_inAir == false && _isSprinting)
            {
                _effectStarted = true;
                _playerCam.WalkingEffect(_walkEffectAmount,_walkEffectDuration);
                if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
                _effectCoroutine = StartCoroutine(EffectRate(_walkEffectDuration));
            }
            else if (_inAir == false && _isSprinting == false)
            {
                _effectStarted = true;
                _playerCam.WalkingEffect(_runEffectAmount,_runEffectDuration);
                _effectCoroutine = StartCoroutine(EffectRate(_runEffectDuration));
            }
        }
    }

    IEnumerator EffectRate(float _time)
    {
        yield return new WaitForSeconds(_time);
        _effectStarted = false;
    }

    
    private void OnDrawGizmosSelected()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc == null) return;

        Bounds b = cc.bounds;
        Gizmos.color = _isGroundedRay ? Color.green : Color.red;
        Gizmos.DrawLine(b.center, b.center + Vector3.down * (b.extents.y + _groundCheckDistance));
    }
    private void OnDestroy()
    {
        if (DamageUI != null)
        {
            for (int i = 0; i < DamageUI.transform.childCount; i++)
            {
                Image img = DamageUI.transform.GetChild(i).GetComponent<Image>();
                if (img != null) img.DOKill();
            }
        }
    }
}