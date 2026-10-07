using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class VacuumProto : MonoBehaviour
{
    public LayerMask Suckable;
    public float reach;
    public Transform SuctionPoint;
    public float speed;
    public int Capacity= 10;
    private Stack <GameObject> StoredObjects;
    private Vector2 SucksTraj;
    public Transform LaunchPoint;
    public float Velocity;
    public float angle;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Suck();
            Debug.Log("Slurp");
        }

        if (Input.GetMouseButton(1))
        {
            Shoot();
        }
        
    }

    void Suck()
    {
        if (StoredObjects.Count >= Capacity) return;
        {
            Collider[]  hits = Physics.OverlapSphere(SuctionPoint.position, reach, Suckable);
            foreach (Collider hit in hits)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = SuctionPoint.transform.position - hit.transform.position;
                    float distance = direction.magnitude;
                    if(distance <= reach)
                    {
                        rb.AddForce(direction.normalized * speed, ForceMode.Force);
                    }
                    else
                    {
                        Absorb(Collider.);
                    }
                }
            }
        }
    }

    void Absorb(GameObject target)
    {
        
    }

    void Shoot()
    {
        if (StoredObjects.Count > 0)
        {
            
        }
        
    }
}
