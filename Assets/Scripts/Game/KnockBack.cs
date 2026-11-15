using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyKnockback : MonoBehaviour
{
    [Header("Qui se fait soulever ?")]
    public bool pushPlayer = true;
    public bool pushEnemy = false;

    [Header("Réglages du soulevage")]
    [Tooltip("Puissance du soulevage")]
    public float pushForce = 12f;
    [Tooltip("Hauteur du soulevage")]
    public float upwardLift = 2f;
    [Tooltip("Durée du soulevage")]
    public float pushDuration = 0.25f;

    private bool _isPushing;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponentInParent<NavMeshAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPushing) return;

        
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMouvements>() != null)
        {
            Transform playerTransform = other.transform.root;
            CharacterController playerCC = playerTransform.GetComponentInChildren<CharacterController>();

            Debug.Log("Joueur touché");
            Vector3 dirToPlayer = playerTransform.position - transform.position;
            dirToPlayer.y = 0f;
            dirToPlayer = dirToPlayer.sqrMagnitude > 0.001f ? dirToPlayer.normalized : transform.forward;

            StartCoroutine(KnockbackRoutine(playerCC, dirToPlayer));
        }
    }

    private IEnumerator KnockbackRoutine(CharacterController playerCC, Vector3 dirToPlayer)
    {
        _isPushing = true;
        float timer = 0f;

        
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        while (timer < pushDuration)
        {
            timer += Time.deltaTime;
            
            float strength = 1f - (timer / pushDuration);

            
            if (pushPlayer && playerCC != null)
            {
                Debug.Log("Joueur soulevé!");
                Vector3 velocity = (dirToPlayer * pushForce + Vector3.up * upwardLift) * strength;
                playerCC.Move(velocity * Time.deltaTime);
            }

            
            if (pushEnemy)
            {
                Debug.Log("Meme l'ennemi veut pas te soulever");
                Vector3 enemyStep = -dirToPlayer * (pushForce * strength * Time.deltaTime);
                if (_agent != null && _agent.isOnNavMesh)
                    _agent.Move(enemyStep);
                else
                    transform.root.position += enemyStep;
            }

            yield return null;
        }

        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
        }

        _isPushing = false;
    }
}