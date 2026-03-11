using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogueChoice
{
    public string choiceText;   // What player sees
    public int nextNodeIndex;   // Which node this choice goes to
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 5)]
    public string dialogueLine;         // NPC or player line

    public bool isPlayerLine;           // Optional, if you want to label who is speaking

    public List<DialogueChoice> choices; // If empty, dialogue continues automatically
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes;
    public int startNodeIndex = 0;
}
