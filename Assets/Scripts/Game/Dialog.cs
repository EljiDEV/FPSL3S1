using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialogueBoxPanel;
    public TextMeshProUGUI dialogueText;

    [Header("Réglages")]
    public float typingSpeed = 0.05f;

    private Queue<string> _sentences = new Queue<string>();
    private bool _isTyping = false;
    private string _currentSentence = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialogueBoxPanel != null) dialogueBoxPanel.SetActive(false);
    }

    void Update()
    {
        
        if (dialogueBoxPanel != null && dialogueBoxPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (_isTyping)
                {
                    
                    StopAllCoroutines();
                    dialogueText.text = _currentSentence;
                    _isTyping = false;
                }
                else
                {
                    DisplayNextSentence();
                }
            }
        }
    }

    public void StartDialogue(string[] sentences)
    {
        if (dialogueBoxPanel != null) dialogueBoxPanel.SetActive(true);

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _sentences.Clear();
        foreach (string sentence in sentences)
        {
            _sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(_currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        
        _isTyping = false;
    }

    void EndDialogue()
    {
        if (dialogueBoxPanel != null) dialogueBoxPanel.SetActive(false);

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}