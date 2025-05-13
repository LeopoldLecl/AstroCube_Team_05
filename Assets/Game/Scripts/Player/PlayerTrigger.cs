using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PlayerTrigger : MonoBehaviour
{
    [Header("SpeedZone")]
    [SerializeField] float newSpeedMultiplyer = 0.5f;

    [Header("Portal")]
    [SerializeField] AnimationCurve curveFOV;
    [SerializeField] AnimationCurve curveAberration;

    [SerializeField] VolumeProfile vol;


    PlayerMovement _playerMovement;
    CharacterController _characterController;

    FloatingZone _flotingZone;
    private void Start()
    {
        if (!vol) vol = GameObject.FindGameObjectWithTag("GlobalVol")?.GetComponent<VolumeProfile>();
        if (vol)
        {
            if (vol.TryGet<ChromaticAberration>(out var ca))
                ca.intensity.Override(.1f);
        }
        _playerMovement = GetComponent<PlayerMovement>();
        _characterController = GetComponent<CharacterController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VictoryZone"))
        {
            EventManager.TriggerPlayerWin();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("DeathZone"))
        {
            EventManager.Instance.TriggerPlayerLose();
        }
        else if (other.CompareTag("SlipperyZone"))
        {
            _playerMovement.SetSlippingState(true);
        }
        else if (other.CompareTag("SpeedZone"))
        {
            _playerMovement.SetSpeed(_playerMovement.defaultSpeed * newSpeedMultiplyer);
        }
        else if (other.CompareTag("GravityZone"))
        {
            _playerMovement.HasGravity = false;
            _flotingZone = other.transform.GetComponent<FloatingZone>();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (_flotingZone && other.CompareTag("GravityZone"))
        {
            _characterController.Move(Vector3.up * _flotingZone.GravityForce * Time.deltaTime);

        }

        if (other.gameObject.tag == "ConveyerBelt")
        {
            Vector3 dir = other.GetComponent<ConveyerBeltManager>().direction;
            float speed = other.GetComponent<ConveyerBeltManager>().speed;
            GetComponent<PlayerMovement>().SetExternallyAppliedMovement(dir, speed);
        }

        if (other.CompareTag("Portal"))
        {
            float cameraFOV = Mathf.Lerp(15, GameManager.Instance.CustomSettings.customFov, curveFOV.Evaluate(Vector3.Distance(this.transform.position, other.transform.position) / 4f));
            float cameraOverlayFOV = Mathf.Lerp(15, 43, curveFOV.Evaluate(Vector3.Distance(this.transform.position, other.transform.position) / 4f));

            float chromaticAbberation = Mathf.Lerp(.1f, 50, curveAberration.Evaluate(Vector3.Distance(this.transform.position, other.transform.position) / 4f));

            if (vol)
            {
                if(vol.TryGet<ChromaticAberration>(out var ca))
                ca.intensity.Override(chromaticAbberation);
            }


            Camera.allCameras[0].fieldOfView = cameraFOV;
            Camera.allCameras[1].fieldOfView = cameraOverlayFOV;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SlipperyZone"))
        {
            _playerMovement.SetSlippingState(false);
        }
        else if (other.CompareTag("SpeedZone"))
        {
            _playerMovement.SetSpeedToDefault();
        }
        else if (other.CompareTag("GravityZone"))
        {
            _flotingZone = null;
            _playerMovement.HasGravity = true;
        }

        if (other.gameObject.tag == "ConveyerBelt")
        {
            GetComponent<PlayerMovement>().SetExternallyAppliedMovement(Vector3.zero);
        }
        if (other.CompareTag("Portal"))
        {
            Camera.main.fieldOfView = GameManager.Instance.CustomSettings.customFov;
            Camera.allCameras[1].fieldOfView = 43;

            if (vol)
            {
                if (vol.TryGet<ChromaticAberration>(out var ca))
                    ca.intensity.Override(.1f);
            }
        }
    }
}
