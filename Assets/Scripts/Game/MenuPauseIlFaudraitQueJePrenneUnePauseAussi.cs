using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPauseIlFaudraitQueJePrenneUnePauseAussi : MonoBehaviour
{
    
    public GameObject pausePanel;
    
    public string mainMenuSceneName = "MainMenu";

    private bool _isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        _isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    
    public void Resume()
    {
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}