using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBarycentrique : MonoBehaviour
{
    private GameObject[] Elements; // gameobjects considered by the camera

    // contains the sum of the inverse distances to other gameobjects in "Elements",
    // the index where each value is stored correspond to the gameobject at the same index in "Elements"
    private float[] elementsSumInverseDistancesToOtherObjects;

    private float rangeSafeZone = 10; // distance from the camera position

    private Camera Cam; // camera component

    // to see the more about the camera behaviour
    public GameObject BarycentrePrefab; // barycentre prefab
    private GameObject BarycentreGO; // barycentre instance
    public Material DefaultElementMaterial; // default barycentre material
    public Material SafeZoneElementMaterial; // barycentre material in safe zone
    // elements bounds
    private GameObject DebugElementsLimit1;
    private GameObject DebugElementsLimit2;
    private GameObject DebugElementsLimit3;
    private GameObject DebugElementsLimit4;
    // zoom bounds
    private GameObject DebugZoomLimit1;
    private GameObject DebugZoomLimit2;
    private GameObject DebugZoomLimit3;
    private GameObject DebugZoomLimit4;
    // dezoom bounds
    private GameObject DebugDezoomLimit5;
    private GameObject DebugDezoomLimit6;
    private GameObject DebugDezoomLimit7;
    private GameObject DebugDezoomLimit8;
    
    void Start()
    {
        Cam = GetComponent<Camera>();
    }

    void Update()
    {
        LookAt();
    }
    
    // return the distance between two positions
    float Distance(Vector3 positionA, Vector3 positionB)
    {
        float distance = Mathf.Sqrt(Mathf.Pow(positionB.x-positionA.x, 2) + Mathf.Pow(positionB.z-positionA.z, 2));
        return distance;
    }

    // return the inverse of the distance between two positions
    float InverseDistance(Vector3 positionA, Vector3 positionB)
    {
        float distance = Distance(positionA, positionB);
        if(distance == 0)
        {
            return distance;
        } else
        {
            return 1 / distance;
        }
    }

    // sum inverse distances for one position to all other positions,
    // and store the value in a table at the same index of the element index
    void SumInversesDistancesToOtherObjects()
    {
        elementsSumInverseDistancesToOtherObjects = new float[Elements.Length]; // reset to empty and match length
        for(int i=0; i<Elements.Length; i++)
        {
            float sumElementInverseDistances = 0;
            for(int j=0; j<Elements.Length; j++)
            {
                if(j != i) // do not calculate distance to the gameobject itself
                {
                    sumElementInverseDistances += InverseDistance(Elements[i].transform.position, Elements[j].transform.position);
                }
            }
            elementsSumInverseDistancesToOtherObjects[i] = sumElementInverseDistances; // store sum
        }
    }

    // calculate the barycentre
    Vector3 GetBarycentre()
    {
        SumInversesDistancesToOtherObjects();
        float sumBarycentrePoint = 0;
        float sumPositionsX = 0;
        float sumPositionsZ = 0;
        for (int i = 0; i < elementsSumInverseDistancesToOtherObjects.Length; i++)
        {
            for(int j=0; j< elementsSumInverseDistancesToOtherObjects.Length; j++)
            {
                ElementWeightBarycentre ewb = (ElementWeightBarycentre) Elements[j].GetComponent("ElementWeightBarycentre");
                float sumByWeight = elementsSumInverseDistancesToOtherObjects[j] * ewb.GetBarycentreWeight();
                sumBarycentrePoint += sumByWeight;
                sumPositionsX += sumByWeight * Elements[j].transform.position.x;
                sumPositionsZ += sumByWeight * Elements[j].transform.position.z;
            }
        }

        float barycentreX;
        float barycentreY = 0;
        float barycentreZ;
        if (sumBarycentrePoint == 0) // avoid division by zero
        {
            barycentreX = 0;
            barycentreZ = 0;
        } else
        {
            barycentreX = sumPositionsX / sumBarycentrePoint;
            barycentreZ = sumPositionsZ / sumBarycentrePoint;
        }

        Vector3 barycentre = new Vector3(barycentreX, barycentreY, barycentreZ);
        ShowBarycentre(barycentre);
        return barycentre;
    }

    // show barycentre position
    void ShowBarycentre(Vector3 barycentrePosition)
    {
        if (BarycentreGO != null)
        {
            Destroy(BarycentreGO); // reset barycentreGO
        }
        BarycentreGO = Instantiate(BarycentrePrefab, barycentrePosition, BarycentrePrefab.transform.rotation);
    }

    // set camera postion to barycentre position
    void LookAt()
    {
        SetElements();
        Vector3 barycentrePosition = GetBarycentre();

        // set new camera position
        Vector3 targetCameraPosition;
        targetCameraPosition = new Vector3(barycentrePosition.x, transform.position.y, barycentrePosition.z); // do not change camera Y position
        float travelDuration = 2; // travel time in seconds to lerp the camera
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * travelDuration); // lerp camera position to the barycentre position

        ZoomDezoom();
    }

    // check if the target position in inside the safe zone area
    bool IsInSafeZoneArea(Vector3 targetPosition)
    {
        if(
            targetPosition.x >= transform.position.x - rangeSafeZone // left side
            && targetPosition.x <= transform.position.x + rangeSafeZone // right side
            && targetPosition.z >= transform.position.z - rangeSafeZone // bottom side
            && targetPosition.z <= transform.position.z + rangeSafeZone // top side
        )
        {
            return true; // position in inside the area
        }
        return false; // position is not inside the area
    }

    // set elements that are in the safe zone area
    void SetElements()
    {
        GameObject[] elts = GameObject.FindGameObjectsWithTag("Element"); // get all gameobjects with tag "Element"
        List<GameObject> e = new List<GameObject>(); // new list which contains gameobjects with tag "Element" that are in safe zone area
        for(int i=0; i<elts.Length; i++)
        {
            if (IsInSafeZoneArea(elts[i].transform.position)) // in safe zone area
            {
                e.Add(elts[i]); // add gameobject to the list
                elts[i].GetComponent<Renderer>().material = SafeZoneElementMaterial; // change the gameobject material to show it is in the safe zone
            }
            else // not in safe zone area
            {
                elts[i].GetComponent<Renderer>().material = DefaultElementMaterial; // change the gameobject material to show it is not in the safe zone
            }
        }
        Elements = e.ToArray(); // cast List to Array
    }

    void ZoomDezoom()
    {
        Vector3 cameraLowerLeft = Cam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 cameraLowerRight = Cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        Vector3 cameraUpperLeft = Cam.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        Vector3 cameraUpperRight = Cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float marginZoom = 3f; // margin distance on camera view to determinate bounds to zoom
        float marginDezoom = 2f; // margin distance on camera view to determinate bounds to dezoom
        float minX = 0;
        float minZ = 0;
        float maxX = 0;
        float maxZ = 0;
        for (int i = 0; i < Elements.Length; i++) // determinate bounds around elements inside the safe zone area
        {
            if (Elements[i].transform.position.x < minX) minX = Elements[i].transform.position.x;
            if (Elements[i].transform.position.z < minZ) minZ = Elements[i].transform.position.z;
            if (Elements[i].transform.position.x > maxX) maxX = Elements[i].transform.position.x;
            if (Elements[i].transform.position.z > maxZ) maxZ = Elements[i].transform.position.z;
        }
        if (
            cameraLowerLeft.x + marginZoom < minX
            && cameraLowerLeft.z + marginZoom < minZ
            && cameraLowerRight.x - marginZoom > maxX
            && cameraLowerRight.z + marginZoom < minZ
            && cameraUpperLeft.x + marginZoom < minZ
            && cameraUpperLeft.z - marginZoom > maxZ
            && cameraUpperRight.x - marginZoom > maxX
            && cameraUpperRight.z - marginZoom > maxZ
             )
        {
            Cam.orthographicSize -= 0.1f; // zoom
        }
        else if (
            cameraLowerLeft.x + marginDezoom > minX
            || cameraLowerLeft.z + marginDezoom > minZ
            || cameraLowerRight.x - marginDezoom < maxX
            || cameraLowerRight.z + marginDezoom > minZ
            || cameraUpperLeft.x + marginDezoom > minZ
            || cameraUpperLeft.z - marginDezoom < maxZ
            || cameraUpperRight.x - marginDezoom < maxX
            || cameraUpperRight.z - marginDezoom < maxZ
             )
        {
            Cam.orthographicSize += 0.1f; // dezoom
        }

        // to see more about the camera bevahiour
        // elements bounds
        Destroy(DebugElementsLimit1);
        Destroy(DebugElementsLimit2);
        Destroy(DebugElementsLimit3);
        Destroy(DebugElementsLimit4);
        DebugElementsLimit1 = Instantiate(BarycentrePrefab, new Vector3(minX, 0, minZ), BarycentrePrefab.transform.rotation);
        DebugElementsLimit2 = Instantiate(BarycentrePrefab, new Vector3(minX, 0, maxZ), BarycentrePrefab.transform.rotation);
        DebugElementsLimit3 = Instantiate(BarycentrePrefab, new Vector3(maxX, 0, minZ), BarycentrePrefab.transform.rotation);
        DebugElementsLimit4 = Instantiate(BarycentrePrefab, new Vector3(maxX, 0, maxZ), BarycentrePrefab.transform.rotation);
        // zoom bounds
        Destroy(DebugZoomLimit1);
        Destroy(DebugZoomLimit2);
        Destroy(DebugZoomLimit3);
        Destroy(DebugZoomLimit4);
        DebugZoomLimit1 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginZoom, 0, cameraLowerLeft.z + marginZoom), BarycentrePrefab.transform.rotation);
        DebugZoomLimit2 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginZoom, 0, cameraUpperRight.z - marginZoom), BarycentrePrefab.transform.rotation);
        DebugZoomLimit3 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginZoom, 0, cameraLowerLeft.z + marginZoom), BarycentrePrefab.transform.rotation);
        DebugZoomLimit4 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginZoom, 0, cameraUpperRight.z - marginZoom), BarycentrePrefab.transform.rotation);
        DebugZoomLimit1.GetComponent<Renderer>().material = DefaultElementMaterial;
        DebugZoomLimit2.GetComponent<Renderer>().material = DefaultElementMaterial;
        DebugZoomLimit3.GetComponent<Renderer>().material = DefaultElementMaterial;
        DebugZoomLimit4.GetComponent<Renderer>().material = DefaultElementMaterial;
        // dezoom bounds
        Destroy(DebugDezoomLimit5);
        Destroy(DebugDezoomLimit6);
        Destroy(DebugDezoomLimit7);
        Destroy(DebugDezoomLimit8);
        DebugDezoomLimit5 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginDezoom, 0, cameraLowerLeft.z + marginDezoom), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit6 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginDezoom, 0, cameraUpperRight.z - marginDezoom), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit7 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginDezoom, 0, cameraLowerLeft.z + marginDezoom), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit8 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginDezoom, 0, cameraUpperRight.z - marginDezoom), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit5.GetComponent<Renderer>().material = SafeZoneElementMaterial;
        DebugDezoomLimit6.GetComponent<Renderer>().material = SafeZoneElementMaterial;
        DebugDezoomLimit7.GetComponent<Renderer>().material = SafeZoneElementMaterial;
        DebugDezoomLimit8.GetComponent<Renderer>().material = SafeZoneElementMaterial;
    }
}
