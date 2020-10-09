using UnityEngine;

public class Plane : MonoBehaviour
{
    private const int DIRECTION_RIGHT = 1;
    private const int DIRECTION_LEFT = -1;
    private const int DIRECTION_UP = -1;
    private const int DIRECTION_DOWN = 1;
    private const int DIRECTION_ZERO = 0;


    [SerializeField] GameObject m_PlaneLogicalObject;
    [SerializeField] GameObject m_PlaneGFXObject;

    private float m_Speed; // plane flying speed
    private float m_SpeedMin; // min speed value
    private float m_SpeedMax; // max speed value
    private float m_SpeedAcceleration; // acceleration speed value : positive value
    private float m_SpeedDeceleration; // deceleration speed value : negative value
    private float m_RotationMaxUpDownAngle; // max angle to rotate the plane up side or down side
    private float m_GfxMaxRotationAngle; // max angle to rotate the plane around the bank axis
    private Quaternion m_DefaultGfxQuaternion;

    void Start()
    {
        m_SpeedMin = 1;
        m_SpeedMax = 100;
        m_Speed = m_SpeedMin;
        m_SpeedAcceleration = 0.5f;
        m_SpeedDeceleration = -0.2f;
        m_RotationMaxUpDownAngle = 90.0f;
        m_GfxMaxRotationAngle = 45.0f;
        m_DefaultGfxQuaternion = m_PlaneGFXObject.transform.rotation;
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
            RotateHeading(DIRECTION_RIGHT);
            RotateBank(DIRECTION_RIGHT);
        }
        else if (hDirection < 0) // rotate left
        {
            RotateHeading(DIRECTION_LEFT);
            RotateBank(DIRECTION_LEFT);
        }
        if(hDirection == 0)
        {
            // do not rotate heading
            RotateBank(DIRECTION_ZERO);
        }
    }

    // rotate the plane vertically
    private void RotateVertically(float vDirection)
    {
        if (vDirection > 0) // rotate top
        {
            RotatePitch(DIRECTION_UP);
        }
        else if (vDirection < 0) // rotate bottom
        {
            RotatePitch(DIRECTION_DOWN);
        }
        // else vDirection == 0, do not rotate
    }

    // rotate the plane around the heading axis
    private void RotateHeading(int direction)
    {
        Quaternion rotation = Quaternion.AngleAxis(direction * Time.deltaTime * m_RotationMaxUpDownAngle, Vector3.up); // to rotation
        m_PlaneLogicalObject.transform.rotation *= Quaternion.Inverse(m_PlaneLogicalObject.transform.rotation)
                                                    * m_PlaneLogicalObject.transform.rotation
                                                    * rotation;
    }

    // rotate the plane around the pitch axis
    private void RotatePitch(int direction)
    {
        m_PlaneLogicalObject.transform.rotation = Quaternion.Slerp(
                                                        m_PlaneLogicalObject.transform.rotation,
                                                        Quaternion.AngleAxis(direction * m_RotationMaxUpDownAngle, Vector3.right),
                                                        Time.deltaTime
                                                      );
    }

    // rotate the plane around the bank axis
    private void RotateBank(int direction)
    {
        if (direction != 0)
        {
            
            m_PlaneGFXObject.transform.localRotation = Quaternion.Slerp(
                                                        m_PlaneGFXObject.transform.localRotation,
                                                        Quaternion.AngleAxis(-direction * m_GfxMaxRotationAngle, Vector3.forward),
                                                        Time.deltaTime
                                                    );
        }
        else
        {
            m_PlaneGFXObject.transform.rotation = Quaternion.RotateTowards(
                                                    m_PlaneGFXObject.transform.rotation,
                                                    m_PlaneLogicalObject.transform.rotation,
                                                    Time.deltaTime * m_GfxMaxRotationAngle
                                                    );
        }
    }
}
