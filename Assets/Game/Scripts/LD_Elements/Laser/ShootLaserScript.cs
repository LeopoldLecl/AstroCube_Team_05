using UnityEngine;

[ExecuteAlways]
public class ShootLaserScript : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private float laserMaxDistance = 30f;

    private LaserBeam _beam;
    private bool _isLaserActive = false;

    private void OnEnable()
    {
        EventManager.OnButtonPressed += ActivateLaser;
        EventManager.OnButtonReleased += DeactivateLaser;
        _isLaserActive = Application.isPlaying ? false : true;
    }

    private void OnDisable()
    {
        EventManager.OnButtonPressed -= ActivateLaser;
        EventManager.OnButtonReleased -= DeactivateLaser;
    }

    private void Update()
    {
        if (_isLaserActive)
        {
            DestroyImmediate(GameObject.Find("Laser Beam")); // Use DestroyImmediate in editor mode
            _beam = new LaserBeam(transform.position, transform.right, _material, laserMaxDistance);
        }
    }

    private void ActivateLaser()
    {
        _isLaserActive = true;
    }

    private void DeactivateLaser()
    {
        _isLaserActive = false;
        DestroyImmediate(GameObject.Find("Laser Beam"));
    }
}
