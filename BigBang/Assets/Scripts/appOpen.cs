using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class appOpen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void open(){
        Application.OpenURL("com.Nasa.MarsVR");
        bool fail = false;
        string bundleId = "com.Nasa.MarsVR"; // your target bundle id
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

        AndroidJavaObject launchIntent = null;
        try
        {
            launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
        }
        catch (System.Exception e)
        {
            fail = true;
        }

        if (fail)
        { //open app in store
            Application.OpenURL("https://google.com");
        }
        else //open the app
            ca.Call("startActivity", launchIntent);

        up.Dispose();
        ca.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
