using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ElementWeightBarycentre : MonoBehaviour
{
    [Range(1, 100)] [SerializeField] public float BarycentreWeight;
    Vector3 screenPos;

    private void Start()
    {
        if (BarycentreWeight <= 0) BarycentreWeight = 1;
    }

    void Update() {
        screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos.y = Screen.height - (screenPos.y + 50);
        screenPos.x = Screen.width - (Screen.width - screenPos.x);
    }
    void OnGUI() {
        GUI.contentColor = GetComponent<Renderer>().material.color;
        GUI.Label(new Rect(screenPos.x, screenPos.y, 100, 50), BarycentreWeight.ToString());
    }

    public float GetBarycentreWeight()
    {
        return BarycentreWeight;
    }
}
