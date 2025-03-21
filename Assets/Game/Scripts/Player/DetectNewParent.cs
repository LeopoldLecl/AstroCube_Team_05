using System.Collections;
using UnityEngine;

public class DetectNewParent : MonoBehaviour
{
    [SerializeField] LayerMask _detectableLayer;
    Vector3 currentRotationDir;

    [SerializeField] SelectionCube OldTilePlayerPos;
    Transform OldTilePlayerPosTransform;


    [SerializeField] private bool _doGroundRotation;

    private bool _doGravityRotation;




    public bool DoGravityRotation { get => _doGravityRotation; set => _doGravityRotation = value; }
    public bool DoGroundRotation { get => _doGroundRotation; set => _doGroundRotation = value; }

    private void Awake()
    {
        EventManager.OnPlayerReset += DisableParentChangerfor;
        EventManager.OnPlayerResetOnce += DisableParentChangerfor;

        currentRotationDir = transform.up;
    }
    private void OnDisable()
    {
        EventManager.OnPlayerReset -= DisableParentChangerfor;
        EventManager.OnPlayerResetOnce -= DisableParentChangerfor;
    }
    private void Update()
    {
        if (_doGroundRotation)
        {
            RaycastHit _raycastInfo;
            if (Physics.Raycast(transform.position, -transform.up, out _raycastInfo, 10, _detectableLayer))
            {
                OldTilePlayerPosTransform = _raycastInfo.collider.transform;

                //if (OldTilePlayerPos) OldTilePlayerPos.SetIsTileLock(false);

                OldTilePlayerPos = _raycastInfo.transform.GetComponentInParent<SelectionCube>();
                if (OldTilePlayerPos)
                {
                    transform.SetParent(OldTilePlayerPos.transform, true);
                }
                //if(!OldTilePlayerPos) _raycastInfo.transform.parent.parent.TryGetComponent(out OldTilePlayerPos);

            }
        }
        else if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }


    }

    public void DisableParentChangerfor(float duration)
    {
        StartCoroutine(DisableParentChanger(duration));
    }
    IEnumerator DisableParentChanger(float duration)
    {
        DoGroundRotation = false;
        yield return new WaitForSeconds(duration);
        DoGroundRotation = true;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!_doGravityRotation) return;

        var h = hit.collider.transform;
        var dif = transform.up - (-h.right);
        if (Mathf.Abs(dif.magnitude) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, -h.right) * transform.rotation, 1f);
        }
        transform.SetParent(hit.collider.gameObject.transform);
    }
}
