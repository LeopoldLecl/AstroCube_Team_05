using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RailFollowingObject : MonoBehaviour
{
    [SerializeField] private Rail rail;
    [SerializeField][Range(0f,10f)] private float _mass;
    private float _velocity;
    private Vector3 _target;
    private int _lastNodeIndex = 0;

    private void Start()
    {
        _velocity = 0f;
    }

    private void Update()
    {
        transform.position = GetPositionOnRail();
    }

    public Vector3 GetPositionOnRail()
    {
        _target = transform.position + (Vector3.down  * _velocity);
        Vector3 result = rail.GetClosestPointOnRail(_target);

        int index = rail.IsOnNode(transform.position);
        if (Vector3.Distance(result, transform.position) <= Mathf.Epsilon)
        {
            _velocity = 0.0f;
        }
        if (index != _lastNodeIndex && index != -1)
        {
            _lastNodeIndex = index;
            _velocity = 0.0f;
        }
        _velocity += Time.deltaTime * _mass;
        return result;
    }
}
