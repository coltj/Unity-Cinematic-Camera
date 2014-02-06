/******************************************************************************/
/*!
\file      CameraPathBezierControlPoint.cs
\author    Colt Johnson
\date      2013
\brief     Defines a control point, which is used for the bezier curve
\par       History:  
\par       Copyright 2013, Digipen Institute of Technology
*/
/******************************************************************************/
using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class CameraPathBezierControlPoint : MonoBehaviour {
	

	
	//the control point for this point - modifies the curve value for this end of the curve
	public Vector3 controlPoint = Vector3.zero;
	//a link ot the bezier curve class
	public CameraPathBezier bezier;
	public AnimationCurve _curve;
	
	public float FOV = 60;
	
	//variables to define the frustum boxes 
    private float directionLineLength = 1.0f;
    private float focusBoxLength = 0.25f;
	
	public Vector3 worldControlPoint
	{
		get
		{
			return controlPoint + transform.position;
		}
	}
	
	public Vector3 reverseWorldControlPoint
	{
		get
		{
			return -controlPoint + transform.position;
		}
	}
	
	public bool isLastPoint
	{
		get
		{
			if(bezier==null || bezier.controlPoints == null)
				return true;
			return this == bezier.controlPoints[bezier.numberOfControlPoints-1];
		}
	}
	
	public AnimationCurve curve
	{
		get{return _curve;}
	}
	
	
	public void SetRotationToCurve()
	{
		float thisperc = bezier.GetPathPercentageAtPoint(this);
		float lastperc = Mathf.Clamp01(thisperc-0.05f);
		float nextperc = Mathf.Clamp01(thisperc+0.05f);
		Vector3 lastPos = bezier.GetPathPosition(lastperc);
		Vector3 nextPos = bezier.GetPathPosition(nextperc);
		Vector3 dir = nextPos-lastPos;
		transform.LookAt(transform.position+dir);
	}
	
	void OnEnable()
	{
		SetAnimationCurve();
	}
	
	void OnDestroy()
	{
		if(bezier!=null)
		{
			bezier.DeletePoint(this,false);
		}
	}
	
	void OnDrawGizmos()
	{
		
		Gizmos.DrawIcon(transform.position, "pathpoint");
		
		switch(bezier.mode)
		{
		case CameraPathBezier.viewmodes.usercontrolled:
			
			Vector3 directionTL = transform.TransformDirection(new Vector3(-focusBoxLength, -focusBoxLength, 1.0f) * directionLineLength) + transform.position;
			Vector3 directionTR = transform.TransformDirection(new Vector3(focusBoxLength, -focusBoxLength, 1.0f) * directionLineLength) + transform.position;
			Vector3 directionBL = transform.TransformDirection(new Vector3(-focusBoxLength, focusBoxLength, 1.0f) * directionLineLength) + transform.position;
			Vector3 directionBR = transform.TransformDirection(new Vector3(focusBoxLength, focusBoxLength, 1.0f) * directionLineLength) + transform.position;
			
			Gizmos.color = new Color(0,1,0,0.6f);
			Gizmos.DrawLine(transform.position, directionTL);
			Gizmos.DrawLine(transform.position, directionTR);
			Gizmos.DrawLine(transform.position, directionBL);
			Gizmos.DrawLine(transform.position, directionBR);
			
			Gizmos.color = new Color(0,1,0,0.4f);
			Gizmos.DrawLine(directionBL, directionTL);
			Gizmos.DrawLine(directionTL, directionTR);
			Gizmos.DrawLine(directionTR, directionBR);
			Gizmos.DrawLine(directionBR, directionBL);
			break;
			
		case CameraPathBezier.viewmodes.target:
			
			Transform target = bezier.target;
			if(target != null)
			{
				Gizmos.color = new Color(1.0f,0.0f,0.0f,0.75f);
				Gizmos.DrawRay(transform.position, (target.position-transform.position)*0.9f);
			}
			break;
			
		}
	}
	
	private void SetAnimationCurve()
	{

		_curve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));

	}
}
