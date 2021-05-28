using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleKeplerOrbits;
public class MaterialAdd : MonoBehaviour
{
    // Start is called before the first frame update
   
    public GameObject planet;
    public KeplerOrbitMover kp;
    public Texture2D load_texture;

    public int starmclass;
    public bool isHabitable;
    //Textures


    public Texture Terrer1;
    public Texture Terrer2;
    public Texture Terrer3;
    public Texture EarthLike;
    
    public Texture Gas1;
    public Texture Gas2;
    public Texture Gas3;

    public Texture GGiant1;
    public Texture GGiant2;
    public Texture GGiant3;

    public Texture Ice1;
    public Texture Ice2;
    public Texture Ice3;

    
    public Material nayavala;

    public int star_temp;
    public float dist;
    public float metali_Fe;
    public float metali_Mg; //metallicities
    public int planet_size;
  

    //reddwarf
    public float red_dist_t1 = 0.6f;
    public float red_dist_t2 = 0.85f;
    public float red_dist_g1 = 0.9f;
    public float red_dist_g2 = 1.2f;
    public float red_dist_i1 = 1.3f;
    public float red_dist_i2 = 1.4f;

    //yellow
    public float yel_dist_t1 = 0.8f;
    public float yel_dist_t2 = 1.1f;
    public float yel_dist_g1 = 1.2f;
    public float yel_dist_g2 = 1.5f;
    public float yel_dist_i1 = 1.66f;
    public float yel_dist_i2 = 1.88f;

    //blue giant


        
    //misc
    private float mean;
    private int chance;
    public int call;
    // Start is called before the first frame update
    void Start()
    {

        /*
        nayavala = this.GetComponent<MeshRenderer>().material;

        starmclass = Random.Range(1, 3);
        //metali_Mg = Random.Range(0.0f, 0.6f);
        //assign tags on metalicity of mg si ratio
        metali_Mg = 0.45f;
        if(metali_Mg <0.2f)
        {
            this.gameObject.tag = "Terrestrial";
        }
        else if (metali_Mg<0.4f && metali_Mg>0.2f)
        {
            gameObject.tag = "GasDwarf";
        }
        else if (metali_Mg<0.6f && metali_Mg>0.4f)
        {
            chance = Random.Range(1, 10);
            chance = 1;
            if (chance % 2 == 0)
            {
                this.gameObject.tag = "IceGiant";


            }
            else { this.gameObject.tag = "GasGiant"; }
        }

        //assign textures based on tags

        if(this.gameObject.tag == "Terrestrial")
        {
            
            isHabitable = CheckHabitability();
            if (isHabitable == true)
            {
                call = 1;
                var HabTextures = Resources.LoadAll<Texture>("Textures/EarthLike") as Texture[];
                chance = Random.Range(0, HabTextures.Length);
                nayavala.SetTexture("_MainTex", HabTextures[1]);
            }
            else
            {
                call = 11;
                var terrTextures = Resources.LoadAll<Texture>("Textures/Terrer") as Texture[];
                chance = Random.Range(0, terrTextures.Length);
                nayavala.SetTexture("_MainTex", terrTextures[0]);
            }
        }

        else if(this.gameObject.tag == "GasDwarf")
        {
            call = 2; 
            var GasDwTextures = Resources.LoadAll<Texture>("Textures/Gas") as Texture[];
            chance = Random.Range(0, GasDwTextures.Length);
            nayavala.SetTexture("_MainTex", GasDwTextures[chance]);
        }

        else if (this.gameObject.tag == "IceGiant")
        {
            call = 3; 
            var IceGTextures = Resources.LoadAll<Texture>("Textures/IceG") as Texture[];
            chance = Random.Range(0, IceGTextures.Length);
            nayavala.SetTexture("_MainTex", IceGTextures[chance]);
        }

        else if (this.gameObject.tag == "GasGiant")
        {
            call = 4; 
            var GasGTextures = Resources.LoadAll<Texture>("Textures/Gas") as Texture[];
            chance = Random.Range(0, GasGTextures.Length);
            nayavala.SetTexture("_MainTex", GasGTextures[chance]);
        }

*/



        /*nayavala.SetFloat("_Shininess",50.0f);

        if (metallicity < 0.2)
        {
            //terrestial

            nayavala.SetTexture("_MainTex", Terrer1);
        
        }*/


        // get starm class of the star
        /*
        if (starmclass == 1)
        {
            //red dwarf
            star_temp = 3120;
            if (red_dist_t1 < dist && dist < red_dist_t2)
            {
                //terresterial texture
                //check metalicity for components and assign colour
                if (isHabitable == true)
                {
                    //add water clouds texture
                    nayavala.SetTexture("_MainTex", EarthLike);
                }

                else
                {
                    nayavala.SetTexture("_MainTex", Terrer1);
                }

            }

            if ((red_dist_g1 < dist) && (dist < red_dist_g2))
            {
                if (dist < mean)
                {
                    //gas dwarf texture

                }
            }
            if (red_dist_i1 < dist && dist < red_dist_i2)
            {

                //ice giant and gas giant texture
            }

        

        }
        //yellow dwarf

        else if (starmclass == 2) 
        {
            star_temp = 5780;
            if (yel_dist_t1 < dist && dist < yel_dist_t2)
            {
                //terresterial texture
                //check metalicity for components and assign colour
                if (isHabitable == true)
                {
                    //add water clouds texture
                }

            }

            if ((yel_dist_g1 < dist) && (dist < yel_dist_g2))
            {
                //gas dwarf

            }
                
            if (yel_dist_i1 < dist && dist < yel_dist_i2)
            {

                //2 options ; choose randomly or lesser mass ice giant, greater mass gass giant texture
            
            
            }
            
            }*/



    }

