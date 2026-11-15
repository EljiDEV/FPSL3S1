using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuration")]
    public GameObject collectiblesContainer; 
    public TextMeshProUGUI scoreText;
    public GameObject winPanel;

    private int totalCollectibles;
    private int collectedCount = 0;
    private bool isQuestActive = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        
        if (collectiblesContainer == null)
        {
            Debug.LogError("ATTENTION ! La case 'Collectibles Container' est VIDE dans le GameManager !");
            return; 
        }

        if (scoreText == null)
        {
            Debug.LogError("ATTENTION ! La case 'Score Text' est VIDE dans le GameManager !");
            return; 
        }

        
        totalCollectibles = collectiblesContainer.transform.childCount;
        collectiblesContainer.SetActive(false); 
        scoreText.gameObject.SetActive(false);
        
        if(winPanel != null) winPanel.SetActive(false);
        StartQuest();
    }

    
    public void StartQuest()
    {
        if (isQuestActive) return; 

        isQuestActive = true;
        collectiblesContainer.SetActive(true); 
        scoreText.gameObject.SetActive(true);  
        UpdateUI();
        
        Debug.Log("La quête a commencé !");
    }

    public void CollectItem()
    {
        if (!isQuestActive) return;

        collectedCount++;
        UpdateUI();

        if (collectedCount >= totalCollectibles)
        {
            WinGame();
        }
    }

    void UpdateUI()
    {
        scoreText.text = $"Objets : {collectedCount} / {totalCollectibles}";
    }

    void WinGame()
    {
        Debug.Log("Bravo !");
        if(winPanel != null) winPanel.SetActive(true);
    }
}