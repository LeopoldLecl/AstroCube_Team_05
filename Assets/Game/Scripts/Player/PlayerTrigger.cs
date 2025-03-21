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
            EventManager.TriggerPlayerLose();
        }

        if (other.gameObject.tag == "SlipperyZone")
        {
            GetComponent<PlayerMovement>().SetSlippingState(true);
        }

        if (other.gameObject.tag == "SpeedZone")
        {
            GetComponent<PlayerMovement>().SetSpeed(GetComponent<PlayerMovement>().defaultSpeed * newSpeedMultiplyer);
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
    }
}
