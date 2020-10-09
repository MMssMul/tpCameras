using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform m_CameraReachPosition;
    [SerializeField] Transform m_CameraLookAtPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ReachPosition();
        LookAtPosition();
    }

    private void ReachPosition()
    {
        transform.position = Vector3.Lerp(transform.position, m_CameraReachPosition.position, Time.deltaTime);
    }

    private void LookAtPosition()
    {
        transform.LookAt(m_CameraLookAtPosition);
    }
}
