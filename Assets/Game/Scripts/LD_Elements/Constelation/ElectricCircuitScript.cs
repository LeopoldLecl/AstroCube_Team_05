using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricCircuitScript : MonoBehaviour
{
    [SerializeField] private GameObject previousElectricPoint;
    [SerializeField] private List<GameObject> Editor_CircuitVisual;
    

    [SerializeField] private float lineStartWidth = 0.05f; 
    [SerializeField] private float lineEndWidth = 0.05f;   

    private LineRenderer lineRenderer;
    private ParticleSystem electricParticles;

    private void Awake()
    {
        foreach (GameObject electricPoint in Editor_CircuitVisual)
        {
            electricPoint.SetActive(false);
        }

        // Ajouter un LineRenderer pour dessiner la connexion
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineStartWidth;
        lineRenderer.endWidth = lineEndWidth;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.cyan;
        lineRenderer.enabled = false;

        // Ajouter un Particle System pour les étincelles électriques
        GameObject particleObject = new GameObject("ElectricParticles");
        particleObject.transform.SetParent(transform);
        electricParticles = particleObject.AddComponent<ParticleSystem>();

        var main = electricParticles.main;
        main.startColor = Color.blue;
        main.startSpeed = 5f;
        main.startSize = 0.1f;
        main.loop = true;
        main.playOnAwake = false;

        var shape = electricParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var emission = electricParticles.emission;
        emission.rateOverTime = 50f;

        var noise = electricParticles.noise;
        noise.enabled = true;
        noise.strength = 0.5f;

        electricParticles.Stop();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ElectricPoint"))
        {
            if (previousElectricPoint != null && other.gameObject == previousElectricPoint)
            {
                Debug.Log("Connexion avec le previous");
                DrawElectricBeam();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ElectricPoint") && previousElectricPoint != null && other.gameObject == previousElectricPoint)
        {
            lineRenderer.enabled = false;
            electricParticles.Stop();
        }
    }

    private void DrawElectricBeam()
    {
        if (previousElectricPoint != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.startWidth = lineStartWidth;  // Mettre à jour la largeur
            lineRenderer.endWidth = lineEndWidth;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, previousElectricPoint.transform.position);

            // Activer les particules et les déplacer au milieu de la ligne
            electricParticles.transform.position = (transform.position + previousElectricPoint.transform.position) / 2;
            electricParticles.Play();
        }
    }
}
