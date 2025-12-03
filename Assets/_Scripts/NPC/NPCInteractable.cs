using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Setup")]
    public DialogueData dialogue;   // Base / default dialogue

    [Header("Death-based Dialogue (optional)")]
    [Tooltip("Index 0 = after 1st death, 1 = after 2nd death, etc. If empty or null, the NPC will always use 'dialogue'.")]
    public DialogueData[] deathDialogues;

    public Transform promptCanvas;
    public KeyCode interactKey = KeyCode.E;

    [Header("Door + Dissolve")]
    public GameObject door;
    [Tooltip("Renderer of the dissolving object. If null, will search in door hierarchy.")]
    public Renderer doorRenderer;
    [Tooltip("Float property name in the dissolve shader.")]
    public string dissolveProperty = "_DissolveStrength";
    [Tooltip("Seconds to go from 0 to 1.")]
    public float dissolveDuration = 1.5f;

    private Material doorMat;          // instance material
    private int dissolvePropID;
    private Coroutine dissolveCo;

    [Header("Audio")]
    public AudioSource blipSource;

    [Header("Re-trigger Control")]
    public float reInteractCooldown = 1.0f;
    public bool requireExitToRetrigger = true;

    bool playerInside;
    bool dialogueActive;
    bool mustExitOnce;
    float nextAllowedTime;

    // ============================
    //  INFO POPUPS AFTER DIALOGUE
    // ============================
    [Header("Info Popups After Dialogue")]
    [Tooltip("Popups shown after this NPC finishes its dialogue, in order.")]
    [SerializeField] private GameObject[] infoPopups;

    [Tooltip("If true, popups will only be shown the first time the dialogue finishes.")]
    [SerializeField] private bool showPopupsOnlyOnce = true;

    private int currentPopupIndex = -1;
    private bool isShowingPopups = false;
    private bool popupsAlreadyShown = false;

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

        if (door) door.SetActive(true);

        if (!doorRenderer && door)
            doorRenderer = door.GetComponentInChildren<Renderer>(true);

        if (doorRenderer)
        {
            // instance material so we do not edit shared
            doorMat = doorRenderer.material;
            dissolvePropID = Shader.PropertyToID(dissolveProperty);

            if (doorMat.HasProperty(dissolvePropID))
                doorMat.SetFloat(dissolvePropID, 0f);
        }

        // Make sure all popups start hidden
        if (infoPopups != null)
        {
            for (int i = 0; i < infoPopups.Length; i++)
            {
                if (infoPopups[i])
                    infoPopups[i].SetActive(false);
            }
        }

        SoundManagerOcta.Instance.PlayMusic("MainTheme");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (CanInteractNow())
        {
            if (!dialogueActive && promptCanvas) promptCanvas.gameObject.SetActive(true);
            {

            }
        }
        playerContext = other.GetComponent<PlayerContext>();
    }

    PlayerContext playerContext;

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        mustExitOnce = false;

        if (promptCanvas) promptCanvas.gameObject.SetActive(false);

        if (DialogueController.Instance)
            DialogueController.Instance.EndIfCurrent(this);

        dialogueActive = false;

        // If the player leaves, make sure any popups are closed
        EndPopupsSequence();
    }

    void OnDisable()
    {
        if (DialogueController.Instance)
            DialogueController.Instance.EndIfCurrent(this);
        dialogueActive = false;

        // Clean up popups if this NPC is disabled
        EndPopupsSequence();
    }

    void Update()
    {
        if (!playerInside || dialogueActive) return;
        if (!CanInteractNow()) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(false);

            if (DialogueController.Instance)
            {
                
                DialogueData selectedDialogue = GetDialogueForCurrentDeath();

                if (selectedDialogue == null)
                {
                    Debug.LogWarning($"NPCInteractable on {name}: No DialogueData found, using default 'dialogue'.");
                    selectedDialogue = dialogue;
                }

                DialogueController.Instance.StartDialogue(this, selectedDialogue);
                dialogueActive = true;
                playerContext.HandleInputs.SetPaused(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }
    }

    bool CanInteractNow()
    {
        if (requireExitToRetrigger && mustExitOnce) return false;
        return Time.time >= nextAllowedTime;
    }

    // ============================
    //   NEW HELPER FOR DIALOGUE
    // ============================
    private DialogueData GetDialogueForCurrentDeath()
    {
        int deaths = PlayerDeathTracker.DeathCount;

        // If no deaths yet, always use base dialogue
        if (deaths <= 0) return dialogue;

        // If no special death dialogues assigned, fall back to base
        if (deathDialogues == null || deathDialogues.Length == 0) return dialogue;

        // deaths = 1 -> index 0, deaths = 2 -> index 1, ...
        int index = deaths - 1;

        // Clamp to last available variant if deaths exceed the array length
        if (index >= deathDialogues.Length)
            index = deathDialogues.Length - 1;

        DialogueData variant = deathDialogues[index];
        return variant != null ? variant : dialogue;
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
        playerContext.HandleInputs.SetPaused(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Begin dissolve to 1. Door deactivates after transition.
        if (dissolveCo != null) StopCoroutine(dissolveCo);
        dissolveCo = StartCoroutine(DissolveRoutine());

        nextAllowedTime = Time.time + reInteractCooldown;
        mustExitOnce = requireExitToRetrigger;

        if (playerInside && CanInteractNow())
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(true);
        }
        else
        {
            if (promptCanvas) promptCanvas.gameObject.SetActive(false);
        }

        UIManager.Instance.ChangeRemainingEnemiesText("MOVE FORWARD");

        // Start info popups sequence (if configured)
        StartPopupsSequence();
    }

    IEnumerator DissolveRoutine()
    {
        if (!door || !doorRenderer || !doorMat) yield break;
        if (!door.activeSelf) door.SetActive(true);

        if (!doorMat.HasProperty(dissolvePropID)) yield break;

        float start = doorMat.GetFloat(dissolvePropID);
        float t = 0f;

        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(start, 1f, Mathf.Clamp01(t / dissolveDuration));
            doorMat.SetFloat(dissolvePropID, v);
            yield return null;
        }

        doorMat.SetFloat(dissolvePropID, 1f);
        door.SetActive(false);
        dissolveCo = null;
    }

    // ============================
    //      POPUP SEQUENCE LOGIC
    // ============================

    private void StartPopupsSequence()
    {
        if (infoPopups == null || infoPopups.Length == 0) return;
        if (PlayerDeathTracker.DeathCount != 0) return; // Only show on first life
        if (showPopupsOnlyOnce && popupsAlreadyShown) return;
        popupsAlreadyShown = true;

        isShowingPopups = true;

        // Hide all first
        for (int i = 0; i < infoPopups.Length; i++)
        {
            if (infoPopups[i])
                infoPopups[i].SetActive(false);
        }

        currentPopupIndex = 0;
        ShowCurrentPopup();
    }

    private void ShowCurrentPopup()
    {
        if (!isShowingPopups) return;
        playerContext.HandleInputs.SetPaused(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        if (currentPopupIndex < 0 || currentPopupIndex >= infoPopups.Length)
        {
            EndPopupsSequence();
            return;
        }

        // Hide all and show only the current one
        for (int i = 0; i < infoPopups.Length; i++)
        {
            if (infoPopups[i])
                infoPopups[i].SetActive(i == currentPopupIndex);
        }
    }

    // Call this from the "Next" button on each popup
    public void ShowNextPopup()
    {
        if (!isShowingPopups) return;

        // Hide current popup
        if (currentPopupIndex >= 0 && currentPopupIndex < infoPopups.Length)
        {
            if (infoPopups[currentPopupIndex])
                infoPopups[currentPopupIndex].SetActive(false);
        }

        currentPopupIndex++;

        if (currentPopupIndex >= infoPopups.Length)
        {
            EndPopupsSequence();
        }
        else
        {
            ShowCurrentPopup();
        }
    }

    private void EndPopupsSequence()
    {
        if (infoPopups != null)
        {
            for (int i = 0; i < infoPopups.Length; i++)
            {
                if (infoPopups[i])
                {
                    infoPopups[i].SetActive(false);
                }
            }
        }

        isShowingPopups = false;
        currentPopupIndex = -1;

        // If you need to re-enable player controls or other systems after popups,
        // you can do it here.
    }

    public void ClosePopUp()
    {
        playerContext.HandleInputs.SetPaused(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
