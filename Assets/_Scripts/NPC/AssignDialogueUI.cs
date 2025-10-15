using UnityEngine;
using TMPro;
using UnityEditor;

public class AssignDialogueUI : MonoBehaviour
{
    public Canvas dialogueCanvas;
    public TextMeshProUGUI bodyText;

    void Reset()
    {
        dialogueCanvas = GetComponentInChildren<Canvas>(true);
        bodyText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void Start()
    {
        DialogueController ctrl = FindObjectOfType<DialogueController>();
        if (ctrl == null)
        {
            ctrl = new GameObject("DialogueController").AddComponent<DialogueController>();
        }

        var so = new SerializedObject(ctrl);
        so.FindProperty("dialogueCanvas").objectReferenceValue = dialogueCanvas;
        so.FindProperty("bodyText").objectReferenceValue = bodyText;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
