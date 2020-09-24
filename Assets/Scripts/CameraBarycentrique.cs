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

    void Start()
    {

    }

    void Update()
    {
        SetElements();
        LookAt();
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
