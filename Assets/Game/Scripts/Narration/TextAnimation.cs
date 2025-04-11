using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] float _durationFadeInText = 2.0f;
    [SerializeField] float _durationDescrambleForEachLetter = 1.0f;
    [SerializeField] float _waitDurationAfterDescramble = 5.0f;
    [SerializeField] float _durationMoveToLineForEachLetter = 3.0f;
    [SerializeField] float _intervalBetweenEachLetterMove = 0.3f;
    [SerializeField] float _durationFadeOutText = 2.0f;

    [Header("Display Settings")]
    [SerializeField] private string text;
    [SerializeField][Range(0.0f, 1.0f)] private float _textLerp;
    [SerializeField][Range(0.0f, 1.0f)] private float _intervalSquare;
    [SerializeField][Range(0.0f, 1.0f)] private float _intervalDetangled;
    [SerializeField] private Transform startPointSquare;
    [SerializeField] private Transform startPointDetangled;
    [SerializeField] private GameObject letterPrefab;
    private List<LetterInfo> spawnedText = new();

    // Start is called before the first frame update
    void Start()
    {
        SpawnText();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateTextPosition();
    }

    public Vector2 GetCharPosFromIndex(int index)
    {
        return (Vector2.Lerp(spawnedText[index].squarePosition, spawnedText[index].linePosition, _textLerp));
    }

    [Button("Spawn Text")]
    public void SpawnText()
    {
        ClearText();
        InitPositions();
        string tempNoSpace = text.Replace(" ", string.Empty);
        for (int i = 0; i < tempNoSpace.Length; i++)
        {
            char c = tempNoSpace[i];
            GameObject spawnedLetter = Instantiate(letterPrefab, GetCharPosFromIndex(i), Quaternion.identity, transform);
            spawnedText[i].index = i;
            spawnedText[i].text = spawnedLetter.GetComponent<Text>();
            spawnedText[i].text.material = Instantiate(spawnedText[i].text.material);
            spawnedText[i].text.text = c.ToString();
        }
    }

    [Button("Update Text Position")]
    public void UpdateTextPosition()
    {
        string tempNoSpace = text.Replace(" ", string.Empty);
        for (int i = 0; i < tempNoSpace.Length; i++)
        {
            spawnedText[i].text.transform.position = GetCharPosFromIndex(i);    
        }
    }

    public void InitPositions()
    {
        string tempNoSpace = text.Replace(" ", string.Empty);

        for (int i = 0; i < tempNoSpace.Length; i++) 
            spawnedText.Add(new LetterInfo());

        float squareSize = Mathf.Floor(Mathf.Sqrt(tempNoSpace.Length));
        for (int i = 0; i < tempNoSpace.Length; i++)
        {
            Vector2 newPos = new Vector2(startPointSquare.position.x + ((i % squareSize) * _intervalSquare),
                                        startPointSquare.position.y - (Mathf.Floor(i / squareSize) * _intervalSquare));
            spawnedText[i].squarePosition = newPos;
        }
        int noSpaceIndex = 0;
        for (int i = 0; i < text.Length; i++)
        {
            Vector2 newPos = new Vector2(startPointDetangled.position.x + (i * _intervalDetangled),
                                        startPointDetangled.position.y);
            if (text[i] != ' ')
            {
                spawnedText[noSpaceIndex++].linePosition = newPos;
            }
        }
    }


    [Button("Clear Text")]
    public void ClearText()
    {
        for (int i = transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        spawnedText.Clear();
    }

    public IEnumerator StartDisplayText()
    {
        List<LetterInfo> randomLetters = spawnedText.ToList();
        Shuffle(randomLetters);

        foreach (LetterInfo letter in randomLetters)
        {
            DOTween.To(() => letter.text.material.GetFloat("_Alpha"), x => letter.text.material.SetFloat("_Alpha", x), 1f, _durationFadeInText).SetEase(Ease.InCirc);
        }
        yield return new WaitForSeconds(_durationFadeInText);

        foreach (LetterInfo letter in randomLetters)
        {
            DOTween.To(() => letter.text.material.GetFloat("_Distort"), x => letter.text.material.SetFloat("_Distort", x), 0f, _durationDescrambleForEachLetter).SetEase(Ease.OutCirc);
            letter.text.transform.DOMove(letter.linePosition, _durationMoveToLineForEachLetter).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(_intervalBetweenEachLetterMove);
        }


        yield return new WaitForSeconds(_waitDurationAfterDescramble);

        foreach (LetterInfo letter in randomLetters)
        {
            DOTween.To(() => letter.text.material.GetFloat("_Alpha"), x => letter.text.material.SetFloat("_Alpha", x), .0f, _durationFadeOutText).SetEase(Ease.OutCirc);
        }
    }

    public void Init()
    {
        foreach (LetterInfo letter in spawnedText)
        {
            letter.text.material.SetFloat("_Alpha", 0.0f);
            letter.text.material.SetFloat("_Distort", 1.0f);
        }
        _textLerp = .0f;
    }

    public void Shuffle<T>(IList<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    class LetterInfo
    {
        public float index;
        public Text text;
        public Vector2 squarePosition;
        public Vector2 linePosition;

        public LetterInfo()
        {
            index = 0;
            text = null;
            squarePosition = new Vector2();
            linePosition = new Vector2();
        }
    }
}
