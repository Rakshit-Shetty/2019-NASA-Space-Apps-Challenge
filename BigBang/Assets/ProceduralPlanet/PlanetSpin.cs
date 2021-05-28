using UnityEngine;
using System.Collections;

public class PlanetSpin : MonoBehaviour
{
	public float speed = 0.01f;
	public Vector3 axis = Vector3.up;
		
	// Update is called once per frame
	void Update ()
	{
		transform.Rotate (axis, Time.smoothDeltaTime * speed);
	}
}
