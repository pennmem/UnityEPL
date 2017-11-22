using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour 
{
    public float jumpStrength = 100f;

	void Update ()
    {
        if (Input.GetButtonDown("Jump"))
            gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpStrength, 0));
    }
}