    public void callMat(float metal,GameObject gt){



        nayavala = gt.gameObject.GetComponent<MeshRenderer>().material;

        //starmclass = Random.Range(1, 3);
        //metali_Mg = Random.Range(0.0f, 0.6f);
        //assign tags on metalicity of mg si ratio
        metali_Mg = metal;
        if (metali_Mg < 0.2f)
        {
            gt.gameObject.tag = "Terrestrial";
        }
        else if (metali_Mg < 0.4f && metali_Mg > 0.2f)
        {
            gt.gameObject.tag = "GasDwarf";
        }
        else if (metali_Mg < 0.6f && metali_Mg > 0.4f)
        {
            chance = Random.Range(1, 10);
            chance = 1;
            if (chance % 2 == 0)
            {

                gt.gameObject.tag = "IceGiant";


            }
            else { gt.gameObject.tag = "GasGiant"; }
        }

        //assign textures based on tags

        if (gt.gameObject.tag == "Terrestrial")
        {

            isHabitable = CheckHabitability();
            if (isHabitable == true)
            {
                call = 1;
                var HabTextures = Resources.LoadAll<Texture>("Textures/EarthLike") as Texture[];
                chance = Random.Range(0, HabTextures.Length-1);
                Terrer1 = HabTextures[chance];
                nayavala.SetTexture("_MainTex", HabTextures[chance]);
            }
            else
            {
                call = 11;
                var terrTextures = Resources.LoadAll<Texture>("Textures/Terrer") as Texture[];
                chance = Random.Range(0, terrTextures.Length-1);
                Terrer1 = terrTextures[chance];

                nayavala.SetTexture("_MainTex", terrTextures[chance]);
            }
        }

        else if (gt.gameObject.tag == "GasDwarf")
        {
            call = 2;
            var GasDwTextures = Resources.LoadAll<Texture>("Textures/Gas") as Texture[];
            chance = Random.Range(0, GasDwTextures.Length-1);
            Gas1 = GasDwTextures[chance];
            nayavala.SetTexture("_MainTex", GasDwTextures[chance]);
        }

        else if (gt.gameObject.tag == "IceGiant")
        {
            call = 3;
            var IceGTextures = Resources.LoadAll<Texture>("Textures/IceG") as Texture[];
            chance = Random.Range(0, IceGTextures.Length-1);
            Ice1= IceGTextures[chance];
            nayavala.SetTexture("_MainTex", IceGTextures[chance]);
        }

        else if (gt.gameObject.tag == "GasGiant")
        {
            call = 4;
            var GasGTextures = Resources.LoadAll<Texture>("Textures/Gas") as Texture[];
            chance = Random.Range(0, GasGTextures.Length-1);
            Gas2 = GasGTextures[chance];
            nayavala.SetTexture("_MainTex", GasGTextures[chance]);
        }





    }
    // Update is called once per frame
    void Update()
    {

    }

    bool CheckHabitability()
    {
        

        return true;
        //do 
    }

}



