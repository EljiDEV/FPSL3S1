using UnityEngine;
using UnityEngine.AI;

public class EnemyShooter : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    public float stopDistance = 5f; 
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    private float nextFireTime;
    public float power = 20f;

    
    public Vector3 aimOffset = new Vector3(0, 1.2f, 0);

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        
        agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
        {
            
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            
            agent.isStopped = true;

            
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        
        Vector3 targetPoint = player.position + aimOffset;
        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

        
        Collider enemyCollider = GetComponent<Collider>();
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (enemyCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, enemyCollider);
        }

        
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.AddForce(shootDirection * power, ForceMode.Impulse);
        }
    }
}