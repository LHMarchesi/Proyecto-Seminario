using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Setup")]
    public DialogueData dialogue;
    public Transform promptCanvas;         // world-space prompt (optional)
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio")]
    public AudioSource blipSource;

    [Header("Re-trigger Control")]
    public float reInteractCooldown = 1.0f; // seconds after finish
    public bool requireExitToRetrigger = true;

    bool playerInside;
    bool dialogueActive;
    bool mustExitOnce;
    float nextAllowedTime;

    void Reset()
    {
        blipSource = GetComponent<AudioSource>();
        if (blipSource)
        {
            blipSource.playOnAwake = false;
            blipSource.loop = false;
            blipSource.spatialBlend = 1f;
        }
    }

    void Start()
    {
        if (promptCanvas) promptCanvas.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // Only show prompt if re-trigger is allowed
        if (CanInteractNow())
        {
            if (!dialogueActive && promptCanvas) promptCanvas.gameObject.SetActive(true);
           
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        mustExitOnce = false; // exit satisfied

        if (promptCanvas) promptCanvas.gameObject.SetActive(false);
        

        if (DialogueController.Instance)
            DialogueController.Instance.EndIfCurrent(this);

        dialogueActive = false;
    }

    void OnDisable()
    {
        if (DialogueController.Instance)
            DialogueController.Instance.EndIfCurrent(this);
        dialogueActive = false;
    }

    void Update()
    {
        if (!playerInside || dialogueActive) return;
        if (!CanInteractNow()) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(false);
            

            DialogueController.Instance.StartDialogue(this, dialogue);
            dialogueActive = true;
        }
    }

    bool CanInteractNow()
    {
        if (requireExitToRetrigger && mustExitOnce) return false;
        return Time.time >= nextAllowedTime;
    }

    // Called by DialogueController while typing
    public void PlayBlip(float pitchJitter = 0f)
    {
        if (!blipSource || !blipSource.clip) return;
        blipSource.pitch = 1f + pitchJitter;
        blipSource.PlayOneShot(blipSource.clip);
    }

    // Called by DialogueController when dialogue ends
    public void OnDialogueEnded()
    {
        dialogueActive = false;

        // Set re-trigger gates
        nextAllowedTime = Time.time + reInteractCooldown;
        mustExitOnce = requireExitToRetrigger;

        // Re-show prompt only if allowed and still inside
        if (playerInside && CanInteractNow())
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(true);
           
        }
        else
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(false);
            
        }
    }
}
