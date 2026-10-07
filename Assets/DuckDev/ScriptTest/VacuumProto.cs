using UnityEngine;
using System.Collections.Generic;

public class VacuumProto : MonoBehaviour
{
    public LayerMask Suckable;
    public float reach = 10f;
    public Transform SuctionPoint;
    public float speed = 15f;
    public int Capacity = 10;
    private Stack<GameObject> StoredObjects = new Stack<GameObject>();
    public Transform LaunchPoint;
    public float Velocity = 25f;
    public float angle = 45f;
    public float absorbDistance = 1.5f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Suck();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Shoot();
        }
    }

    void Suck()
    {
        if (StoredObjects.Count >= Capacity) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, reach, Suckable);
        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            GameObject rootObj = rb.gameObject;

            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(SuctionPoint.forward, directionToTarget) <= angle)
            {
                Vector3 direction = (SuctionPoint.position - hit.transform.position).normalized;
                
                float distance = Vector3.Distance(hit.ClosestPoint(SuctionPoint.position), SuctionPoint.position);

                if (distance <= absorbDistance)
                {
                    Absorb(rootObj);
                    break;
                }
                else
                {
                    rb.linearVelocity = direction * speed;
                }
            }
        }
    }

    void Absorb(GameObject target)
    {
        if (StoredObjects.Contains(target)) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        target.SetActive(false);
        StoredObjects.Push(target);
    }

    void Shoot()
    {
        if (StoredObjects.Count == 0) return;

        GameObject obj = StoredObjects.Pop();
        obj.transform.position = LaunchPoint.position;
        obj.transform.rotation = LaunchPoint.rotation;
        obj.SetActive(true);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(LaunchPoint.forward * Velocity, ForceMode.Impulse);
        }
    }
}