using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

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

    GameObject _shootPosition;
    NavMeshAgent _navMeshAgent;
    Coroutine _coroutine;
    
    private MeshRenderer _meshRenderer;
    private Color _originalColor;
    
    private bool _followPlayer;
    private bool _canShoot;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
        _canShoot = true;
        _shootPosition = gameObject.transform.GetChild(0).gameObject;
        
        _meshRenderer = GetComponent<MeshRenderer>();
        _originalColor = _meshRenderer.material.GetColor("_BaseColor");
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
                transform.rotation = Quaternion.LookRotation(MainGame.Instance.Player.transform.position - transform.position);
                
                if (CanSeePlayer())
                {
                    _navMeshAgent.isStopped = true;
                    Shoot();
                }
            }
            else
            {
                _navMeshAgent.isStopped = false;
            }
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 _startPos = transform.position + Vector3.up;
        Transform _playerTransform = MainGame.Instance.Player.transform;
        Vector3 _playerPos = _playerTransform.position + Vector3.up;

        Vector3 _direction = _playerPos - _startPos;
        float _distance = _direction.magnitude;

        RaycastHit _hit;

        if (Physics.Raycast(_startPos, _direction.normalized, out _hit, _distance))
        {
            return false;
        }
        return true;
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
            return;
        }
        FeedbackDamage();
    }
    
    private void FeedbackDamage()
    {
        _meshRenderer.material.DOKill();
        _meshRenderer.material.DOColor(Color.red, 0.05f).OnComplete(() =>
        {
            _meshRenderer.material.DOColor(_originalColor, 0.15f); 
        });
    }
}
