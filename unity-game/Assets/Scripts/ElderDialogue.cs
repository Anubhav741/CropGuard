
using UnityEngine;
using TMPro;

public class ElderDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private bool playerNearby = false;
    private bool dialogueOpen = false;
    private int dialogueIndex = 0;

    private string[] dialogues =
    {
        "Welcome, young friend. I am the elder of this village.",
        "Our village was once blessed with plenty of water.",
        "But now our wells are drying and farmers are struggling.",
        "Our groundwater is disappearing because of excessive water usage.",
        "Can you help us save our village?",
        "Explore the village and discover ways to conserve groundwater."
    };

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogueOpen)
                StartDialogue();
        }

        if (dialogueOpen && Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogue();
        }
    }

    void StartDialogue()
    {
        dialogueOpen = true;
        dialogueIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowDialogue();
    }

    void NextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex >= dialogues.Length)
        {
            CloseDialogue();
            return;
        }

        ShowDialogue();
    }

    void ShowDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = dialogues[dialogueIndex];
    }

    void CloseDialogue()
    {
        dialogueOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (dialogueOpen)
                CloseDialogue();
        }
    }
}
