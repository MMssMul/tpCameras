using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    [SerializeField] GameObject m_PlaneLogicalObject;
    [SerializeField] GameObject m_PlaneGFXObject;

    private float m_Speed; // plane flying speed
    private float m_SpeedMin; // min speed value
    private float m_SpeedMax; // max speed value
    private float m_SpeedAcceleration; // acceleration speed value : positive value
    private float m_SpeedDeceleration; // deceleration speed value : negative value

    void Start()
    {
        m_SpeedMin = 1;
        m_SpeedMax = 100;
        m_Speed = m_SpeedMin;
        m_SpeedAcceleration = 0.5f;
        m_SpeedDeceleration = -0.2f;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Accelerate();
        }
        else
        {
            Decelerate();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    // increase the plane speed
    private void Accelerate()
    {
        Debug.Log("Accelerate");
        m_Speed = Mathf.Clamp(m_Speed + m_SpeedAcceleration, m_SpeedMin, m_SpeedMax);
    }

    // decrease the plane speed
    private void Decelerate()
    {
        Debug.Log("Decelerate");
        m_Speed = Mathf.Clamp(m_Speed + m_SpeedDeceleration, m_SpeedMin, m_SpeedMax);
    }

    // move the plane over the world
    private void Move()
    {
        Debug.Log("Speed = " + m_Speed);
        Debug.Log("m_PlaneLogicalObject.transform.forward = " + m_PlaneLogicalObject.transform.forward);
        Debug.Log("m_PlaneLogicalObject.transform.forward * m_Speed = " + m_PlaneLogicalObject.transform.forward * m_Speed);
        //m_RigidBody.velocity = m_PlaneLogicalObject.transform.forward * m_Speed;
        //transform.position = Vector3.Lerp(transform.position, transform.position + m_PlaneLogicalObject.transform.forward * m_Speed * Time.deltaTime);
        //transform.position += Mathf.Lerp(m_PlaneLogicalObject.transform.forward * m_Speed * Time.deltaTime);
        transform.position += m_PlaneLogicalObject.transform.forward * m_Speed * Time.deltaTime;
    }
}
