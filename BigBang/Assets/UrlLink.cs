using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrlLink : MonoBehaviour
{
    // Start is called before the first frame update
    public string str;
    public void khol()
    {
        Application.OpenURL(str);
    }
}
