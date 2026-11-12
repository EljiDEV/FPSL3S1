using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BroomEnemy : MonoBehaviour
{
    [Header("Basic Data")] 
    [SerializeField] private int _health;
    [SerializeField] private float _speed;
    [SerializeField] private float _targetDistance;
    
    [Header("Damage Data")]
    [SerializeField] private float _damage;
    [SerializeField] private float _damageCooldown;
    
    [Header("PushBack Data")]
    [SerializeField] private float _pushBackDistance;
    [SerializeField] private float _pushBackForce;
    
    Rigidbody _rigidbody;
    
    NavMeshAgent _navMeshAgent;
    Coroutine _coroutine;
    
    private bool _followPlayer;
    private bool _hitPlayer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
    }

    void Update()
    {
        float _playerDistance = Vector3.Distance(transform.position,MainGame.Instance.Player.transform.position);
        if (_playerDistance <= _targetDistance)
        {
            _followPlayer = true;
        }

        if (_followPlayer && _hitPlayer == false)
        {
            _navMeshAgent.SetDestination(MainGame.Instance.Player.transform.position);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == MainGame.Instance.Player)
        {
            _hitPlayer = true;
            MainGame.Instance.Player.GetComponent<PlayerMouvements>().TakeDamage(_damage);
            _rigidbody.AddForce(transform.forward * -_pushBackForce);
            _navMeshAgent.SetDestination(transform.position);
            _coroutine = StartCoroutine(PushBack());
        }
    }
    
    IEnumerator PushBack()
    {
        yield return new WaitForSeconds(_damageCooldown);
        _navMeshAgent.SetDestination(transform.position);
        _hitPlayer = false;
    }
}