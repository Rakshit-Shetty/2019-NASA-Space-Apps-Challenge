using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleKeplerOrbits;
public class retur : MonoBehaviour
{
    public KeplerOrbitMover kp;
    public float dist;
    public GameObject star;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        star = GameObject.FindWithTag("Star");
        kp = this.GetComponent<KeplerOrbitMover>();
    }
    // Update is called once per frame
    void Update()
    {

        dist = Mathf.Sqrt((star.transform.localPosition.x - transform.localPosition.x) * (star.transform.localPosition.x - transform.localPosition.x)+ (star.transform.localPosition.y - transform.localPosition.y) * (star.transform.localPosition.y - transform.localPosition.y)+ (star.transform.localPosition.z - transform.localPosition.z) * (star.transform.localPosition.z - transform.localPosition.z));
    }
}
