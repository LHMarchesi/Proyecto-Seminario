using UnityEngine;

[CreateAssetMenu(menuName = "SonOfOdin/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [TextArea(2, 5)]
    public string[] lines;
    public float charsPerSecond = 45f;
}
