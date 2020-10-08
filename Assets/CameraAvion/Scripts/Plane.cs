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
    private float m_RotationMaxUpDown; // max angle to rotate the plane up side or down side

    void Start()
    {
        m_SpeedMin = 1;
        m_SpeedMax = 100;
        m_Speed = m_SpeedMin;
        m_SpeedAcceleration = 0.5f;
        m_SpeedDeceleration = -0.2f;
        m_RotationMaxUpDown = 90.0f;
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

        int leftArrow = Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
        int rightArrow = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
        float hInput = Input.GetAxis("Horizontal") - leftArrow + rightArrow; // intervalle [-1;1]
        RotateHorizontally(hInput);

        int downArrow = Input.GetKey(KeyCode.DownArrow) ? 1 : 0;
        int upArrow = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
        float vInput = Input.GetAxis("Vertical") - downArrow + upArrow; // intervalle [-1;1]
        RotateVertically(vInput);
    }

    // increase the plane speed
    private void Accelerate()
    {
        m_Speed = Mathf.Clamp(m_Speed + m_SpeedAcceleration, m_SpeedMin, m_SpeedMax);
    }

    // decrease the plane speed
    private void Decelerate()
    {
        m_Speed = Mathf.Clamp(m_Speed + m_SpeedDeceleration, m_SpeedMin, m_SpeedMax);
    }

    // move the plane over the world
    private void Move()
    {
        transform.position += m_PlaneLogicalObject.transform.forward * m_Speed * Time.deltaTime;
    }

    // rotate the plane horizontally
    private void RotateHorizontally(float hDirection)
    {
        if(hDirection > 0) // rotate right
        {
            /*
            m_PlaneLogicalObject.transform.rotation = Quaternion.Slerp(
                                                        m_PlaneLogicalObject.transform.rotation,
                                                        Quaternion.RotateTowards(
                                                            m_PlaneLogicalObject.transform.rotation,
                                                           Quaternion.Inverse(m_PlaneLogicalObject.transform.rotation),
                                                           10
                                                        ),
                                                        Time.deltaTime
                                                      );
            */
            //m_PlaneLogicalObject.transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);

            RotateHeading(1);
        }
        else if (hDirection < 0) // rotate left
        {
            /*
            m_PlaneLogicalObject.transform.rotation = Quaternion.Slerp(
                                                        m_PlaneLogicalObject.transform.rotation,
                                                        Quaternion.AngleAxis(-180, Vector3.up),
                                                        Time.deltaTime
                                                      );
            */
            RotateHeading(1);
        }
        // else hDirection == 0, do not rotate
    }

    // rotate the plane vertically
    private void RotateVertically(float vDirection)
    {
        if (vDirection > 0) // rotate top
        {
            RotatePitch(-1);
        }
        else if (vDirection < 0) // rotate bottom
        {
            RotatePitch(1);
        }
        // else vDirection == 0, do not rotate
    }

    // rotate the plane around the heading axis
    private void RotateHeading(float direction)
    {
        Quaternion rotation = Quaternion.AngleAxis(direction * Time.deltaTime * m_RotationMaxUpDown, m_PlaneLogicalObject.transform.up); // to rotation
        m_PlaneLogicalObject.transform.rotation *= Quaternion.Inverse(m_PlaneLogicalObject.transform.rotation)
                                                    * m_PlaneLogicalObject.transform.rotation
                                                    * rotation;
        
    }

    // rotate the plane around the pitch axis
    private void RotatePitch(float direction)
    {
        m_PlaneLogicalObject.transform.rotation = Quaternion.Slerp(
                                                        m_PlaneLogicalObject.transform.rotation,
                                                        Quaternion.AngleAxis(direction * m_RotationMaxUpDown, Vector3.right),
                                                        Time.deltaTime
                                                      );
    }
}
