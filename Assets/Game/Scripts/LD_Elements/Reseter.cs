using System.Collections;
using UnityEngine;

public class Reseter : MonoBehaviour
{
    Pose _startPos;
    Pose _poseOnReset;

    Rigidbody _rb;

    Pose _positionOnLastRotation;

    //CONTROLLER AND CLOSE INPUTS

    void Awake()
    {
        _startPos = new Pose();
        transform.GetPositionAndRotation(out _startPos.position, out _startPos.rotation);
        TryGetComponent(out _rb);


        //need
        EventManager.OnPlayerReset += OnReset;
        EventManager.OnPlayerResetOnce += ResetOnce;
        
        EventManager.OnCubeRotated += SavePose;

    }
    private void OnDisable()
    {
        EventManager.OnPlayerReset -= OnReset;
        EventManager.OnPlayerResetOnce -= ResetOnce;

        EventManager.OnCubeRotated -= SavePose;


    }

    void SavePose()
    {
        _positionOnLastRotation = new Pose();
        transform.GetPositionAndRotation(out _positionOnLastRotation.position, out _positionOnLastRotation.rotation);
    }
    private void ResetOnce(float time)
    {
        _poseOnReset = new Pose();
        transform.GetPositionAndRotation(out _poseOnReset.position, out _poseOnReset.rotation);
        StartCoroutine(ResetOneMove(time));
    }

    IEnumerator ResetOneMove(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(_poseOnReset.position, _positionOnLastRotation.position, elapsedTime / duration);
            transform.rotation = Quaternion.Lerp(_poseOnReset.rotation, _positionOnLastRotation.rotation, elapsedTime / duration);
            yield return null;
        }
        transform.position = _positionOnLastRotation.position;
        transform.rotation = _positionOnLastRotation.rotation;
    }
    void OnReset(float duration)
    {
        if (_rb)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        _poseOnReset = new Pose();
        transform.GetPositionAndRotation(out _poseOnReset.position, out _poseOnReset.rotation);
        StartCoroutine(Reset(duration));
    }

    IEnumerator Reset(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(_poseOnReset.position, _startPos.position, elapsedTime / duration);
            transform.rotation = Quaternion.Lerp(_poseOnReset.rotation, _startPos.rotation, elapsedTime / duration);
            yield return null;
        }
        transform.position = _startPos.position;
        transform.rotation = _startPos.rotation;
    }

}
