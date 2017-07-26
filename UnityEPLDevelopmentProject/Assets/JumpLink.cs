using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpLink : MonoBehaviour 
{

	public UnityEngine.Rigidbody jumper;

	public void DoJump()
	{
		jumper.velocity += new Vector3 (0, 3, 0);
	}


}