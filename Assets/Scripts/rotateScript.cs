using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateScript : MonoBehaviour
{
    // Vitesse de rotation (modifiable depuis l'inspecteur)
    public float rotationSpeed = 50f;

    // Axe de rotation aléatoire
    private Vector3 rotationAxis;

    // Start est appelée avant la première frame
    void Start()
    {
        // Génère un axe de rotation aléatoire (vecteur unitaire)
        rotationAxis = Random.onUnitSphere;
    }

    // Update est appelée une fois par frame
    void Update()
    {
        // Fait tourner l'objet autour de l'axe aléatoire
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
