/******************************************************************************/
/*!   
\author    Colt Johnson
\date      2013
\brief   
\par       History:  
\par       Copyright 2013, Digipen Institute of Technology
*/
/******************************************************************************/
using UnityEngine;
using System.Collections;

public class CameraPathTrigger : MonoBehaviour {
	
	private bool playing = false;
	public Collider target;
	public Camera playercamera;
	private Transform camerapos;
	private bool triggered = false;
	public CameraPathBezierAnimator curve = null;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (curve.isPlaying == false)
		{
			if (playing == true)
			{
				playing = false;
				playercamera.transform.position =  camerapos.position;
			}
		}
	
	}
	
	void OnTriggerEnter(Collider collision)
	{
		if (collision != target) //The colliding object isn't our object
		{
			return; //don't do anything if it's not our target
		}
		
		if (triggered == false)
		{
		
			camerapos = playercamera.transform;
			playing = true;
			curve.Play();
			
		}
		
		triggered = true;	
		
	
	}	
}
