using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleKeplerOrbits;
public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject planet;
    public KeplerOrbitMover kp;
    public float val;
    void Start()

    {
        planet = GameObject.Find("Planet01");
        kp = planet.GetComponent<KeplerOrbitMover>();
        Debug.Log(kp.AttractorSettings.AttractorMass);
        val = kp.AttractorSettings.AttractorMass;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
