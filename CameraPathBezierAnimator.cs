/******************************************************************************/
/*!
\file      CameraPathBezierAnimator.cs
\author    Colt Johnson
\date      2013
\brief     Determines how the path will be animated
\par       History:  
\par       Copyright 2013, Digipen Institute of Technology
*/
/******************************************************************************/
using UnityEngine;
using System.Collections;


public class CameraPathBezierAnimator : MonoBehaviour 
{
	
	public enum modes
	{
		once,
		loop,
		pingPong
	}
	
	public CameraPathBezier bezier;
	//do you want this path to automatically animate at the start of your scene
	public bool playOnStart = true;
	//the actual transform you want to animate
	public Transform animationTarget = null;
	private bool playing = false;
	public modes mode = modes.once;
	private float pingPongDirection = 1;
	public bool normalised = true;
	
	
	//the time used in the editor to preview the path animation
	public float editorTime = 0;
	//the time the path animation should last for
	public float pathTime = 10;
	private float _percentage = 0;
	
	//the sensitivity of the mouse in mouselook
	public float sensitivity = 5.0f;
	//the minimum the mouse can move down
	public float minX = -90.0f;
	//the maximum the mouse can move up
	public float maxX = 90.0f;
	private float rotationX = 0;
	private float rotationY = 0;
	
	public CameraPathBezierAnimator nextAnimation = null;
	
	//play the animation as runtime
	public void Play()
	{
		playing = true;
	}
	
	//stop and set the animation at the beginning
	public void Stop()
	{
		playing = false;
		_percentage = 0;
	}
	
	//pause the animation where it is
	public void Pause()
	{
		playing = false;
	}
	
	//set the time of the animtion (0-1)
	public void Seek(float value)
	{
		_percentage = Mathf.Clamp01(value);
	}
	
	public bool isPlaying
	{
		get{return playing;}
	}
	
	public float percentage
	{
		get{return _percentage;}
	}
	
	public bool pingPongGoingForward
	{
		get{return pingPongDirection==1;}
	}
	
	//normalise the curve and apply easing
	public float RecalculatePercentage(float percentage)
	{
		if(!bezier)
			bezier = GetComponent<CameraPathBezier>();
		if(bezier.numberOfControlPoints==0)
			return percentage;
		float normalisedPercentage = bezier.GetNormalisedT(percentage);
		int numberOfCurves = bezier.numberOfCurves;
		float curveT = 1.0f/(float)numberOfCurves;
		int point = Mathf.FloorToInt(normalisedPercentage/curveT);
		float curvet = Mathf.Clamp01((normalisedPercentage - point * curveT) * numberOfCurves);
		return bezier.controlPoints[point]._curve.Evaluate(curvet)/numberOfCurves + (point*curveT);
	}
	void Start()
	{
		
		bezier = GetComponent<CameraPathBezier>();
		bezier.RecalculateStoredValues();
		playing = playOnStart;
		
		_percentage = 0;
		
		Vector3 initalRotation = bezier.GetPathRotation(0).eulerAngles;
		rotationX = initalRotation.y;
		rotationY = initalRotation.x;
	}
	
	void Update()
	{
		
				if(nextAnimation!=null)
				{
					nextAnimation.Play();
					nextAnimation=null;
				}
		
	}
	
	void LateUpdate()
	{
	
		if(playing)
		{
			UpdateAnimationTime();
			UpdateAnimation();
		}
		else
		{
			if(nextAnimation!=null && _percentage >= 1)
			{
				nextAnimation.Play();
				nextAnimation=null;
			}
		}
		
	}
	
	void OnDrawGizmos()
	{
		if(bezier==null)
			return;
		
		if(Application.isPlaying)
			return;
		
		if(bezier.numberOfControlPoints==0)
			return;
		
		Vector3 debugPosition = bezier.GetPathPosition(Mathf.Clamp01(RecalculatePercentage(editorTime)));
		
		Gizmos.color = new Color(1.0f,0.0f,1.0f);
		Gizmos.DrawLine(debugPosition,debugPosition+Vector3.up * 0.5f);
		Gizmos.DrawLine(debugPosition,debugPosition+Vector3.down * 0.5f);
		Gizmos.DrawLine(debugPosition,debugPosition+Vector3.left * 0.5f);
		Gizmos.DrawLine(debugPosition,debugPosition+Vector3.right * 0.5f);
		Gizmos.DrawLine(debugPosition,debugPosition+Vector3.forward * 0.5f);
		Gizmos.DrawLine(debugPosition,debugPosition+Vector3.back * 0.5f);	
	}
	
	void UpdateAnimation()
	{
		if(animationTarget==null){
			Debug.LogError("There is no aniamtion target specified in the Camera Path Bezier Animator component. Nothing to animate.\nYou can find this component in the main camera path component.");
			return;
		}
		
		if(!playing)
			return;
		
		float usePercentage = normalised? RecalculatePercentage(_percentage): _percentage;
		animationTarget.position = bezier.GetPathPosition(usePercentage);
			animationTarget.camera.fov = bezier.GetPathFOV(usePercentage);
		
		Vector3 minusPoint, plusPoint;
		switch(bezier.mode)
		{
			case CameraPathBezier.viewmodes.usercontrolled:
				animationTarget.rotation = bezier.GetPathRotation(_percentage);
				break;
			
			case CameraPathBezier.viewmodes.target:
				animationTarget.LookAt(bezier.target.transform.position);
				break;
			
			case CameraPathBezier.viewmodes.followpath:
				if(!bezier.loop)
				{
					minusPoint = bezier.GetPathPosition(Mathf.Clamp01(usePercentage-0.05f));
					plusPoint = bezier.GetPathPosition(Mathf.Clamp01(usePercentage+0.05f));
				}
				else
				{
					float minus = usePercentage-0.05f;
					if(minus<0)
						minus+=1;
				
					float plus = usePercentage+0.05f;
					if(plus>1)
						plus+=-1;
				
					minusPoint = bezier.GetPathPosition(minus);
					plusPoint = bezier.GetPathPosition(plus);
				}
			
				animationTarget.LookAt(animationTarget.position+(plusPoint-minusPoint));
				break;
	
			
		}
	}
	
	private void UpdateAnimationTime()
	{
		switch(mode)
		{
			
			case modes.once:
				if(_percentage>=1)
					playing=false;
				else
					_percentage += Time.deltaTime * (1.0f/pathTime);
				
				break;
			
			case modes.loop:
				_percentage += Time.deltaTime * (1.0f/pathTime);
				if(_percentage>=1)
					_percentage+=-1;
				
				break;
			
			
			case modes.pingPong:
				_percentage += Time.deltaTime * (1.0f/pathTime) * pingPongDirection;
				
				if(_percentage>=1)
					pingPongDirection=-1;
				
				if(_percentage<=0)
					pingPongDirection=1;
				
				break;
		}
		
		_percentage = Mathf.Clamp01(_percentage);
	}
	
	
	private float ClampAngle(float angle, float min, float max) 
	{
	   if (angle < -360)
	      angle += 360;
	
	   if (angle > 360)
	      angle -= 360;
	
	   return Mathf.Clamp (angle, -max, -min);
	
	}
}
