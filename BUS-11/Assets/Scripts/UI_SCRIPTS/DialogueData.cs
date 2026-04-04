using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DialogueBranchType
{
    Linear,
    Choice1,
    Choice2,
    Ending
}

public enum SpeakerType
{
    Driver,
    Ghost,
    Unknown,
    SchoolGirl,
    SchoolBoy,
    Nurse,
    OfficeWorker,
    ElderlyMan,
    ElderlyWoman
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

    public SpeakerType speaker;

    public int nextNodeIndex = -1;

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
