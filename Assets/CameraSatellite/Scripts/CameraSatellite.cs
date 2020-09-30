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
                RotateCameraPivot(LEFT_STEP_ROTATION);
            }
            else if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                RotateCameraPivot(RIGHT_STEP_ROTATION);
            }
            else
            {
                RotateCameraPivot(FREE_ROTATION);
            }
        }

        if (Input.GetKey(KeyCode.Space)) // reset rotation
        {
            CameraSatellitePivot CSP = Pivot.GetComponent<CameraSatellitePivot>();
            Pivot.transform.rotation = CSP.Target.transform.rotation; // reset pivot transform rotation to its target transform rotation
            CamLocalRotation = new Vector3(0, 0, 0);
        }
    }

    void RotateCameraPivot(int RotationMode)
    {
        SetTheta(RotationMode);
        SetPhi();
        SetRho();

        CamLocalRotation.x += Theta;
        CamLocalRotation.y = Mathf.Clamp(CamLocalRotation.y + Phi, PhiMin, PhiMax);

        Quaternion Rotation = Quaternion.Euler(CamLocalRotation.y, CamLocalRotation.x, 0); // define the quaternion rotation for the pivot

        Pivot.transform.rotation = Quaternion.Lerp(Pivot.transform.rotation, Rotation, Time.deltaTime); // rotate pivot, and so the camera attached will follow

        Cam.transform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(Cam.transform.localPosition.z, Rho * -1f, Time.deltaTime)); // zoom or unzoom camera using local position z axis to move

        Debug.Log("CamLocalRotation.x = " + CamLocalRotation.x);
        Debug.Log("CamLocalRotation.y = " + CamLocalRotation.y);
    }

    private void SetPhi()
    {
        Phi = Input.GetAxis("Mouse Y");
    }

    private void SetRho()
    {
        float ZoomAxis = Input.GetAxis("Mouse ScrollWheel");
        if (ZoomAxis > 0)
        {
            Rho = Mathf.Clamp(Rho + RhoStep, RhoMin, RhoMax); // dezoom
        }
        else if (ZoomAxis < 0)
        {
            Rho = Mathf.Clamp(Rho - RhoStep, RhoMin, RhoMax); // zoom
        }
    }

    private void SetTheta(int RotationMode)
    {
        if (RotationMode == RIGHT_STEP_ROTATION)
        {
            Theta = CamLocalRotation.x;
            if (Theta < 0) Theta *= -1; // get positive value
            Theta = Theta - Mathf.Floor(Theta / RotationStep) * RotationStep; // modulo
            if (CamLocalRotation.x <= 0) Theta = RotationStep - Theta;
            if (Theta == 0) Theta = RotationStep;
            Theta *= -1;
        }
        else if (RotationMode == LEFT_STEP_ROTATION)
        {
            Theta = CamLocalRotation.x;
            if (Theta < 0) Theta *= -1; // get positive value
            Theta = Theta - Mathf.Floor(Theta / RotationStep) * RotationStep; // modulo
            if (CamLocalRotation.x >= 0) Theta = RotationStep - Theta;
            if (Theta == 0) Theta = RotationStep;
        }
        else // RotationMode == FREE_ROTATION
        {
            Theta = Input.GetAxis("Mouse X");
        }
    }
}