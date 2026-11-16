using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    
    public GameObject loadingScreenPanel;
    public Slider progressBar;

    
    [SerializeField] private float minimumLoadTime = 2.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            
            timer += Time.unscaledDeltaTime;

            
            
            float progressFromAsync = Mathf.Clamp01(operation.progress / 0.9f);
            float progressFromTimer = Mathf.Clamp01(timer / minimumLoadTime);
            
            
            float finalProgress = Mathf.Min(progressFromAsync, progressFromTimer);

            if (progressBar != null)
            {
                progressBar.value = finalProgress;
            }

            
            if (operation.progress >= 0.9f && timer >= minimumLoadTime)
            {
                progressBar.value = 1f;
                yield return new WaitForSecondsRealtime(0.2f );
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);
    }
}