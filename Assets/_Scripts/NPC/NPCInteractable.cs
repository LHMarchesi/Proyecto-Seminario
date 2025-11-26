using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Setup")]
    public DialogueData dialogue;
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
        SoundManagerOcta.Instance.PlayMusic("MainTheme");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (CanInteractNow())
        {
            if (!dialogueActive && promptCanvas) promptCanvas.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        mustExitOnce = false;

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

            if (DialogueController.Instance)
            {
                DialogueController.Instance.StartDialogue(this, dialogue);
                dialogueActive = true;
            }
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
}
