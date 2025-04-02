using System;
using UnityEngine;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class DialogueNodeData
    {
        public enum EmotionOverride
        {
            None,
            Idle,
            Happy,
            Inlove,
            Angry
        }
        public enum LoveAmount
        {
            LargeMinus,
            MediumMinus,
            SmallMinus,
            None,
            SmallPlus,
            MediumPlus,
            LargePlus
        }


        public string NodeGUID;
        public string DialogueText;
        public int LoveValue;
        public EmotionOverride emotionOverride;
        public LoveAmount loveAmount;
        public Vector2 Position;
    }
}