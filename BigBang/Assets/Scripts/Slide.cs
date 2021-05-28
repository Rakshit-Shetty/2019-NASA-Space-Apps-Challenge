using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Slide : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider size;
    public float valu;
    public GameObject Nebula,Nebula2,StarBirth;

    void Start()
    {
       
    }

    public void Changed(){
        Nebula.transform.localScale = new Vector3(0.002628969f * size.value, 0.002628971f * size.value, 0.001759308f * size.value);
        Nebula2.transform.localScale = new Vector3(0.002628969f * size.value, 0.002628971f * size.value, 0.001759308f * size.value);
        StarBirth.transform.localScale = new Vector3(0.2459746f * size.value, 0.2459746f * size.value, 0.2459746f * size.value);

    }
    // Update is called once per frame
    void Update()
    {

    }
}
