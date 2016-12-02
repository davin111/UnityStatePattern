using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CapsuleActor : MonoBehaviour
{
	public enum eBtn{
		BtnA,
		BtnS,
		BtnD,
		BtnW,
		BtnSpace,
		None
	}

	delegate void TypeA();

	TypeA DelUpdateJump;
	TypeA DelUpdateJumpCp;
	Transform _tr;
	Vector3 _lookTarget = Vector3.forward;
	float _damping = 10f;
	Vector3[] dirList = new Vector3[]{new Vector3(-1.0f, 0f, 0f), new Vector3(1.0f, 0f, 0f), new Vector3(0.0f, 0f, 1f), new Vector3(0.0f, 0f, -1f)}; //adws

	int _colCount;

	void Start()
	{
		_tr = gameObject.transform;
		DelUpdateJump = null;
		DelUpdateJumpCp = new TypeA(UpdateJump);

		StartCoroutine(UpdateMonster());
	}

	void OnTriggerEnter (Collider other)
	{
		if(other.CompareTag("Enemy")) 
		{
			_colCount++;
		}

	}

	IEnumerator UpdateMonster()
	{
		while(true)
		{
			bool pressed = false;
			bool shouldJump = false;
			if(pressed = IsPressed(eBtn.BtnA, ref shouldJump)) _lookTarget = dirList[0];
			else if(pressed = IsPressed(eBtn.BtnD, ref shouldJump)) _lookTarget = dirList[1];
			else if(pressed = IsPressed(eBtn.BtnW, ref shouldJump)) _lookTarget = dirList[2];
			else if(pressed = IsPressed(eBtn.BtnS, ref shouldJump)) _lookTarget = dirList[3];

			UpdateLookAt();
			UpdateMove(pressed);
			if(shouldJump && !_isJumping)
			{
				shouldJump = false;
				_isJumping = true;
				DelUpdateJump = DelUpdateJumpCp;
				_jumpT = 0f;
			}
			if(null != DelUpdateJump) DelUpdateJump();

			yield return null;
		}

	}

	void UpdateMove(bool isMove)
	{
		if(isMove)
		{
			_tr.position = _tr.position + (_lookTarget * 0.1f);
		}
	}

	float accelG = 9.8f * 2f;
	float _jumpT = 0f;
	bool _isJumping = false;
	void UpdateJump()
	{
		float newY = 0.5f + accelG * _jumpT - 7.5f * 0.5f * accelG * _jumpT * _jumpT;
		if(newY < 0.5f)
		{
			_isJumping = false;
			newY = 0.5f;
			DelUpdateJump = null;
		}
		_tr.position = new Vector3(_tr.position.x, newY, _tr.position.z);
		_jumpT += Time.deltaTime * 0.5f;
	}

	void UpdateLookAt()
	{
		//Look at and dampen the rotation
		Quaternion rotation = Quaternion.LookRotation(_lookTarget);
		_tr.rotation = Quaternion.Slerp(_tr.rotation, rotation, Time.deltaTime * _damping);
	}

	bool IsPressed(eBtn btn, ref bool isJump)
	{
		eBtn pressedBtn = eBtn.None;

		if(Input.GetKey("a") || Input.GetKey("left"))
			pressedBtn = eBtn.BtnA;
		else if(Input.GetKey("s") || Input.GetKey("down"))
			pressedBtn = eBtn.BtnS;
		else if(Input.GetKey("d") || Input.GetKey("right"))
			pressedBtn = eBtn.BtnD;
		else if(Input.GetKey("w") || Input.GetKey("up"))
			pressedBtn = eBtn.BtnW;
		if(Input.GetKey("space"))
			isJump = true;

		return (btn == pressedBtn);
	}
}
