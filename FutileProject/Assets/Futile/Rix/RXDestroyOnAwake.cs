
using System;
using UnityEngine;

//put this on a root transform in the editor where you want to view stuff, but not actually have it stay in the game

public class RXDestroyOnAwake : MonoBehaviour
{
	public void Awake()
	{
		UnityEngine.Object.Destroy(gameObject);
	}
}

