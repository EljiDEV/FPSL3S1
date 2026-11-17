using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class BroomEnemy : MonoBehaviour
{
    [Header("Basic Data")] 
    [SerializeField] private int _health;
    [SerializeField] private float _speed;
    [SerializeField] private float _targetDistance;
    [SerializeField] private float _damageDistance;
    
    [Header("Damage Data")]
    [SerializeField] private float _damage;
    [SerializeField] private float _damageCooldown;
    
    [Header("PushBack Data")]
    [SerializeField] private float _pushBackDistance;
    [SerializeField] private float _pushBackForce;
    
    private MeshRenderer _meshRenderer;
    private Animator _animator;
    private Color _originalColor;
    
    private Renderer[] _renderers;
    
    NavMeshAgent _navMeshAgent;
    Coroutine _coroutine;
    
    private bool _followPlayer;
    private bool _hitPlayer;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _renderers = GetComponentsInChildren<Renderer>();
        _navMeshAgent.speed = _speed;
        
        _meshRenderer = GetComponent<MeshRenderer>();
        _originalColor = _meshRenderer.material.GetColor("_BaseColor");
        _animator = gameObject.transform.GetChild(0).GetComponent<Animator>();
    }

    void Update()
    {
        float _playerDistance = Vector3.Distance(transform.position,MainGame.Instance.Player.transform.position);
        if (_playerDistance <= _targetDistance)
        {
            _followPlayer = true;
        }

        if (_playerDistance <= _damageDistance && _hitPlayer == false)
        {
            _hitPlayer = true;
            MainGame.Instance.Player.GetComponent<PlayerMouvements>().TakeDamage(_damage);
            _navMeshAgent.isStopped = true;
            _animator.SetBool("Run", false);
            _coroutine = StartCoroutine(CoolDown());
        }

        if (_followPlayer && _hitPlayer == false)
        {
            _navMeshAgent.SetDestination(MainGame.Instance.Player.transform.position);
            _animator.SetBool("Run", true);
        }
    }
    
    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(_damageCooldown);
        _hitPlayer = false;
        _navMeshAgent.isStopped = false;
        _animator.SetBool("Run", true);
    }
    
    public void TakeDamage(int _damage)
    {
        _health -= _damage;
        if (_health <= 0)
        {
            Destroy(gameObject);
            return;
        }
        FeedbackDamage();
    }
    
    private void FeedbackDamage()
    {
        foreach (Renderer rend in _renderers)
        {
            rend.material.DOKill();
            rend.material.DOColor(Color.red, 0.05f).SetLoops(2, LoopType.Yoyo);
        }
    }
}