using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walls : MonoBehaviour {
	[SerializeField]
	Vector3			toRemove;

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "NPC")
			collision.collider.transform.position -= toRemove;
	}
}
