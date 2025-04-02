using Subtegral.DialogueSystem.DataContainers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;




namespace Subtegral.DialogueSystem.Editor
{

    public class DialogueNode : Node
    {
        public string DialogueText;
        public DialogueNodeData.EmotionOverride emotionOverride;
        public DialogueNodeData.LoveAmount loveAmount;
        public int LoveValue;
        public string GUID;
        public bool EntryPoint = false;
    }

    public class MonologueNode : DialogueNode
    {

    }

    public class EndingNode : DialogueNode
    {
        public int EntryNodeID;
        public EndingNode()
        {
            EntryPoint = true;
        }
    }
}
