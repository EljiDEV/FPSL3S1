using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [Tooltip("Écris chaque ligne de dialogue ici dans l'Inspector")]
    public string[] sentences;

    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_hasTriggered && (other.CompareTag("Player") || other.GetComponentInParent<PlayerMouvements>() != null))
        {
            TriggerDialogue();
            _hasTriggered = true;
        }
    }

    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(sentences);
        }
    }
}