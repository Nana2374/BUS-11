using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DialogueBranchType
{
    None,
    Friendly,
    Rude,
    Neutral,
    HorrorEscalation,
    Ending
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextNodeIndex;
}

[System.Serializable]
public class DialogueNode
{
    public int nodeID;   // automatic numbering

    public DialogueBranchType branchType;

    [TextArea(2, 5)]
    public string dialogueLine;

    public bool isPlayerLine;

    public List<DialogueChoice> choices;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{

    public List<DialogueNode> nodes;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (nodes == null) return;

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].nodeID = i;
        }
    }
#endif
}
