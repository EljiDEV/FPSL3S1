using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    private Vector3 originalScale;
    [SerializeField] private string _sceneName;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter()
    {
        transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit()
    {
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnLoadScene()
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
