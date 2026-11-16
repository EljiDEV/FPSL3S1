using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangementScene : MonoBehaviour
{
    [SerializeField] private string nomDeLaScene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(nomDeLaScene);
            }
            else
            {
                SceneManager.LoadScene(nomDeLaScene);
            }
        }
    }
}