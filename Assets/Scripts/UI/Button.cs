using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    private Vector3 originalScale;
    [SerializeField] private string _sceneName;
    [SerializeField] private GameObject _credits;
    [SerializeField] private GameObject _menu;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter()
    {
        transform.DOKill();
        transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerExit()
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnLoadScene()
    {
        Time.timeScale = 1f;
        transform.DOKill();

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(_sceneName);
        }
        else
        {
            SceneManager.LoadScene(_sceneName);
        }
    }

    public void OnClickGameObject()
    {
        if (_credits.activeInHierarchy)
        {
            _credits.SetActive(false);
            _menu.SetActive(true);
        }
        else
        {
            _menu.SetActive(false);
            _credits.SetActive(true);
        }
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}