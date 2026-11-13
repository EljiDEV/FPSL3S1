using UnityEngine;

public class Collectible : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            GameManager.instance.CollectItem();
            Debug.Log("Collected Item");

            Destroy(gameObject);
        }
    }
}