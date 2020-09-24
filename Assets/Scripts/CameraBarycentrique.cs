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
    GameObject go1;
    GameObject go2;
    GameObject go3;
    GameObject go4;
    GameObject go5;
    GameObject go6;
    GameObject go7;
    GameObject go8;
    GameObject b1;
    GameObject b2;
    GameObject b3;
    GameObject b4;

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
        float marginOut = 1f;
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
        Destroy(b1);
        Destroy(b2);
        Destroy(b3);
        Destroy(b4);
        b1 = Instantiate(BarycentrePrefab, new Vector3(minX, 0, minZ), BarycentrePrefab.transform.rotation);
        b2 = Instantiate(BarycentrePrefab, new Vector3(minX, 0, maxZ), BarycentrePrefab.transform.rotation);
        b3 = Instantiate(BarycentrePrefab, new Vector3(maxX, 0, minZ), BarycentrePrefab.transform.rotation);
        b4 = Instantiate(BarycentrePrefab, new Vector3(maxX, 0, maxZ), BarycentrePrefab.transform.rotation);
        Destroy(go1);
        Destroy(go2);
        Destroy(go3);
        Destroy(go4);
        go1 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginIn, 0, cameraLowerLeft.z + marginIn), BarycentrePrefab.transform.rotation);
        go2 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginIn, 0, cameraUpperRight.z - marginIn), BarycentrePrefab.transform.rotation);
        go3 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginIn, 0, cameraLowerLeft.z + marginIn), BarycentrePrefab.transform.rotation);
        go4 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginIn, 0, cameraUpperRight.z - marginIn), BarycentrePrefab.transform.rotation);
        go1.GetComponent<Renderer>().material = defaultElementMaterial;
        go2.GetComponent<Renderer>().material = defaultElementMaterial;
        go3.GetComponent<Renderer>().material = defaultElementMaterial;
        go4.GetComponent<Renderer>().material = defaultElementMaterial;
        Destroy(go5);
        Destroy(go6);
        Destroy(go7);
        Destroy(go8);
        go5 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginOut, 0, cameraLowerLeft.z + marginOut), BarycentrePrefab.transform.rotation);
        go6 = Instantiate(BarycentrePrefab, new Vector3(cameraLowerLeft.x + marginOut, 0, cameraUpperRight.z - marginOut), BarycentrePrefab.transform.rotation);
        go7 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginOut, 0, cameraLowerLeft.z + marginOut), BarycentrePrefab.transform.rotation);
        go8 = Instantiate(BarycentrePrefab, new Vector3(cameraUpperRight.x - marginOut, 0, cameraUpperRight.z - marginOut), BarycentrePrefab.transform.rotation);
        go5.GetComponent<Renderer>().material = safeZoneElementMaterial;
        go6.GetComponent<Renderer>().material = safeZoneElementMaterial;
        go7.GetComponent<Renderer>().material = safeZoneElementMaterial;
        go8.GetComponent<Renderer>().material = safeZoneElementMaterial;
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

        bool a1 = cameraLowerLeft.z + marginOut > minZ;
        bool a2 = cameraLowerLeft.z + marginOut > minZ;
        bool a3 = cameraLowerRight.x - marginOut < maxX;
        bool a4 = cameraLowerRight.z + marginOut > minZ;
        bool a5 = cameraUpperLeft.x + marginOut > minZ;
        bool a6 = cameraUpperLeft.z - marginOut < maxZ;
        bool a7 = cameraUpperRight.x - marginOut < maxX;
        bool a8 = cameraUpperRight.z - marginOut < maxZ;

        Debug.Log("BEGIN");
        Debug.Log("cameraLowerLeft.x + marginOut > minX ? " + a1);
        Debug.Log("cameraLowerLeft.z + marginOut > minZ ? " + a2);
        Debug.Log("cameraLowerRight.x - marginOut < maxX ? " + a3);
        Debug.Log("cameraLowerRight.z + marginOut > minZ ? " + a4);
        Debug.Log("cameraUpperLeft.x + marginOut > minZ ? " + a5);
        Debug.Log("cameraUpperLeft.z - marginOut < maxZ ? " + a6);
        Debug.Log("cameraUpperRight.x - marginOut < maxX ? " + a7);
        Debug.Log("cameraUpperRight.z - marginOut < maxZ ? " + a8);
        Debug.Log("END");
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
