using RedBlueGames.Tools.TextTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TextTyper textType= GetComponent<TextTyper>();
        textType.TypeText("Hi my name is Maxens Thiam", 0.1f);
    }


}
