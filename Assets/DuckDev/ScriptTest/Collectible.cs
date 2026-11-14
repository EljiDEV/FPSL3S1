using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public Vector3 rotate = new Vector3(0, 40f, 0);
    public float speed = 1f;

    
    private void Update()
    {
        transform.Rotate(rotate * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            if (GameManager.instance != null)
            {
                GameManager.instance.CollectItem();
            }
            else
            {
                Debug.LogWarning("Attention : Aucun GameManager trouvé dans la scène !");
            }

            Debug.Log("Collected Item");

            
            Destroy(gameObject);
        }
    }
}