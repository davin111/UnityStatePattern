using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class Monster : MonoBehaviour
{
	public enum eState{
		Patrol,
		Investigate,
		Chase,
		None
	}

	private eState state_;
	public eState _state
	{
		get
		{
			return state_;
		}
		set
		{
			ExitState(state_);
			state_ = value;
			EnterState(state_);
		}
	}

	private Job hear;
	private Job see;	
	private Job patrol;	
	private Job investigate;

	private NavMeshAgent guard;
	private float patrolSpeed = 3.5f;
	private float alertSpeed = 5.0f;
	private float playerSpeed = 0f;
	public Transform playerTr;	
	public List<Transform> patrolPoints = new List<Transform>();
	Vector3 suspiciousPosition;

	Transform _tr;
	float _damping = 0.5f;
	Vector3 _lookTarget = Vector3.forward;
	GameObject[] _effList = new GameObject[3];
	Text _stateText;
	void Start()
	{
		_tr = gameObject.transform;
		playerTr = GameObject.Find("Capsule").transform;

		GameObject mon = GameObject.Find("monster1");
		_effList[0] = mon.transform.FindChild("eA").gameObject;
		_effList[1] = mon.transform.FindChild("eB").gameObject;
		_effList[2] = mon.transform.FindChild("eC").gameObject;

		guard = gameObject.GetComponent<NavMeshAgent>();

		patrolPoints.Add(GameObject.Find("patrolP1").transform);
		patrolPoints.Add(GameObject.Find("patrolP2").transform);
		patrolPoints.Add(GameObject.Find("patrolP3").transform);
		patrolPoints.Add(GameObject.Find("patrolP4").transform);
	
		_stateText = GameObject.Find("HUDCanvas/Panel/state").GetComponent<Text>();

		_state = eState.Patrol;
	}

	void SetUIText()
	{
		_stateText.text = "State : " + _state.ToString();
	}

	void SetEffect()
	{
		for(int i=0; i<_effList.Length;i++)
			_effList[i].SetActive(false);
		if(_state != eState.None)
			_effList[(int)_state].SetActive(true);
	}

	void EnterState(eState state)
	{
		switch(state)
		{
		case eState.Patrol:
			patrol = new Job(Patrolling(),true);
			SetEffect();
			see = new Job(Seeing(playerTr, 45f,60f,10f,0.5f,true,
				() => 
				{	
					Debug.Log ("Saw you!");

					_state = eState.Chase;

				}),true); 

			hear = new Job(Hearing(
				() =>
				{
					Debug.Log ("What was that?");

					_state = eState.Investigate;

				}),true);

			break;
		case eState.Investigate:
			SetEffect();
			investigate = new Job(Investigating(
				() =>
				{
					_state = eState.Patrol;
				}
			),true);

			see = new Job(Seeing(playerTr, 45f,60f,10f,0.5f,true,
				() => 
				{	
					Debug.Log ("Saw you!");
					_state = eState.Chase;

				}),true);			

			break;

		case eState.Chase:
			guard.speed = alertSpeed;
			SetEffect();
			see = new Job(Seeing(playerTr, 45f,60f,10f,0.5f,false,
				() => 
				{	
					Debug.Log ("Where you gone?");
					_state = eState.Investigate;

				}),true);				

			break;
		}

		SetUIText();
	}

	void ExitState(eState state)
	{
		switch(state)
		{
		case eState.Patrol:

			if(patrol != null) patrol.kill();
			if(see != null) see.kill();
			if(hear != null) hear.kill();

			break;
		case eState.Investigate:

			if(investigate != null) investigate.kill();
			if(see != null) see.kill();

			break;
		case eState.Chase:

			if(see != null) see.kill();

			break;
		}
	}

	IEnumerator Patrolling()
	{
		int i = 0;

		while(true)
		{
			guard.speed = patrolSpeed;

			guard.SetDestination(patrolPoints[i].position);

			while((_tr.position - guard.destination).sqrMagnitude > 2f)
			{
				yield return null;
			}

			if(i == patrolPoints.Count - 1)
				i = 0;
			else
				++i;			

			guard.speed = 0f;

			yield return new WaitForSeconds(1f);			
		}
	}

	IEnumerator Investigating(Action OnComplete)
	{
		suspiciousPosition = playerTr.position;

		while(true)
		{
			guard.speed = 0f;
			yield return new WaitForSeconds(1f);			

			guard.SetDestination(suspiciousPosition);

			guard.speed = alertSpeed;

			while((_tr.position - guard.destination).sqrMagnitude > 2f)
			{
				yield return null;
			}  

			guard.speed = 0f;

			yield return new WaitForSeconds(1f);

			if(OnComplete != null) OnComplete();
		}
	}


	IEnumerator Seeing(Transform target, float angle, float distance, float maxHeight, float time, bool inRange, Action OnComplete)
	{		
		while(true)
		{		
			float timer = 0f;

			if(inRange)
			{
				while(IsInFov(target, angle, maxHeight) && (VisionCheck(target,distance)) && timer < time) 
				{			
					timer += Time.deltaTime;			
					yield return null;			
				}
			}
			else if(!inRange)
			{
				while((!IsInFov(target, angle, maxHeight) || !VisionCheck(target,distance)) && timer < time) 
				{			
					timer += Time.deltaTime;			
					yield return null;			
				}			
			}

			if(timer > time && OnComplete != null) OnComplete(); 

			yield return null;
		}
	}	

	IEnumerator Hearing(Action onComplete)
	{
		while(true)
		{			
			float hearingRange = 10f;

			bool heardNoise = false;

			while(!heardNoise && (_tr.position - playerTr.position).sqrMagnitude < hearingRange*hearingRange && playerSpeed > 20f)
			{
				heardNoise = true;
			}

			if(heardNoise && onComplete != null) onComplete();

			yield return null;
		}
	}	


	public bool VisionCheck(Transform target, float distance)
	{
		RaycastHit hit;

		if(Physics.Raycast(_tr.position, target.position-_tr.position,out hit,distance))
		{
			if(hit.transform == playerTr) return true;
			else return false;
		}
		else return false;
	}	

	public bool IsInFov(Transform target, float angle, float maxHeight)
	{
		var relPos = target.position - _tr.position; 
		float height = relPos.y;
		relPos.y = 0;

		if(Mathf.Abs(Vector3.Angle(relPos,transform.forward)) < angle)
		{
			if(Mathf.Abs(height) < maxHeight)
			{				
				return true;
			}
			else
			{				
				return false;
			}			
		}
		else return false;
	}
}
