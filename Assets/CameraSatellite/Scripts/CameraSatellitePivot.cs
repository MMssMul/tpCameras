using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSatellitePivot : MonoBehaviour
{
    [SerializeField] public GameObject Target; // can be set/changed in the inspector while the game in running
    private GameObject _Target;

    void Start()
    {
        SetTarget();
    }

    void Update()
    {
        if (!_Target.Equals(Target)) // check if the target gameobject has changed
        {
            SetTarget();
        }
    }

    private void SetTarget()
    {
        _Target = Target;
        transform.position = _Target.transform.position; // set this gameobject position to be the same as the target position
        transform.rotation = _Target.transform.rotation; // set this gameobject rotation to be the same as the target rotation
    }
}
