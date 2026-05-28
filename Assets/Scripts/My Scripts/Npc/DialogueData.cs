using UnityEngine;

[CreateAssetMenu(fileName = "NPC dialogue", menuName = "Dialogue system/Dialogue data")]
public class DialogueData : ScriptableObject
{
    [Header("NPC info")]

    public string nameNPC;

    [Header("Dialogues")]
    [TextArea(3, 5)]
    public string[] dialogues;
}
