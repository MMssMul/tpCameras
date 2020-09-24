using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBarycentrique : MonoBehaviour
{
    GameObject[] elements;
    float[] elementsSumInversesDistancesOtherToObjects;
    float rangeSafeZone = 10;
    public GameObject BarycentrePrefab;
    private GameObject BarycentreGO;
    public Material defaultElementMaterial;
    public Material safeZoneElementMaterial;
    GameObject DebugZoomLimit1;
    GameObject DebugZoomLimit2;
    GameObject DebugZoomLimit3;
    GameObject DebugZoomLimit4;
    GameObject DebugDezoomLimit5;
    GameObject DebugDezoomLimit6;
    GameObject DebugDezoomLimit7;
    GameObject DebugDezoomLimit8;
    GameObject DebugElementsLimit1;
    GameObject DebugElementsLimit2;
    GameObject DebugElementsLimit3;
    GameObject DebugElementsLimit4;

    void Start()
    {
        
    }

    void Update()
    {
        SetElements();
        LookAt();
        ZoomDezoom();
            
    }
    
    // return the distance between two positions
    float GetDistance(Vector3 positionA, Vector3 positionB)
    {
        float distance = Mathf.Sqrt(Mathf.Pow(positionB.x-positionA.x, 2) + Mathf.Pow(positionB.z-positionA.z, 2));
        return distance;
    }

    // return the inverse of the distance between two positions
    float GetInverseDistance(Vector3 positionA, Vector3 positionB)
    {
        float distance = GetDistance(positionA, positionB);
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
    void SetSumInversesDistancesToOtherObjects()
    {
        elementsSumInversesDistancesOtherToObjects = new float[elements.Length]; // reset to default and match length
        for(int i=0; i<elements.Length; i++)
        {
            float sumElement = 0;
            for(int j=0; j<elements.Length; j++)
            {
                if(j != i) // do not calculate distance to myself
                {
                    sumElement += GetInverseDistance(elements[i].transform.position,
                                                            elements[j].transform.position);
                }
            }
            elementsSumInversesDistancesOtherToObjects[i] = sumElement; // store sum
        }
    }

    // calculate the barycentre
    Vector3 GetBarycentre()
    {
        SetSumInversesDistancesToOtherObjects();
        float sumBarycentrePoint = 0;
        float sumPositionsX = 0;
        float sumPositionsZ = 0;
        for (int i = 0; i < elementsSumInversesDistancesOtherToObjects.Length; i++)
        {
            for(int j=0; j< elementsSumInversesDistancesOtherToObjects.Length; j++)
            {
                ElementWeightBarycentre ewb = (ElementWeightBarycentre) elements[j].GetComponent("ElementWeightBarycentre");
                float sumByWeight = elementsSumInversesDistancesOtherToObjects[j] * ewb.GetBarycentreWeight();
                sumBarycentrePoint += sumByWeight;
                sumPositionsX += sumByWeight * elements[j].transform.position.x;
                sumPositionsZ += sumByWeight * elements[j].transform.position.z;
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

    // show barycentre position by creating an object
    void ShowBarycentre(Vector3 barycentrePosition)
    {
        if (BarycentreGO != null)
        {
            Destroy(BarycentreGO); // reset barycentreGO
        }
        BarycentreGO = Instantiate(BarycentrePrefab, barycentrePosition, BarycentrePrefab.transform.rotation);
    }

    void ZoomDezoom()
    {
        Camera cam = GetComponent<Camera>();
        Vector3 cameraLowerLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 cameraLowerRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        Vector3 cameraUpperLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        Vector3 cameraUpperRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float marginIn = 3f;
        float marginOut = 2f;
        float minX = 0;
        float minZ = 0;
        float maxX = 0;
        float maxZ = 0;
        for (int i=0; i<elements.Length; i++) // determinate bounds around elements inside the safe zone area
        {
            if (elements[i].transform.position.x < minX) minX = elements[i].transform.position.x;
            if (elements[i].transform.position.z < minZ) minZ = elements[i].transform.position.z;
            if (elements[i].transform.position.x > maxX) maxX = elements[i].transform.position.x;
            if (elements[i].transform.position.z > maxZ) maxZ = elements[i].transform.position.z;
        }
        if (
            cameraLowerLeft.x + marginIn < minX
            && cameraLowerLeft.z + marginIn < minZ
            && cameraLowerRight.x - marginIn > maxX
            && cameraLowerRight.z + marginIn < minZ
            && cameraUpperLeft.x + marginIn < minZ
            && cameraUpperLeft.z - marginIn > maxZ
            && cameraUpperRight.x - marginIn > maxX
            && cameraUpperRight.z - marginIn > maxZ
             )
        {
            cam.orthographicSize -= 0.1f; // zoom
        }
        else if (
            cameraLowerLeft.x + marginOut > minX
            || cameraLowerLeft.z + marginOut > minZ
            || cameraLowerRight.x - marginOut < maxX
            || cameraLowerRight.z + marginOut > minZ
            || cameraUpperLeft.x + marginOut > minZ
            || cameraUpperLeft.z - marginOut < maxZ
            || cameraUpperRight.x - marginOut < maxX
            || cameraUpperRight.z - marginOut < maxZ
             )
        {
            cam.orthographicSize += 0.1f; // dezoom
        }

        // VISUAL DEBUG
        /*
        Destroy(DebugElementsLimit1);
        Destroy(DebugElementsLimit2);
        Destroy(DebugElementsLimit3);
        Destroy(DebugElementsLimit4);
        DebugElementsLimit1 = Instantiate(BarycentrePrefab, new Vector3(minX, 0, minZ), BarycentrePrefab.transform.rotation);
        DebugElementsLimit2 = Instantiate(BarycentrePrefab, new Vector3(minX, 0, maxZ), BarycentrePrefab.transform.rotation);
        DebugElementsLimit3 = Instantiate(BarycentrePrefab, new Vector3(maxX, 0, minZ), BarycentrePrefab.transform.rotation);
        DebugElementsLimit4 = Instantiate(BarycentrePrefab, new Vector3(maxX, 0, maxZ), BarycentrePrefab.transform.rotation);
        */
        Destroy(DebugZoomLimit1);
        Destroy(DebugZoomLimit2);
        Destroy(DebugZoomLimit3);
        Destroy(DebugZoomLimit4);
        DebugZoomLimit1 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginIn, 0, cameraLowerLeft.z + marginIn), BarycentrePrefab.transform.rotation);
        DebugZoomLimit2 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginIn, 0, cameraUpperRight.z - marginIn), BarycentrePrefab.transform.rotation);
        DebugZoomLimit3 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginIn, 0, cameraLowerLeft.z + marginIn), BarycentrePrefab.transform.rotation);
        DebugZoomLimit4 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginIn, 0, cameraUpperRight.z - marginIn), BarycentrePrefab.transform.rotation);
        DebugZoomLimit1.GetComponent<Renderer>().material = defaultElementMaterial;
        DebugZoomLimit2.GetComponent<Renderer>().material = defaultElementMaterial;
        DebugZoomLimit3.GetComponent<Renderer>().material = defaultElementMaterial;
        DebugZoomLimit4.GetComponent<Renderer>().material = defaultElementMaterial;
        Destroy(DebugDezoomLimit5);
        Destroy(DebugDezoomLimit6);
        Destroy(DebugDezoomLimit7);
        Destroy(DebugDezoomLimit8);
        DebugDezoomLimit5 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginOut, 0, cameraLowerLeft.z + marginOut), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit6 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginOut, 0, cameraUpperRight.z - marginOut), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit7 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginOut, 0, cameraLowerLeft.z + marginOut), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit8 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginOut, 0, cameraUpperRight.z - marginOut), BarycentrePrefab.transform.rotation);
        DebugDezoomLimit5.GetComponent<Renderer>().material = safeZoneElementMaterial;
        DebugDezoomLimit6.GetComponent<Renderer>().material = safeZoneElementMaterial;
        DebugDezoomLimit7.GetComponent<Renderer>().material = safeZoneElementMaterial;
        DebugDezoomLimit8.GetComponent<Renderer>().material = safeZoneElementMaterial;

    }

    // set camera postion to barycentre position
    void LookAt()
    {
        Vector3 barycentrePosition = GetBarycentre();

        // set new camera position
        Vector3 targetCameraPosition;
        targetCameraPosition = new Vector3(barycentrePosition.x, transform.position.y, barycentrePosition.z);
        float travelDuration = 2;
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * travelDuration);
    }

    // check if the target position in inside the safe zone area
    bool IsInSafeZoneArea(Vector3 targetPosition)
    {
        if(targetPosition.x >= transform.position.x - rangeSafeZone
            && targetPosition.x <= transform.position.x + rangeSafeZone
            && targetPosition.z >= transform.position.z - rangeSafeZone
            && targetPosition.z <= transform.position.z + rangeSafeZone)
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
                e.Add(elts[i]);
                elts[i].GetComponent<Renderer>().material = safeZoneElementMaterial;
            }
            else // not in safe zone area
            {
                elts[i].GetComponent<Renderer>().material = defaultElementMaterial;
            }
        }
        elements = e.ToArray(); // cast List to Array
    }
}
