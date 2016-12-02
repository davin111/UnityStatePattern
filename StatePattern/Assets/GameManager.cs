using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
	void Awake()
	{
		if (instance == null)
			instance = this;

		else if (instance != this)
			Destroy(gameObject);    

		DontDestroyOnLoad(gameObject);

		InitGame();
	}

	//Initializes the game for each level.
	void InitGame()
	{
		
	}

	void Update()
	{

	}

	public void DoHello()
	{
		Debug.Log("Hello");
	}
}

