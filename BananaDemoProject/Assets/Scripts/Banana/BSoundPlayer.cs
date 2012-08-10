using System;
using UnityEngine;


public class BSoundPlayer
{

	static public void PlayBananaSound()
	{
		FSoundManager.PlaySound("BananaSound", 0.95f);
	}
	
	static public void PlayVictoryMusic() 
	{
		FSoundManager.PlayMusic("VictorySound", 0.2f);
	}
	
	static public void PlayRegularMusic()
	{
		FSoundManager.PlayMusic("Music", 0.3f);
	}
	
}

