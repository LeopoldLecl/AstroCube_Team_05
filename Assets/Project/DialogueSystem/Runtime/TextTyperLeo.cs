using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RedBlueGames.Tools.TextTyper;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Subtegral.DialogueSystem.Runtime;
public class TextTyperLeo : MonoBehaviour
{
#pragma warning disable 0649 // Ignore "Field is never assigned to" warning, as these are assigned in inspector
    [SerializeField]
    private AudioClip printSoundEffect;

    [Header("UI References")]

    [SerializeField]
    private DialogueParser dialogueParser;

    [Header("UI References")]

    [SerializeField]
    private Button printNextButton;

    [SerializeField]
    private Button printNoSkipButton;

    [SerializeField]
    private Toggle pauseGameToggle;

    public Queue<string> dialogueLines = new Queue<string>();

    [SerializeField]
    [Tooltip("The text typer element to test typing with")]
    private TextTyper testTextTyper;
    

#pragma warning restore 0649
    public void Start()
    {


        this.testTextTyper.PrintCompleted.AddListener(this.HandlePrintCompleted);
        this.testTextTyper.CharacterPrinted.AddListener(this.HandleCharacterPrinted);

        this.printNextButton?.onClick.AddListener(this.HandlePrintNextClicked);
        this.printNoSkipButton?.onClick.AddListener(this.HandlePrintNoSkipClicked);

        //dialogueLines.Enqueue("Hello! My name is... <delay=0.5>NPC</delay>. Got it, <i>bub</i>?");
        //dialogueLines.Enqueue(" <anim=fullshake>MAXENS AAAAAAAAAHHHHHHHHHHH !!!!!!!!!!</anim>");
        Debug.Log(dialogueLines.Count);

        //ShowScript();
    }

    public void Update()
    {
        UnityEngine.Time.timeScale = this.pauseGameToggle.isOn ? 0.0f : 1.0f;

        if (Input.GetKeyDown(KeyCode.Space))
        {

            var tag = RichTextTag.ParseNext("blah<color=red>boo</color");
            LogTag(tag);
            tag = RichTextTag.ParseNext("<color=blue>blue</color");
            LogTag(tag);
            tag = RichTextTag.ParseNext("No tag in here");
            LogTag(tag);
            tag = RichTextTag.ParseNext("No <color=blueblue</color tag here either");
            LogTag(tag);
            tag = RichTextTag.ParseNext("This tag is a closing tag </bold>");
            LogTag(tag);
        }
    }

    private void HandlePrintNextClicked()
    {
        if (this.testTextTyper.IsSkippable() && this.testTextTyper.IsTyping)
        {
            this.testTextTyper.Skip();
            printNextButton.gameObject.SetActive(false);
        }
        else
        {
            ShowScript();
            printNextButton.gameObject.SetActive(false);
        }
    }

    private void HandlePrintNoSkipClicked()
    {
        ShowScript();
        printNoSkipButton.gameObject.SetActive(false);
    }

    public void ShowScript()
    {
        if (dialogueLines.Count <= 0)
        {
            return;
        }

        this.testTextTyper.TypeText(dialogueLines.Dequeue());
    }

    private void LogTag(RichTextTag tag)
    {
        if (tag != null)
        {
            Debug.Log("Tag: " + tag.ToString());
        }
    }

    private void HandleCharacterPrinted(string printedCharacter)
    {
        // Do not play a sound for whitespace
        if (printedCharacter == " " || printedCharacter == "\n")
        {
            return;
        }

        var audioSource = this.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = this.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = this.printSoundEffect;
        audioSource.Play();
    }

    private void HandlePrintCompleted()
    {
        Debug.Log("TypeText Complete");
        dialogueParser.isTextTyped = true;
        // proceed with narrative
    }

    public  void AddDialogue(string text)
    {
        printNextButton.gameObject.SetActive(true);
        if (text == null)
        {
            Debug.LogError("FUCK TEXT IS MISSING FIND IT");
        }
        //Debug.Log("Next text : " + text);
        dialogueLines.Enqueue(text);
    }
    
    public void DisplayChoiceButtons(bool show)
    {
            // thebuttons.setActive(show)
    }
}
