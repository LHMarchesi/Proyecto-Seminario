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

    DialogueData data;
    NPCInteractable currentNPC;
    int lineIndex;
    bool typing;
    float nextBlipTime;
    float inputBlockUntil; // prevents the initial E from skipping

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dialogueCanvas) dialogueCanvas.enabled = false;
    }

    public void StartDialogue(NPCInteractable npc, DialogueData dialogue)
    {
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

            if (Time.unscaledTime >= nextBlipTime && currentNPC != null)
            {
                currentNPC.PlayBlip(Random.Range(-blipPitchJitter, blipPitchJitter));
                nextBlipTime = Time.unscaledTime + minBlipInterval;
            }

            yield return new WaitForSecondsRealtime(delay);
        }

        typing = false;
    }

    void Update()
    {
        if (!dialogueCanvas || !dialogueCanvas.enabled) return;

        // only E advances or completes
        bool advancePressed = Input.GetKeyDown(KeyCode.E);

        // prevent the initial press from skipping the first line
        if (Time.unscaledTime < inputBlockUntil)
            return;

        if (advancePressed)
        {
            if (typing)
            {
                typing = false;
                StopAllCoroutines();
                bodyText.text = data.lines[lineIndex]; // complete current line
            }
            else
            {
                lineIndex++;
                if (lineIndex >= data.lines.Length) EndDialogue();
                else { StopAllCoroutines(); StartCoroutine(TypeCurrentLine()); }
            }
        }

    
    }


    void EndDialogue()
    {
        dialogueCanvas.enabled = false;
        StopAllCoroutines();
        bodyText.text = "";
        var npc = currentNPC;
        currentNPC = null;
        data = null;
        if (npc) npc.OnDialogueEnded();
    }
}
