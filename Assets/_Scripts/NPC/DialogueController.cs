using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] Canvas dialogueCanvas;
    [SerializeField] TextMeshProUGUI bodyText;

    [Header("Typing")]
    [SerializeField] float minBlipInterval = 0.035f;
    [SerializeField] float blipPitchJitter = 0.05f;
    [SerializeField] float autoCloseDelayOnLastLine = 0.35f; // auto end after final line

    DialogueData data;
    NPCInteractable currentNPC;
    int lineIndex;
    bool typing;
    float nextBlipTime, inputBlockUntil;

    public bool IsActive => dialogueCanvas && dialogueCanvas.enabled;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dialogueCanvas) dialogueCanvas.enabled = false;
    }

    public void StartDialogue(NPCInteractable npc, DialogueData dialogue)
    {
        if (!dialogueCanvas || !bodyText || dialogue == null)
        { Debug.LogError("DialogueController: assign dialogueCanvas, bodyText, and DialogueData."); return; }

        currentNPC = npc;
        data = dialogue;
        lineIndex = 0;
        bodyText.text = "";
        dialogueCanvas.enabled = true;
        inputBlockUntil = Time.unscaledTime + 0.12f;

        StopAllCoroutines();
        StartCoroutine(TypeCurrentLine());
    }

    IEnumerator TypeCurrentLine()
    {
        typing = true;
        nextBlipTime = 0f;
        string full = data.lines[lineIndex];
        bodyText.text = "";
        float delay = 1f / Mathf.Max(1f, data.charsPerSecond);

        for (int i = 0; i < full.Length; i++)
        {
            if (!typing) { bodyText.text = full; break; }
            bodyText.text += full[i];

            if (Time.unscaledTime >= nextBlipTime && currentNPC)
            {
                currentNPC.PlayBlip(Random.Range(-blipPitchJitter, blipPitchJitter));
                nextBlipTime = Time.unscaledTime + minBlipInterval;
            }
            yield return new WaitForSecondsRealtime(delay);
        }

        typing = false;

        // If last line, auto-close after a short delay
        if (lineIndex >= data.lines.Length - 1)
        {
            yield return new WaitForSecondsRealtime(autoCloseDelayOnLastLine);
            EndDialogue();
        }
    }

    void Update()
    {
        if (!dialogueCanvas || !dialogueCanvas.enabled) return;
        if (Time.unscaledTime < inputBlockUntil) return;

        // Only E advances or completes
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (typing)
            {
                typing = false;
                StopAllCoroutines();
                bodyText.text = data.lines[lineIndex];

                // If this was the last line, end immediately
                if (lineIndex >= data.lines.Length - 1)
                {
                    EndDialogue();
                    return;
                }
            }
            else
            {
                lineIndex++;
                if (lineIndex >= data.lines.Length) EndDialogue();
                else { StopAllCoroutines(); StartCoroutine(TypeCurrentLine()); }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) EndDialogue();
    }

    public void EndIfCurrent(NPCInteractable npc)
    {
        if (currentNPC == npc) EndDialogue();
    }

    public void EndDialogue()
    {
        if (dialogueCanvas) dialogueCanvas.enabled = false;
        StopAllCoroutines();
        if (bodyText) bodyText.text = "";
        var npc = currentNPC;
        currentNPC = null;
        data = null;
        if (npc) npc.OnDialogueEnded();
    }
}
