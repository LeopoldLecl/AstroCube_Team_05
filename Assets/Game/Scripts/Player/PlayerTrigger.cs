using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [Header("SpeedZone")]
    [SerializeField] float newSpeedMultiplyer = 0.5f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VictoryZone"))
        {
            EventManager.TriggerPlayerWin();
            Destroy(other.gameObject);
        }
        if (other.CompareTag("DeathZone"))
        {
            EventManager.Instance.TriggerPlayerLose();
        }

        if (other.gameObject.tag == "SlipperyZone")
        {
            GetComponent<PlayerMovement>().SetSlippingState(true);
        }

        if (other.gameObject.tag == "SpeedZone")
        {
            GetComponent<PlayerMovement>().SetSpeed(GetComponent<PlayerMovement>().defaultSpeed * newSpeedMultiplyer);
        }

        if (other.gameObject.tag == "ConveyerBelt")
        {
            Vector3 dir = other.GetComponent<ConveyerBeltManager>().direction;
            float speed = other.GetComponent<ConveyerBeltManager>().speed;
            GetComponent<PlayerMovement>().SetExternallyAppliedMovement(dir, speed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "SlipperyZone")
        {
            GetComponent<PlayerMovement>().SetSlippingState(false);
        }

        if (other.gameObject.tag == "SpeedZone")
        {
            GetComponent<PlayerMovement>().SetSpeedToDefault();
        }

        if (other.gameObject.tag == "ConveyerBelt")
        {
            GetComponent<PlayerMovement>().SetExternallyAppliedMovement(Vector3.zero);
        }
    }
}
