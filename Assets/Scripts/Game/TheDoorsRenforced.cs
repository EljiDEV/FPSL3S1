using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TheDoorsRenforced : MonoBehaviour
{
    public string keyTag = "RenforcedKey";
    public float minImpactSpeed = 10f;
    public float breakForceMultiplier = 1.5f;
    public float breakTorque = 10f;
    private GameObject child;
    [SerializeField] public bool NeedKey = true;

    private Rigidbody doorRb;
    private bool isBroken = false;

    void Start()
    {
        doorRb = GetComponent<Rigidbody>();
        doorRb.isKinematic = true;
        child = this.transform.GetChild(0).gameObject;

        if (child != null)
        {
            
            Debug.Log($"{child.name}  a été assigné correctement au bouton {this.name} ");
        }
        else
        {
            Debug.Log($"pas d'enfant dans la cave");
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision ");
        

        if (NeedKey && collision.gameObject.CompareTag(keyTag))
        {
            Debug.Log("clé detecté");
                disabledChild();
        }

        if (NeedKey == false && collision.gameObject)
        {
            disabledChild();
            Debug.Log("pas besoin de clé");
        }

    }

    void disabledChild()
    {
        child.GameObject().SetActive(false);

    }
}

/*void BreakDoor(Collision collision)
{
    isBroken = true;
    doorRb.isKinematic = false;
    doorRb.useGravity = true;

    Vector3 impactDirection = collision.relativeVelocity.normalized;
    Vector3 impactPoint = collision.contacts[0].point;

    float impactForce = collision.relativeVelocity.magnitude * breakForceMultiplier;

    doorRb.AddForceAtPosition(impactDirection * impactForce, impactPoint, ForceMode.Impulse);
    doorRb.AddTorque(Random.insideUnitSphere * breakTorque, ForceMode.Impulse);
    gameObject.layer = LayerMask.NameToLayer("Bite");
}
}*/