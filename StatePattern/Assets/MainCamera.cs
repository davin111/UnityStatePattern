using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCamera : MonoBehaviour
{
	Transform _tr;
	float _tCamera;

	void Start()
	{
		_tr	 			= transform;
		_tCamera 		= 0f;
	}
		
	void Update()
	{
		/*
		_tCamera 		+= 0.1f*Time.deltaTime; _tCamera = _tCamera>1.0f ? 0 : _tCamera;
		_tr.position 	= _curve.GetSplinePosition(_tCamera);
		_tr.LookAt(Vector3.zero);
		*/
	}
}
