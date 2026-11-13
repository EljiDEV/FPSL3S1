using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Basic Data")] 
    [SerializeField] private int _health;
    [SerializeField] private float _speed;
    [SerializeField] private float _targetDistance;
    [SerializeField] private float _shootDistance;
    [SerializeField] private GameObject _projectilePrefab;
    
    [Header("Shoot Data")]
    [SerializeField] private float _shootCooldown;
    
    Rigidbody _rigidbody;

    GameObject _shootPosition;
    NavMeshAgent _navMeshAgent;
    Coroutine _coroutine;
    
    private bool _followPlayer;
    private bool _canShoot;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
        _canShoot = true;
        _shootPosition = gameObject.transform.GetChild(0).gameObject;
    }

    void Update()
    {
        float _playerDistance = Vector3.Distance(transform.position,MainGame.Instance.Player.transform.position);
        if (_playerDistance <= _targetDistance)
        {
            _followPlayer = true;
        }

        if (_followPlayer)
        {
            _navMeshAgent.SetDestination(MainGame.Instance.Player.transform.position);
            if (_playerDistance <= _shootDistance)
            {
                _navMeshAgent.SetDestination(transform.position);
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        if (_canShoot)
        {
            _coroutine = StartCoroutine(ShootProjectile());
            _canShoot = false;
        }
    }

    IEnumerator ShootProjectile()
    {
        yield return new WaitForSeconds(_shootCooldown);
        Instantiate(_projectilePrefab, new Vector3(_shootPosition.transform.position.x,_shootPosition.transform.position.y,_shootPosition.transform.position.z), transform.rotation);
        _canShoot = true;
    }
    
    public void TakeDamage(int _damage)
    {
        _health -= _damage;
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
