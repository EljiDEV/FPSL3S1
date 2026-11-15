using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TheDoors : MonoBehaviour
{
    public string keyTag = "Key";
    public float minImpactSpeed = 10f;
    public float breakForceMultiplier = 1.5f;
    public float breakTorque = 10f;

    private Rigidbody doorRb;
    private bool isBroken = false;

    void Start()
    {
        doorRb = GetComponent<Rigidbody>();
        doorRb.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        if (collision.gameObject.CompareTag(keyTag))
        {
            Rigidbody keyRb = collision.rigidbody;

            if (keyRb != null && keyRb.useGravity && collision.relativeVelocity.magnitude >= minImpactSpeed)
            {
                BreakDoor(collision);
            }
        }
    }

    void BreakDoor(Collision collision)
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
}