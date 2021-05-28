using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tp : MonoBehaviour

{
    public Texture Ice3;


    public Material nayavala;
    // Start is called before the first frame update
    void Start()
    {
    nayavala = this.GetComponent<MeshRenderer>().material;
    var EarthLike = Resources.Load<Texture>("Textures/download") as Texture;
    var textures = Resources.LoadAll<Texture>("Textures/Terrer") as Texture[];
        
    nayavala.SetTexture("_MainTex", textures[0]);

}

    // Update is called once per frame
    void Update()
    {
        
    }
}
