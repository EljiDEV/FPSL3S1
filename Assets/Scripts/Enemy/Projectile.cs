using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private float _speed;
    [SerializeField] private float _damage;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        transform.rotation = Quaternion.LookRotation(MainGame.Instance.Player.transform.position - _rigidbody.position);
    }

    private void Update()
    {
        _rigidbody.linearVelocity = transform.forward * _speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<FlyingEnemy>() == null)
        {
            if (other.gameObject.GetComponent<PlayerMouvements>() != null)
            {
                MainGame.Instance.Player.GetComponent<PlayerMouvements>().TakeDamage(_damage);
            }
            Destroy(gameObject);
        }
    }
}
