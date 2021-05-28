using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceGraphicsToolkit;
using UnityEngine.UI;

public class StarBirth : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject star1,star2,star3,newstar,nebula1,nebula2,disk,bill,prominence,billsub,chakra,interstar;
    public GameObject sn1,sn2,sn3;
    public Color starcolor,starcolorsub;
    public Transform par;
    public SgtAccretion sgt;
    public SgtProminence sgtp;
    public int c1,c2,c3,starSelect,temp,starmclass;
    public float speed,valstar,metstar;
    public bool neb,activated,act2,mslide;
    public Slider mass_slider, metslider;
    void Start()
    {
        act2 = false;
        c1 = 0;

        // sgt.Brightness = 0;
        //sgtp.Brightness = 0;
        prominence.transform.localScale = new Vector3(prominence.transform.localScale.x, 0, prominence.transform.localScale.z);
        starcolor = bill.GetComponent<SpriteRenderer>().color;
        // starcolorsub = billsub.GetComponent<SpriteRenderer>().color;
        starcolor.a = 0;
        starcolorsub.a = 0;
        bill.GetComponent<SpriteRenderer>().color = starcolor;
        billsub.GetComponent<SpriteRenderer>().color = starcolor;
        disk.SetActive(false);
        // par = GameObject.Find("Billboard").transform;
        neb = false;
        activated = false;
        chakra.transform.localScale = new Vector3(0, 0, 0);
        interstar.transform.localScale = new Vector3(0, 0, 0);
        starcolorsub = sn1.GetComponent<Renderer>().material.GetColor("_TintColor");
        c2 = 0;
        c3 = 0;
        mass_slider = GameObject.Find("MassSlider").GetComponent<Slider>();
        metslider = GameObject.Find("MetSlider").GetComponent<Slider>();
        mslide = true;
    }
    public void Trans1(){
        neb = true;

    }
    public void setup(){
        mass_slider = GameObject.Find("MassSlider").GetComponent<Slider>();
        metslider = GameObject.Find("MetSlider").GetComponent<Slider>();
        mslide = true;
    }
    public  void Create(){
        if (valstar < 1.3)
        {
            newstar = Instantiate(star1, par);
            //newstar.SetActive(false);
            newstar.name = "Star";
            starmclass = 1;

        }
        else if(valstar>=1.3 && valstar<1.55){
            newstar = Instantiate(star2, par);
           // newstar.SetActive(false);
            newstar.name = "Star";
            starmclass = 2;

        }
        else
        {
            newstar = Instantiate(star3, par);
          //  newstar.SetActive(false);
            newstar.name = "Star";
            starmclass = 3;

        }

        if (metstar<33.33 && starmclass==1){
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x*150)/100), ((newstar.transform.localScale.x * 150) / 100), ((newstar.transform.localScale.x * 150) / 100));

        }
        else if(metstar>=33.33 && metstar<66.66 && starmclass==1){
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 165) / 100), ((newstar.transform.localScale.x * 165) / 100), ((newstar.transform.localScale.x * 165) / 100));
        }
        else if(metstar>=66.66 && starmclass==1)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 175) / 100), ((newstar.transform.localScale.x * 175) / 100), ((newstar.transform.localScale.x * 175) / 100));
        }

        if (metstar < 33.33 && starmclass == 2)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 108) / 100), ((newstar.transform.localScale.x * 108) / 100), ((newstar.transform.localScale.x * 108) / 100));

        }
        else if (metstar >= 33.33 && metstar < 66.66 && starmclass == 2)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 113) / 100), ((newstar.transform.localScale.x * 113) / 100), ((newstar.transform.localScale.x * 113) / 100));
        }
        else if (metstar >= 66.66 && starmclass == 2)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 116) / 100), ((newstar.transform.localScale.x * 116) / 100), ((newstar.transform.localScale.x * 116) / 100));
        }

        if (metstar < 33.33 && starmclass == 3)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 103) / 100), ((newstar.transform.localScale.x * 103) / 100), ((newstar.transform.localScale.x * 103) / 100));

        }
        else if (metstar >= 33.33 && metstar < 66.66 && starmclass == 3)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 104) / 100), ((newstar.transform.localScale.x * 104) / 100), ((newstar.transform.localScale.x * 104) / 100));
        }
        else if (metstar >= 66.66 && starmclass == 3)
        {
            newstar.transform.localScale = new Vector3(((newstar.transform.localScale.x * 105) / 100), ((newstar.transform.localScale.x * 105) / 100), ((newstar.transform.localScale.x * 105) / 100));
        }
        gameObject.GetComponent<PlanetPlacer>().vPlace();
        c3 = 1;
       // newstar.SetActive(true);
    }
    public void Trans2(){
        //sn1.GetComponent<Renderer>().material.SetColor("_TintColor", Color.black);

        disk.SetActive(true);
        activated = true;
        mslide = false;
    }

   
    // Update is called once per frame
    void Update()
    {
        if(mslide==true){
            valstar = mass_slider.value;
            metstar = metslider.value;
        }

        if (neb == true)
        {

            //starcolorsub = sn1.GetComponent<MeshRenderer>().material.GetColor("Tint Color");
            //  nebula1.transform.Rotate(Vector3.left * speed * Time.deltaTime);
            nebula1.transform.Rotate(0, -speed/2 * Time.deltaTime, 0);
            nebula2.transform.Rotate(0, -speed/4 * Time.deltaTime, 0);
            speed++;
            if(speed>=1500){
                Trans2();
            }
        }

        if(sgt.Brightness > 0 && c3==1)
            {
            newstar.transform.parent= GameObject.Find("ImageTarget").transform; 
            sgt.Brightness -= 0.0059f;
            sgtp.Brightness -= 0.00075f;
            sgtp.UpdateMesh();
            sgtp.UpdateMaterial();
            sgt.UpdateMaterial();
            sgt.UpdateMesh();
            if (prominence.transform.localScale.y > 0)
            {
                prominence.transform.localScale = new Vector3(prominence.transform.localScale.x, prominence.transform.localScale.y - 0.1f, prominence.transform.localScale.z);
            }

            if (interstar.transform.localScale.x > 0)
            {
                interstar.transform.localScale = new Vector3(interstar.transform.localScale.x - 0.0005f, interstar.transform.localScale.y - 0.0005f, interstar.transform.localScale.z - 0.0005f);
            }

        }

        if (activated == true && c1 <= 200)
        {
            /*if (sgt.Brightness < 1.18f)
            {
                sgt.Brightness += 0.0236f;
            }

            if (sgtp.Brightness < 0.15)
            {
                sgtp.Brightness += 0.003f;
            }*/
            if (prominence.transform.localScale.y < 20)
            {
                prominence.transform.localScale = new Vector3(prominence.transform.localScale.x, prominence.transform.localScale.y + 0.1f, prominence.transform.localScale.z);
            }

            if (interstar.transform.localScale.x < 0.1)
            {
                interstar.transform.localScale = new Vector3(interstar.transform.localScale.x + 0.0005f, interstar.transform.localScale.y + 0.0005f, interstar.transform.localScale.z + 0.0005f);
            }

            if (chakra.transform.localScale.x < 1)
            {
                chakra.transform.localScale = new Vector3(chakra.transform.localScale.x + 0.005f, chakra.transform.localScale.x + 0.005f, chakra.transform.localScale.x + 0.005f);
            }

            if (starcolor.a < 255)
            {
                starcolor.a += 1.275f;
                bill.GetComponent<SpriteRenderer>().color = starcolor;
                billsub.GetComponent<SpriteRenderer>().color = starcolor;

            }
            c1++;
            if(c1==180){
                act2 = true;
            }

        }
        if (act2 == true )
        {
            starcolorsub=sn1.GetComponent<Renderer>().material.GetColor("_TintColor");
            starcolorsub.a -= 1;
            sn1.GetComponent<Renderer>().material.SetColor("_TintColor", starcolorsub);
            sn2.GetComponent<Renderer>().material.SetColor("_TintColor", starcolorsub);
            sn3.GetComponent<Renderer>().material.SetColor("_TintColor", starcolorsub);
            if(starcolorsub.a<=1 && c2==0){
                c2++;
                for (int q = 0; q < 70; q++)
                {
                    if (q == 69)
                    {
                        Create();
                    }
                    
                }
            }
        }
    }

    public IEnumerator Kreate()
    {
        yield return new WaitForSeconds(1.2f);
        Create();
    }



}
