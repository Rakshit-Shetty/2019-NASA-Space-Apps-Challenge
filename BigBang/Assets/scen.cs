using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void back(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
