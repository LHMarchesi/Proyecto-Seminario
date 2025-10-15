using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Setup")]
    public DialogueData dialogue;
    public Transform promptCanvas;
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio")]
    public AudioSource blipSource;

    bool playerInside;
    bool dialogueActive;

    void Reset()
    {
        blipSource = GetComponent<AudioSource>();
        if (blipSource) { blipSource.playOnAwake = false; blipSource.loop = false; }
    }

    void Start()
    {
        if (promptCanvas) promptCanvas.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (!dialogueActive && promptCanvas) promptCanvas.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (promptCanvas) promptCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInside || dialogueActive) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(false);
            DialogueController.Instance.StartDialogue(this, dialogue);
            dialogueActive = true;
        }
    }

    public void OnDialogueEnded()
    {
        dialogueActive = false;
        if (playerInside && promptCanvas) promptCanvas.gameObject.SetActive(true);
    }

    // Called by DialogueController while typing
    public void PlayBlip(float pitchJitter = 0f)
    {
        if (!blipSource || !blipSource.clip) return;
        blipSource.pitch = 1f + pitchJitter;
        blipSource.PlayOneShot(blipSource.clip);
    }
}
