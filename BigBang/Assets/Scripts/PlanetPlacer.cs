using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleKeplerOrbits;
using UnityEngine.UI;
public class PlanetPlacer : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject planet,planetholder,p2,a1,a2,test;
    public KeplerOrbitMover kepler;
    public GameObject star;
    public Transform par;
    public int i,r1,r3,randmDeg,count,starmclass;
    public float r,r2, randDist,randY,X,Zd,dist,mgsi,planetmass,a,b,c,xf;
    public Slider slideDist;
    public retur rtr;

    void Start()
    {
        par = GameObject.Find("ImageTarget").transform;
        i = 0;
        count = 0;
    }
    public void vPlace(){
        randY = 0.8f;
        starmclass = this.GetComponent<StarBirth>().starmclass;
        if(starmclass==1){//red dwarf

            randDist = Random.Range(0.6f, 0.85f);//terrestial
            mgsi = Random.Range(0,0.2f);
            Placer();
            randDist = Random.Range(0.9f, 1.2f);//gas
            mgsi = Random.Range(0.2f, 0.4f);
            Placer();
            randDist = Random.Range(1.3f, 1.4f);//ice
            mgsi = Random.Range(0.4f, 0.6f);
            Placer();

        }
        else if (starmclass==2){//yellow dwarf
            randDist = Random.Range(0.8f, 1.1f);
            mgsi = Random.Range(0, 0.2f);

            //randDist = 1;
            Placer();
            randDist = Random.Range(1.2f, 1.5f);
            mgsi = Random.Range(0.2f, 0.4f);

            Placer();
            randDist = Random.Range(1.66f, 1.88f);
            mgsi = Random.Range(0.4f, 0.6f);

            Placer();
        }
        else{//blue giant
             randDist = Random.Range(1.4f, 1.66f);
            mgsi = Random.Range(0, 0.2f);

            Placer();
            randDist = Random.Range(1.77f, 1.89f);
            mgsi = Random.Range(0.2f, 0.4f);

            Placer();
            randDist = Random.Range(1.9f, 2.2f);
            mgsi = Random.Range(0.4f, 0.6f);

            Placer();

        }

    }
    public void xPlace(){
        xf=GameObject.Find("SliderDist").GetComponent<Slider>().value;
        mgsi = GameObject.Find("Mg/SiSlider").GetComponent<Slider>().value;
        randY = 0.8f;
        if(starmclass==1){

            if (xf <= 33.33)
            {
                randDist = (0.25f/100) * xf + 0.6f;
            }else if(xf>33.33 && xf<=66.66){
                randDist = 0.003f * xf + 0.9f;

            }else{
                randDist = 0.001f * xf + 1.3f;

            }

        }
        else if(starmclass==2){
            if (xf <= 33.33)
            {
                randDist = 0.003f * xf + 0.8f;

            }
            else if (xf > 33.33 && xf <= 66.66)
            {
                randDist = 0.003f * xf + 1.2f;
            }
            else
            {
                randDist = 0.0022f * xf + 1.66f;
            }
        }else{
            if (xf <= 33.33)
            {
                randDist = 0.0022f * xf + 1.4f;
            }
            else if (xf > 33.33 && xf <= 66.66)
            {
                randDist = 0.0012f * xf + 1.77f;
            }
            else
            {
                randDist = 0.003f * xf + 1.9f; 
            }
        }

        Placer();
    }
    public void Placer(){
        randmDeg = Random.Range(1, 3);
        test =(GameObject)Instantiate(planetholder, par.transform);
        r = Random.Range(-1.5f, 1.5f);
       // p2 = (GameObject)Instantiate(planet,par.transform);
        test.transform.localPosition = new Vector3(0, randY, 0);
        //p2.name = "planet"+i.ToString();
        
        star = GameObject.FindWithTag("Star");
        X = star.transform.localPosition.x + randDist * Mathf.Cos(randmDeg);
        Zd = star.transform.localPosition.z + randDist * Mathf.Sin(randmDeg);
        //kepler = ((KeplerOrbitMover)GameObject.Find("planet"+i.ToString()).GetComponent<KeplerOrbitMover>());
        Vector3 vx =  new Vector3(X,randY ,Zd);
        test.transform.localPosition = vx;
        p2 = (GameObject)Instantiate(planet, test.transform);
        rtr = p2.GetComponent<retur>();
        kepler = rtr.kp;
        kepler.AttractorSettings.AttractorObject = star.transform;
        kepler.SetAutoCircleOrbit();

        //  kepler = p2.GetComponent<KeplerOrbitMover>();
        //kepler = planet.GetComponent<KeplerOrbitMover>();
        i++;
        //kepler = rtr.kp;
        //p2.transform.parent = par;
        rtr.kp.SetAutoCircleOrbit();
        r1 = Random.Range(1, 5);
        kepler.TimeScale = r1;
        starmclass=this.GetComponent<StarBirth>().starmclass;
      //  r2 = Random.Range(1000, 1075);
        if(starmclass==1){
            planetmass = Random.Range(0, 2.9f);
            if (planetmass < 3f)
            {
                float vval = Random.Range(0.38f, 0.42f);
               test.transform.localScale = new Vector3(vval, vval, vval);
                a = -4.13f;b = 57.31f;c = 850f;
                r2 = (float)(a*planetmass*planetmass+b*planetmass+c);
            }
            else if (planetmass >= 3f && planetmass < 5f)
            {
                float vval = Random.Range(0.42f, 0.46f);
                test.transform.localScale = new Vector3(vval, vval, vval);
                a = -3.26f;b = 47.13f;c = 875f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);
                p2.transform.localScale = new Vector3(Random.Range(0.03f, 0.136f), Random.Range(0.139f, 0.8f), Random.Range(0.81f, 1.26f));

            }
            else
            {
                float vval = Random.Range(0.46f, 0.5f);
                test.transform.localScale = new Vector3(vval, vval, vval);
                a = -2.61f;b = 39.15f;c = 895f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);
                p2.transform.localScale = new Vector3(Random.Range(0.03f, 0.136f), Random.Range(0.139f, 0.8f), Random.Range(0.81f, 1.26f));

            }
        }
        else if(starmclass==2){
            planetmass = Random.Range(3, 4.9f);
            planetmass = Random.Range(0, 2.9f);
            if (planetmass < 3f)
            {
                float vval = Random.Range(0.45f, 0.523f);
                test.transform.localScale = new Vector3(vval,vval, vval);
                a = -2.16f;b = 36.14f;c = 900f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);

            }
            else if (planetmass >= 3f && planetmass < 5f)
            {
                float vval = Random.Range(0.523f, 0.596f);
                test.transform.localScale = new Vector3(vval, vval, vval);
                a = -1.42f; b = 29.28f; c = 915f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);

            }
            else
            {
                float vval = Random.Range(0.596f, 0.67f);
                test.transform.localScale = new Vector3(vval, vval, vval);
                a = -0.98f; b = 24.890f; c = 925f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);

            }
        }
        else
        {
            planetmass = Random.Range(5, 10);
            planetmass = Random.Range(0, 2.9f);
            if (planetmass < 3f)
            {
                float vval = Random.Range(0.8f, 1.0f);
                test.transform.localScale = new Vector3(vval, vval, vval);

                a = -1.03f;b = 19.32f;c = 945f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);

            }
            else if (planetmass >= 3f && planetmass< 5f)
            {
                float vval = Random.Range(1.0f, 1.2f);
                test.transform.localScale = new Vector3(vval,vval, vval);
                a = -0.51f;b = 14.66f;c = 955f;
                r = (float)(a * planetmass * planetmass + b * planetmass + c);

            }
            else
            {
                float vval = Random.Range(1.2f, 1.4f);

                test.transform.localScale = new Vector3(vval,vval,vval);
                a = 1.53f;b = -5.38f;c = 1000f;
                r2 = (float)(a * planetmass * planetmass + b * planetmass + c);

            }
        }
        kepler.SetAutoCircleOrbit();
        //p2.transform.localPosition = vx;
    //    kepler.OrbitData.SemiMajorAxis = 0;
//        kepler.OrbitData.SemiMinorAxis = 0;
        count = 1;
        //kepler.AttractorSettings.AttractorMass = r2;
        dist = Vector3.Distance(star.transform.position, p2.transform.position);
        GameObject.Find("MATT").GetComponent<MaterialAdd>().callMat(mgsi, p2);

        //star=null;
    }

    // Update is called once per frame
    void Update()
    {
       // kepler.OrbitData.SemiMajorAxisBasis.y = 0;
        //kepler.OrbitData.SemiMinorAxisBasis.y = 0;
        if (count!=0 && count<=50){

            count++;
            kepler.OrbitData.SemiMajorAxisBasis.y = 0;
            kepler.OrbitData.SemiMinorAxisBasis.y = 0;
            kepler.SetAutoCircleOrbit();

            if (count==48){
                kepler.AttractorSettings.AttractorMass = r2;
                kepler.TimeScale = r1;
            }
            kepler.OrbitData.SemiMajorAxisBasis.y = 0;
            kepler.OrbitData.SemiMinorAxisBasis.y = 0;
            //kepler.SetAutoCircleOrbit();
        }

    }
}
