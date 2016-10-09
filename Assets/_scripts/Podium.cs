using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Podium : MonoBehaviour {

	// Use this for initialization
	public orderManger om;
	public int podID;
	public GameObject[] arr;


	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}
	void OnCollisionEnter(Collision col){
		
		if (col.gameObject.tag == "object") {
			col.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
			om.order [podID] = col.gameObject.GetComponent<ObjectID> ().objID;

			Debug.Log (om.order[podID]);


		}
	}

}
