using UnityEngine;

public class SkyboxRotationController : MonoBehaviour
{
    public Material skyboxMaterial;
    public float rotationSpeed = 20f;
    private float currentRotation = 0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.R)) // Appuie sur "R" pour tourner la section
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            skyboxMaterial.SetFloat("_Rotation", currentRotation);
        }
    }
}
