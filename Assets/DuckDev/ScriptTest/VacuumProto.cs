using UnityEngine;
using System.Collections.Generic;

public class VacuumProto : MonoBehaviour
{
    [Header("Filtres & Portée")]
    public LayerMask suckableLayer;
    public LayerMask biteLayer;
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

    [Header("Tir")]
    public Transform launchPoint;
    public float launchVelocity = 25f;

    private int suckableLayerIndex;
    private int biteLayerIndex;

    private float nextActionTime = 0f;
    private float actionCooldown = 0.2f;

    void Start()
    {
        suckableLayerIndex = (int)Mathf.Log(suckableLayer.value, 2);
        biteLayerIndex = (int)Mathf.Log(biteLayer.value, 2);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Shoot(launchVelocity);
            return;
        }

        if (heldBiteObject != null)
        {
            heldBiteObject.transform.position = suctionPoint.position;
            heldBiteObject.transform.rotation = suctionPoint.rotation;

            if (Input.GetMouseButtonDown(0) && Time.time >= nextActionTime)
            {
                Shoot(0f);
                nextActionTime = Time.time + actionCooldown;
            }
        }
        else
        {
            if (Input.GetMouseButton(0) && Time.time >= nextActionTime)
            {
                Suck();
            }
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
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;

            if (Vector3.Angle(suctionPoint.forward, directionToTarget) <= angle)
            {
                Vector3 direction = (suctionPoint.position - hit.transform.position).normalized;
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
                    rb.linearVelocity = direction * speed;
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

        heldBiteRb.linearVelocity = Vector3.zero;
        heldBiteRb.angularVelocity = Vector3.zero;
        heldBiteRb.isKinematic = true;

        heldBiteColliders = target.GetComponentsInChildren<Collider>();
        foreach (Collider col in heldBiteColliders)
        {
            col.enabled = false;
        }

        nextActionTime = Time.time + actionCooldown;
    }

    void Shoot(float appliedForce)
    {
        if (launchPoint == null) return;

        if (heldBiteObject != null && heldBiteRb != null)
        {
            heldBiteObject.transform.position = launchPoint.position; 
            
            if (heldBiteColliders != null)
            {
                foreach (Collider col in heldBiteColliders)
                {
                    if (col != null) col.enabled = true;
                }
                heldBiteColliders = null;
            }

            heldBiteRb.isKinematic = false;
            heldBiteRb.linearVelocity = Vector3.zero;
            
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
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(launchPoint.forward * appliedForce, ForceMode.Impulse);
            }
        }
    }
}