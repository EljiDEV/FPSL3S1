using UnityEngine;
using System.Collections.Generic;

public class VacuumProto : MonoBehaviour
{
    [Header("Filtres & Portée")]
    public LayerMask suckableLayer;
    public LayerMask biteLayer;
    [Tooltip("Mettez les layers ou le joueur doit pas pouvoir aspirer l'objet (mur , canard, sols etc...")]
    public LayerMask obstacleLayer;
    public float reach = 10f;
    public float angle = 45f;
    public float absorbDistance = 1.5f;

    [Header("Aspiration")]
    public Transform suctionPoint;
    public float speed = 15f;
    public int capacity = 10;
    private Stack<GameObject> storedObjects = new Stack<GameObject>();

    private GameObject heldBiteObject = null;
    private Rigidbody heldBiteRb = null;
    private Collider[] heldBiteColliders = null;
    private Collider[] playerColliders = null;
    private Quaternion heldRotationOffset = Quaternion.identity;

    private bool originalUseGravity;
    private float originalLinearDamping;
    private float originalAngularDamping;
    private CollisionDetectionMode originalCollisionMode;

    [Header("Tir & Maintien")]
    public Transform launchPoint;
    public float launchVelocity = 25f;
    [Tooltip("Fermeté du maintien d'objet pour les gros objets, plus souple l objet va flotter devant l aspi, plus haut l objet va rester collé au canon")]
    public float holdSpringForce = 15f;
    [Tooltip("Sécurité rappel objet bug de collision")]
    public float maxHoldSpeed = 25f;
    [Tooltip("Garde la rota initial")]
    public bool keepInitialRelativeRotation = true;

    private int suckableLayerIndex;
    private int biteLayerIndex;

    private float nextActionTime = 0f;
    private float actionCooldown = 0.2f;

    void Start()
    {
        suckableLayerIndex = (int)Mathf.Log(suckableLayer.value, 2);
        biteLayerIndex = (int)Mathf.Log(biteLayer.value, 2);
        
        playerColliders = GetComponentsInParent<Collider>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(launchVelocity);
            return;
        }

        if (heldBiteObject != null)
        {
            if (Input.GetMouseButtonDown(1) && Time.time >= nextActionTime)
            {
                Shoot(0f);
                nextActionTime = Time.time + actionCooldown;
            }
        }
        else
        {
            if (Input.GetMouseButton(1) && Time.time >= nextActionTime)
            {
                Suck();
            }
        }
    }

    void FixedUpdate()
    {
        if (heldBiteObject != null && heldBiteRb != null && suctionPoint != null)
        {
            
            Vector3 direction = suctionPoint.position - heldBiteRb.position;
            heldBiteRb.linearVelocity = Vector3.ClampMagnitude(direction * holdSpringForce, maxHoldSpeed);

            
            heldBiteRb.angularVelocity = Vector3.zero;
            Quaternion targetRotation = keepInitialRelativeRotation 
                ? suctionPoint.rotation * heldRotationOffset 
                : suctionPoint.rotation;

            heldBiteRb.MoveRotation(targetRotation);
        }
    }

    void Suck()
    {
        if (suctionPoint == null) return;
        
        LayerMask combinedLayers = suckableLayer | biteLayer;
        Collider[] hits = Physics.OverlapSphere(transform.position, reach, combinedLayers);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            GameObject rootObj = rb.gameObject;

            
            Vector3 targetCenter = hit.bounds.center;
            Vector3 directionToTarget = (targetCenter - suctionPoint.position).normalized;

            if (Vector3.Angle(suctionPoint.forward, directionToTarget) <= angle)
            {
                
                if (Physics.Linecast(suctionPoint.position, targetCenter, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                Vector3 pullDirection = (suctionPoint.position - hit.transform.position).normalized;
                float distance = Vector3.Distance(hit.ClosestPoint(suctionPoint.position), suctionPoint.position);

                if (distance <= absorbDistance)
                {
                    if (rootObj.layer == suckableLayerIndex)
                    {
                        if (storedObjects.Count < capacity)
                        {
                            AbsorbSuckable(rootObj, rb);
                            break; 
                        }
                    }
                    else if (rootObj.layer == biteLayerIndex && heldBiteObject == null)
                    {
                        HoldBiteObject(rootObj, rb);
                        break;
                    }
                }
                else
                {
                    rb.linearVelocity = pullDirection * speed;
                }
            }
        }
    }

    void AbsorbSuckable(GameObject target, Rigidbody rb)
    {
        if (storedObjects.Contains(target)) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        target.SetActive(false);
        storedObjects.Push(target);
    }

    void HoldBiteObject(GameObject target, Rigidbody rb)
    {
        heldBiteObject = target;
        heldBiteRb = rb;

        
        originalUseGravity = heldBiteRb.useGravity;
        originalLinearDamping = heldBiteRb.linearDamping;
        originalAngularDamping = heldBiteRb.angularDamping;
        originalCollisionMode = heldBiteRb.collisionDetectionMode;

        
        heldRotationOffset = Quaternion.Inverse(suctionPoint.rotation) * heldBiteRb.rotation;

        
        heldBiteRb.isKinematic = false; 
        heldBiteRb.useGravity = false;
        heldBiteRb.linearDamping = 5f;
        heldBiteRb.angularDamping = 5f;
        heldBiteRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        heldBiteColliders = target.GetComponentsInChildren<Collider>();
        foreach (Collider col in heldBiteColliders)
        {
            foreach (Collider pCol in playerColliders)
            {
                if (col != null && pCol != null) Physics.IgnoreCollision(col, pCol, true);
            }
        }

        nextActionTime = Time.time + actionCooldown;
    }

    void Shoot(float appliedForce)
    {
        if (launchPoint == null) return;

        if (heldBiteObject != null && heldBiteRb != null)
        {
            if (heldBiteColliders != null)
            {
                foreach (Collider col in heldBiteColliders)
                {
                    foreach (Collider pCol in playerColliders)
                    {
                        if (col != null && pCol != null) Physics.IgnoreCollision(col, pCol, false);
                    }
                }
                heldBiteColliders = null;
            }

            heldBiteRb.useGravity = originalUseGravity;
            heldBiteRb.linearDamping = originalLinearDamping;
            heldBiteRb.angularDamping = originalAngularDamping;
            heldBiteRb.collisionDetectionMode = appliedForce > 0f 
                ? CollisionDetectionMode.ContinuousDynamic 
                : originalCollisionMode;

            heldBiteRb.linearVelocity = Vector3.zero;
            heldBiteRb.angularVelocity = Vector3.zero;
            
            float finalForce = appliedForce > 0f ? appliedForce : 2f;
            heldBiteRb.AddForce(launchPoint.forward * finalForce, ForceMode.Impulse);

            heldBiteObject = null;
            heldBiteRb = null;
            return; 
        }

        if (appliedForce > 0f && storedObjects.Count > 0)
        {
            GameObject obj = storedObjects.Pop();
            obj.transform.position = launchPoint.position;
            obj.transform.rotation = launchPoint.rotation;
            obj.SetActive(true);

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(launchPoint.forward * appliedForce, ForceMode.Impulse);
            }
        }
    }
}