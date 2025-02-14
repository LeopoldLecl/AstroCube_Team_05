using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DoMoves : MonoBehaviour
{

    [SerializeField] Transform middle;

    [SerializeField] Transform YUp;
    [SerializeField] Transform YDown;

    [SerializeField] Transform ZFront;
    [SerializeField] Transform ZBack;

    [SerializeField] Transform XFront;
    [SerializeField] Transform XBack;

    [SerializeField] List<GameObject> Axis = new List<GameObject>();
    [SerializeField] List<GameObject> allBlocks = new List<GameObject>();


    public bool doScramble = true;

    private bool _isRotating = false;
    private void Start()
    {
        allBlocks = GameObject.FindGameObjectsWithTag("Movable").ToList();
        StartCoroutine(Scramble());
    }
    IEnumerator Scramble()
    {
        while (doScramble)
        {
            if (!_isRotating)
            {
                _isRotating = true;
                StartCoroutine(RotateAngle(Axis[Random.Range(0, Axis.Count - 1)].transform, Random.Range(0, 5) % 2 == 0,1));
            }
            yield return null;
        }
    }
    IEnumerator RotateAngle(Transform Axis, bool Reverse, float duration = 0.5f)
    {
        print(Axis.name + ' ' + Reverse);
        _isRotating = true;
        foreach (var block in allBlocks)
        {
            Vector3 blockPos = block.transform.position;

            if ((Axis.position.x > 0.5f && blockPos.x > 0.5f) ||
                (Axis.position.y > 0.5f && blockPos.y > 0.5f) ||
                (Axis.position.z > 0.5f && blockPos.z > 0.5f) ||
                (Axis.position.x < -0.5f && blockPos.x < -0.5f) ||
                (Axis.position.y < -0.5f && blockPos.y < -0.5f) ||
                (Axis.position.z < -0.5f && blockPos.z < -0.5f))
            {
                block.transform.SetParent(Axis);
            }
        }

        Vector3 angles = Axis.eulerAngles;
        float elapsedTime = 0;

        int direction = Reverse ? 1 : -1;
        Vector3 rotationAxis = Vector3.zero;

        if (Axis.transform.position.x > 0.5 || Axis.transform.position.x < -0.5)
            rotationAxis = Vector3.right;
        else if (Axis.transform.position.y > 0.5 || Axis.transform.position.y < -0.5)
            rotationAxis = Vector3.up;
        else if (Axis.transform.position.z > 0.5 || Axis.transform.position.z < -0.5)
            rotationAxis = Vector3.forward;

        Quaternion startRotation = Axis.rotation;
        Quaternion targetRotation = startRotation * Quaternion.AngleAxis(direction * 90, rotationAxis);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Axis.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);
            yield return null;
        }

        Axis.rotation = targetRotation;

        foreach (GameObject t in allBlocks)
        {
            if (t.transform.position.x > 0) t.transform.parent = null;
        }
        _isRotating = false;
    }

    IEnumerator RotateXFront()
    {
        for (int i = 0; i < allBlocks.Count; i++)
        {
            if (allBlocks[i].transform.position.x > 0.5f)
            {
                allBlocks[i].transform.SetParent(XFront);
            }
        }

        int RotateValue = 0;
        while (RotateValue != 90)
        {
            RotateValue++;
            XFront.eulerAngles = new Vector3(RotateValue, 0, 0);
            yield return null;
        }

        foreach (GameObject t in allBlocks)
        {
            if (t.transform.position.x > 0) t.transform.parent = null;
        }

        StartCoroutine(RotateYUp());
    }
    IEnumerator RotateYUp()
    {
        for (int i = 0; i < allBlocks.Count; i++)
        {
            if (allBlocks[i].transform.position.y > 0.5f)
            {
                allBlocks[i].transform.SetParent(YUp);
            }
        }

        int RotateValue = 0;
        while (RotateValue != 90)
        {
            RotateValue++;
            YUp.eulerAngles = new Vector3(0, RotateValue, 0);
            yield return null;
        }

        foreach (GameObject t in allBlocks)
        {
            if (t.transform.position.x > 0) t.transform.parent = null;
        }

        StartCoroutine(RotateZFront());
    }
    IEnumerator RotateZFront()
    {
        for (int i = 0; i < allBlocks.Count; i++)
        {
            if (allBlocks[i].transform.position.z > 0.5f)
            {
                allBlocks[i].transform.SetParent(ZFront);
            }
        }

        int RotateValue = 0;
        while (RotateValue != 90)
        {
            RotateValue++;
            ZFront.eulerAngles = new Vector3(0, 0, RotateValue);
            yield return null;
        }

        foreach (GameObject t in allBlocks)
        {
            if (t.transform.position.x > 0) t.transform.parent = null;
        }

        StartCoroutine(RotateXFront());
    }


}
