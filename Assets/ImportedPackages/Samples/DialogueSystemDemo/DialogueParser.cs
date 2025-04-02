using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Subtegral.DialogueSystem.DataContainers;
//using Subtegral.DialogueSystem.Editor;
using Unity.VisualScripting;

namespace Subtegral.DialogueSystem.Runtime
{

    public enum CurrentCharacter {NERD = 0, BULLY = 1, EMO = 2}

    public class DialogueParser : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TextTyperLeo textTyperLeo;
        [SerializeField] private loveScript loveBar;
        [SerializeField] private SceneFader sceneFader;
        

        [Header("Componnents")]
        [SerializeField] public DialogueContainer dialogue;
        [SerializeField] private CurrentCharacter character = CurrentCharacter.NERD;
        [SerializeField] private TextMeshProUGUI dialogueName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Button choicePrefab2;
        [SerializeField] private Button nextSceneButton;
        [SerializeField] private Transform buttonContainer;

        [HideInInspector] public bool isTextTyped;
        [HideInInspector] public bool isGameOver = false;
        CurrentCharacter currentCharacter;
        private string nextScene;
        private string gameOverScene;
        public Coroutine dialogueCoroutine;
        

        //private List<DialogueNode> entryPoints;

        private void Start()
        {
            switch(character)
            {
                case CurrentCharacter.NERD:
                    gameOverScene = "EndingScene1";
                    break;
                case CurrentCharacter.BULLY:
                    gameOverScene = "EndingScene2";
                    break;
                case CurrentCharacter.EMO:
                    gameOverScene = "EndingScene3";
                    break;
            }
            var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node
            nextScene = sceneFader.sceneName;
            StartDialogue();
        }


        private void ProcessOutputNodes(string narrativeDataGUID, int entry)
        {
            //var text = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueText;
            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);
            //dialogueText.text = ProcessProperties(text);
            //DeleteButtons();

            var data = dialogue.DialogueNodeData.Where(x => x.NodeGUID == narrativeDataGUID).First();
            if (!choices.Any())
            {

                Debug.Log("NEXT SCENE");
                var button = Instantiate(choicePrefab2, buttonContainer);
                if(isGameOver) StartCoroutine(sceneFader.FadeToScene(gameOverScene));
                nextSceneButton.gameObject.SetActive(true);
            }
            else if (choices.Count() < 2) //the node is a monologue dialogue
            {

                var nextNode = choices.First();
                var button = Instantiate(choicePrefab2, buttonContainer);
                button.onClick.AddListener(() => NextDialogue(nextNode.TargetNodeGUID, entry));
                return;
            }
            else foreach (var choice in choices)
                {
                    var button = Instantiate(choicePrefab, buttonContainer);
                    button.GetComponentInChildren<TMP_Text>().text = ProcessProperties(choice.PortName);
                    button.onClick.AddListener(() => NextDialogue(choice.TargetNodeGUID, entry));
                }
        }

        private void CheckLoveBonus(DialogueNodeData.LoveAmount loveAmount)
        {
            switch (loveAmount)
            {
                case DialogueNodeData.LoveAmount.LargeMinus:
                    loveBar.BigLoveSubstraction();
                    break;
                case DialogueNodeData.LoveAmount.MediumMinus:
                    loveBar.MediumLoveSubstraction();
                    break;
                case DialogueNodeData.LoveAmount.SmallMinus:
                    loveBar.SmallLoveSubstraction();
                    break;
                case DialogueNodeData.LoveAmount.None:
                    break;
                case DialogueNodeData.LoveAmount.SmallPlus:
                    loveBar.SmallLoveAddition();
                    break;
                case DialogueNodeData.LoveAmount.MediumPlus:
                    loveBar.MediumLoveAddition();
                    break;
                case DialogueNodeData.LoveAmount.LargePlus:
                    loveBar.BigLoveAddition();
                    break;
            }
        }

        private void CheckEmotions(DialogueNodeData.EmotionOverride emote)
        {
            loveBar.currentOverridenEmotion = emote;
            //switch (emote)
            //{
            //    case DialogueNodeData.EmotionOverride.None:
            //        break;

            //    case DialogueNodeData.EmotionOverride.Idle:
            //        loveBar.rizzCharacterState = 1;
            //        break;
            //    case DialogueNodeData.EmotionOverride.Disappointed:
            //        loveBar.rizzCharacterState = 0;
            //        break;
            //    case DialogueNodeData.EmotionOverride.Happy:
            //        loveBar.rizzCharacterState = 2;
            //        break;
            //    case DialogueNodeData.EmotionOverride.Inlove:
            //        loveBar.rizzCharacterState = 3;
            //        break;
            //}
            loveBar.RizzCharacterEmotionUpdater();
        }

        public void CheckLoveBar(int love)
        {
            Debug.Log("Current love : " + love);
            if (love < 5)
            {
                ProceedToEndingDialogue(1);
            }
            if (love > 95)
            {
                ProceedToEndingDialogue(2);
            }
        }

        private void DeleteButtons()
        {
            var buttons = buttonContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i].gameObject);
            }
        }

        private void NextDialogue(string targetNodeGUID, int entry)
        {
            dialogueCoroutine = StartCoroutine(Proceed(targetNodeGUID, entry));
            //Debug.Log("Current Dialogue" + dialogueCoroutine != null);
            textTyperLeo.ShowScript();
        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }

        public void StartDialogue()
        {
            var narrativeData = dialogue.NodeLinks.First();
            StartCoroutine(Proceed(narrativeData.TargetNodeGUID, 0));
            textTyperLeo.ShowScript();
        }

        public IEnumerator Proceed(string nodeGUID, int entry)
        {
            if (isGameOver)
            {
                StartCoroutine(sceneFader.FadeToScene(gameOverScene));
                yield break;
                
            }
            var data = dialogue.DialogueNodeData.First(x => x.NodeGUID == nodeGUID) ;
            Debug.Log("HAHAHA DIALOGUE TEXT IS : " + data.DialogueText);
            CheckEmotions(data.emotionOverride);
            CheckLoveBonus(data.loveAmount);

            DialogueNodeData dialogueNode = dialogue.DialogueNodeData.Find(x => x.NodeGUID == nodeGUID);
            DeleteButtons();
            textTyperLeo.AddDialogue(dialogueNode.DialogueText);
            isTextTyped = false;
            if (dialogueCoroutine != null) yield break;
            while (!isTextTyped)
            {
                yield return new WaitForSeconds(0.1f);
            }
            dialogueCoroutine = null;
            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == nodeGUID);
            ProcessOutputNodes(nodeGUID, entry);
        }


        public void ProceedToEndingDialogue(int ending)
        {
            isGameOver = true;
            nextScene = $"EndingScene{(int)currentCharacter + 1}";
            Debug.Log("Proceeding to scene" + nextScene);

            //textTyperLeo.dialogueLines.Clear();
            Debug.Log(dialogue.endings);
            StartCoroutine(Proceed(dialogue.endings[ending].NodeGUID, ending));
            //switch (ending)
            //{
            //    case 0:
            //        Debug.Log("Huh?");
            //        break;

            //    case 1:
            //        //var ending1 = dialogue.NodeLinks[-1]; //ending node ?
            //        StartCoroutine(Proceed(dialogue.endings[1].NodeGUID, 1));
            //        break;
            //    case 2:
            //        //var ending2 = dialogue.NodeLinks[-2]; //ending2 node ?
            //        StartCoroutine(Proceed(dialogue.endings[2].NodeGUID, 1));
            //        break;
            //}
        }
    }
}