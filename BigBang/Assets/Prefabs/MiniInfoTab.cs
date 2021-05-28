using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniInfoTab : MonoBehaviour
{
    private const string Str01 = "Name: Planet";
    private const string Str02 = "Mass: ";
    private const string Str03 = "Metalicity: ";
    private const string Str04 = "Age: ";
    private const string Str05 = "Distance: ";
    private const string Str06 = "acchavala: ";
    public float DestroyTime;
    //GameObject uskaMass;
    // Start is called before the first frame update
    void Start()
    {
        //MiniInfoTabConfig teraMaal = gameObject.GetComponentInParent<MiniInfoTabConfig>();
        //uskaMass = GameObject.Find("Mass");
        //uskaMass.GetComponent<TextMeshProUGUI>().text = string.Concat(Str02, teraMaal.meraMass.ToString());
        Destroy(gameObject, DestroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        // Face the camera directly.
        transform.LookAt(Camera.main.transform.position);

        // Rotate so the visible side faces the camera.
        transform.Rotate(0, 180, 0);
    }
}
