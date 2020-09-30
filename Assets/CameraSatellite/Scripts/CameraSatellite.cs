using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSatellite : MonoBehaviour
{
    private const int FREE_ROTATION = 0;
    private const int LEFT_STEP_ROTATION = -1;
    private const int RIGHT_STEP_ROTATION = 1;

    private Camera Cam; // camera which the script is attached to
    private GameObject Pivot; // parent gameobject which will be focused by the camera
    private Vector3 CamLocalRotation; // camera local rotation vector
    private float RotationStep; // step bewteen to angles in degree

    private float Rho; // current distance value bewteen the camera and the Pivot gameobject
    private float RhoMin; // minimum distance value bewteen the camera and the Pivot gameobject
    private float RhoMax; // maximum distance value bewteen the camera and the Pivot gameobject
    private float RhoStep; // step bewteen two zoom values
    private float Theta; // current horizontal degree value to translate
    private float Phi; // current vertical degree value to translate
    private float PhiMin; // minimum vertical degree value to translate
    private float PhiMax; // maximum vertical degree value to translate

    void Start()
    {
        Cam = GetComponent<Camera>();
        Pivot = transform.parent.gameObject;
        CamLocalRotation = new Vector3(0, 0, 0);
        RotationStep = 30f;
        Rho = 5f;
        RhoMin = 1.5f;
        RhoMax = 10f;
        RhoStep = 0.5f;
        PhiMin = 0f;
        PhiMax = 90f;
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) // if mouse right button is pressed down
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                RotateCameraAroundPivot(LEFT_STEP_ROTATION);
            }
            else if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                RotateCameraAroundPivot(RIGHT_STEP_ROTATION);
            }
            else
            {
                RotateCameraAroundPivot(FREE_ROTATION);
            }
        }

        if (Input.GetKey(KeyCode.Space)) // reset rotation
        {
            CameraSatellitePivot CSP = Pivot.GetComponent<CameraSatellitePivot>();
            Pivot.transform.rotation = CSP.Target.transform.rotation; // reset pivot transform rotation to its target transform rotation
            CamLocalRotation = new Vector3(0, 0, 0);
        }
    }

    void RotateCameraAroundPivot(int RotationMode)
    {
        if(RotationMode == RIGHT_STEP_ROTATION)
        {
            float k = CamLocalRotation.x;
            if (k < 0) k *= -1; // get positive value
            k = k - Mathf.Floor(k / RotationStep) * RotationStep; // modulo
            if (CamLocalRotation.x <= 0) k = RotationStep - k;
            if (k == 0) k = RotationStep;
            CamLocalRotation.x -= k;
        }
        else if(RotationMode == LEFT_STEP_ROTATION)
        {
            float k = CamLocalRotation.x;
            if (k < 0) k *= -1; // get positive value
            k = k - Mathf.Floor(k / RotationStep) * RotationStep; // modulo
            if (CamLocalRotation.x >= 0) k = RotationStep - k;
            if (k == 0) k = RotationStep;
            CamLocalRotation.x += k;
        }
        else // RotationMode == FREE_ROTATION
        {
            Theta = Input.GetAxis("Mouse X");
            CamLocalRotation.x += Theta;
        }
        Phi = Input.GetAxis("Mouse Y");
        CamLocalRotation.y = Mathf.Clamp(CamLocalRotation.y + Phi, PhiMin, PhiMax);
        Debug.Log("CamLocalRotation.x = " + CamLocalRotation.x);
        Debug.Log("CamLocalRotation.y = " + CamLocalRotation.y);
        Quaternion Rotation = Quaternion.Euler(CamLocalRotation.y, CamLocalRotation.x, 0);
        Pivot.transform.rotation = Quaternion.Lerp(Pivot.transform.rotation, Rotation, Time.deltaTime);

        float ZoomAxis = Input.GetAxis("Mouse ScrollWheel");
        if(ZoomAxis > 0)
        {
            Rho = Mathf.Clamp(Rho + RhoStep, RhoMin, RhoMax); // dezoom
        }
        else if(ZoomAxis < 0)
        {
            Rho = Mathf.Clamp(Rho - RhoStep, RhoMin, RhoMax); // zoom
        }
        
        Cam.transform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(Cam.transform.localPosition.z, Rho * -1f, Time.deltaTime));
    }
}