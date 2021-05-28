using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniInfoTabConfig : MonoBehaviour
{
    public GameObject canvasPrefab;
    void OnMouseDown()
    {
        // load a new scene
        ShowInfo();
    }
    private void ShowInfo()
    {
        _ = Instantiate(canvasPrefab, transform.position, Quaternion.identity, transform);
    }
}
