using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class loveScript : MonoBehaviour
{
    [SerializeField] SceneFader sceneFader;
    [SerializeField] DialogueParser dialogueParser;

    [Header("bar Value")]

    [SerializeField] public Slider loveBar;
    [SerializeField] public float totalLove; // Set to public by me Maxens
    [SerializeField] private float loveStartValue;

    [Header("love states")]
    [SerializeField] private Color colorMaxStage;
    [SerializeField] private float percentageMaxLove;
    [SerializeField] private Color colorStageFive;
    [SerializeField] private float percentageStageLoveFour;
    [SerializeField] private Color colorStageFour;
    [SerializeField] private float percentageStageLoveThree;
    [SerializeField] private Color colorStageThree;
    [SerializeField] private float percentageStageLoveTwo;
    [SerializeField] private Color colorStageTwo;
    [SerializeField] private float percentageStageLoveOne;
    [SerializeField] private Color colorStageOne;
    [SerializeField] private float percentageMinLove;
    [SerializeField] private Color colorMinStage;



    [Header("love amount modifiying")]


    [SerializeField] private float smallLoveModification;
    [SerializeField] private float mediumLoveModification;
    [SerializeField] private float bigLoveModification;

    [Header("bar juiciness")]
    [SerializeField] private float barJuiciness;
    [SerializeField] private float velocityJuicy;

    [Header("love bar moving with time")]
    [SerializeField] private bool isLoveMovingWithTime;
    [SerializeField] private bool isLoveIcreasingOrDecreasing;
    [SerializeField] private float loveBarIncreasingMultiplier = 1;

    [Header("Characters Emotions")]
    [SerializeField] private GameObject rizzCharacter;
    [SerializeField] private Sprite rizzCharacterAngry;
    [SerializeField] private Sprite rizzCharacterIdle;
    [SerializeField] private Sprite rizzCharacterHappy;
    [SerializeField] private Sprite rizzCharacterInLove;



    [HideInInspector] public int rizzCharacterState = 0;
    [HideInInspector] public bool currentEmotionOverride = false;
    [HideInInspector] public DialogueNodeData.EmotionOverride currentOverridenEmotion = DialogueNodeData.EmotionOverride.None;

    private Coroutine coroutineFade = null;


    private void Start()
    {
        loveBar.maxValue = totalLove;
        loveBar.minValue = 0;
        loveBar.value = loveStartValue;


    }

    private void FixedUpdate()
    {

        loveIncreaseWithTime();
       RizzCharacterEmotionUpdater();
        return;
        if (dialogueParser.isGameOver) return;
        //mini 1/7
        if (loveBar.value <= totalLove * percentageMinLove)
        {
            Debug.Log("t'as perdu 1/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorMinStage;
            rizzCharacterState = 0;
            if (!dialogueParser.isGameOver)
            {
                dialogueParser.isGameOver = true;
                Debug.Log("GAME OVER : HATE");
                dialogueParser.ProceedToEndingDialogue(1);
            }
        }
        // 2/7
        if (loveBar.value > totalLove * percentageMinLove && loveBar.value < totalLove * percentageStageLoveOne)
        {
            Debug.Log("2/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorStageOne;
            rizzCharacterState = 0;
        }
        // 3/7
        if (loveBar.value > totalLove * percentageStageLoveOne && loveBar.value < totalLove * percentageStageLoveTwo)
        {
            Debug.Log("3/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorStageTwo;
            rizzCharacterState = 1;

        }
        // 4/7
        if (loveBar.value > totalLove * percentageStageLoveTwo && loveBar.value < totalLove * percentageStageLoveThree)
        {
            Debug.Log("4/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorStageThree;
            rizzCharacterState = 2;

        }
        // 5/7
        if (loveBar.value > totalLove * percentageStageLoveThree && loveBar.value < totalLove * percentageStageLoveFour)
        {
            Debug.Log("5/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorStageFour;
            rizzCharacterState = 2;

        }
        // 6/7
        if (loveBar.value > totalLove * percentageStageLoveThree && loveBar.value < totalLove * percentageStageLoveFour)
        {
            Debug.Log("6/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorStageFive;
            rizzCharacterState = 3;

        }
        // 7/7
        if (loveBar.value >= totalLove * percentageMaxLove)
        {
            Debug.Log("t'as perdu 7/7");
            loveBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = colorMaxStage;
            rizzCharacterState = 3;
            if (coroutineFade == null)
            {
                dialogueParser.isGameOver = true;
                dialogueParser.ProceedToEndingDialogue(2);
            }
        }
    }

    private IEnumerator SmoothlyUpdateLove(float targetValue, float modificationAmount, float duration)
    {
        dialogueParser.CheckLoveBar((int)targetValue);
        float elapsedTime = 0f;
        float startValue = loveBar.value;
        while (elapsedTime < duration)
        {
            loveBar.value = Mathf.Lerp(loveBar.value, targetValue, elapsedTime);
            //loveBar.value = Mathf.SmoothDamp(startValue, targetValue,ref velocityJuicy, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        loveBar.value = targetValue;
        
    }

    public void SmallLoveSubstraction()
    {
        StartCoroutine(SmoothlyUpdateLove(loveBar.value - smallLoveModification, smallLoveModification, barJuiciness));
    }
    public void SmallLoveAddition()
    {
        StartCoroutine(SmoothlyUpdateLove(loveBar.value + smallLoveModification, smallLoveModification, barJuiciness));
    }
    public void MediumLoveSubstraction()
    {
        StartCoroutine(SmoothlyUpdateLove(loveBar.value - mediumLoveModification, mediumLoveModification, barJuiciness));
    }
    public void MediumLoveAddition()
    {
        StartCoroutine(SmoothlyUpdateLove(loveBar.value + mediumLoveModification, mediumLoveModification, barJuiciness));
    }
    public void BigLoveSubstraction()
    {
        StartCoroutine(SmoothlyUpdateLove(loveBar.value - bigLoveModification, bigLoveModification, barJuiciness));
    }
    public void BigLoveAddition()
    {
        StartCoroutine(SmoothlyUpdateLove(loveBar.value + bigLoveModification, bigLoveModification, barJuiciness));
    }
    private void loveIncreaseWithTime()
    {
        if (isLoveMovingWithTime)
        {
            if (isLoveIcreasingOrDecreasing)
            {
                loveBar.value += Time.deltaTime * loveBarIncreasingMultiplier;
            }
            else
            {
                loveBar.value -= Time.deltaTime * loveBarIncreasingMultiplier;

            }
        }
    }



    public void RizzCharacterEmotionUpdater()
    {
        if (currentOverridenEmotion == DialogueNodeData.EmotionOverride.None)
        {
            switch (rizzCharacterState)
            {
                case 0:
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterAngry;
                    break;

                case 1:
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterIdle;
                    break;

                case 2 :
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterHappy;
                    break;

                case 3 :
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterInLove;
                    break;
            }
        }
        else
        {
            switch (currentOverridenEmotion)
            {
                case DialogueNodeData.EmotionOverride.Angry:
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterAngry;
                    break;
                case DialogueNodeData.EmotionOverride.Idle:
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterIdle;
                    break;
                case DialogueNodeData.EmotionOverride.Happy:
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterHappy;
                    break;
                case DialogueNodeData.EmotionOverride.Inlove:
                    rizzCharacter.GetComponent<Image>().sprite = rizzCharacterInLove;
                    break;
            }
        }
    }
}
