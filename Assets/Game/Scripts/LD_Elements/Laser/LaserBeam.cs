using System.Collections.Generic;
using UnityEngine;

public class LaserBeam
{
    Vector3 _pos, _dir;
    GameObject _laserObj;
    LineRenderer _laser;
    List<Vector3> _laserIndices = new List<Vector3>();
    float _maxDistance;

    public LaserBeam(Vector3 pos, Vector3 dir, Material material, float maxDistance)
    {
        _laserObj = new GameObject("Laser Beam");
        _pos = pos;
        _dir = dir;
        _maxDistance = maxDistance;

        _laser = _laserObj.AddComponent<LineRenderer>();
        _laser.startWidth = 0.1f;
        _laser.endWidth = 0.1f;
        _laser.material = material;
        _laser.startColor = Color.red;
        _laser.endColor = Color.red;

        CastRay(pos, dir);
    }

    void CastRay(Vector3 pos, Vector3 dir)
    {
        _laserIndices.Add(pos);

        Ray ray = new Ray(pos, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _maxDistance, ~0))
        {
            CheckHit(hit, dir);
        }
        else
        {
            _laserIndices.Add(ray.GetPoint(_maxDistance));
            UpdateLaser();
        }
    }

    void UpdateLaser()
    {
        _laser.positionCount = _laserIndices.Count;

        for (int i = 0; i < _laserIndices.Count; i++)
        {
            _laser.SetPosition(i, _laserIndices[i]);
        }
    }

    void CheckHit(RaycastHit hitInfo, Vector3 direction)
    {
        if (hitInfo.collider.CompareTag("Aim"))
        {
            EventManager.TriggerPlayerWin();
        }

        if (hitInfo.collider.CompareTag("Mirror"))
        {
            Vector3 pos = hitInfo.point;
            Vector3 dir = Vector3.Reflect(direction, hitInfo.normal);
            CastRay(pos, dir);
        }
        else
        {
            _laserIndices.Add(hitInfo.point);
            UpdateLaser();
        }
    }
}
