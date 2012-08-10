using System;
using UnityEngine;
using System.Collections.Generic;


public class FSoundManager
{
	static private String _resourcePrefix;
	
	static private GameObject _gameObject;
	static private AudioListener _listener;
	static private AudioSource _soundSource;
	static private AudioSource _musicSource;
	
	static private Dictionary<string, AudioClip> _soundClips;
	private static AudioClip _currentMusicClip;

	static FSoundManager ()
	{
		_gameObject = new GameObject("FSoundManager");
		_musicSource = _gameObject.AddComponent<AudioSource>();
		_soundSource = _gameObject.AddComponent<AudioSource>();
		
		_gameObject.AddComponent<AudioListener>(); //we don't need a reference to it, we just need it to exist
		
		_resourcePrefix = "Audio/";
		_soundClips = new Dictionary<string, AudioClip>();
		_currentMusicClip = null;
	}

	static public void SetResourcePrefix (String prefix)
	{
		_resourcePrefix = prefix;
	}

	static public void PlaySound (String resourceName, float volume)
	{
		string fullPath = _resourcePrefix+resourceName;
		
		AudioClip soundClip;
		
		if(_soundClips.ContainsKey(fullPath))
		{
			soundClip = _soundClips[fullPath];
		}
		else
		{
			soundClip = Resources.Load(fullPath) as AudioClip;	
			_soundClips[fullPath] = soundClip;
		}
		
		_soundSource.PlayOneShot(soundClip, volume);
	}

	static public void PlaySound (String resourceName)
	{
		PlaySound(resourceName,1.0f);
	}

	static public void PlayMusic (string resourceName, float volume)
	{
		string fullPath = _resourcePrefix+resourceName;
		
		if(_currentMusicClip != null) //we only want to have one music clip in memory at a time
		{
			_musicSource.Stop();
			Resources.UnloadAsset(_currentMusicClip);	
			_currentMusicClip = null;
		}
		
		_currentMusicClip = Resources.Load(fullPath) as AudioClip;
		_musicSource.clip = _currentMusicClip;
		_musicSource.volume = volume;
		_musicSource.loop = true;
		_musicSource.Play();
	}

	static public void PlayMusic (string resourceName)
	{
		PlayMusic(resourceName,1.0f);
	}

	static public void StopMusic ()
	{
		_musicSource.Stop();
	}
	
	static public void UnloadAllSounds()
	{
		foreach(AudioClip audioClip in _soundClips.Values)
		{
			Resources.UnloadAsset(audioClip);
		}
		
		_soundClips.Clear();
	}
	
	static public void UnloadAllSoundsAndMusic()
	{
		UnloadAllSounds();
		
		if(_currentMusicClip)
		{
			Resources.UnloadAsset(_currentMusicClip);
		}
		
		_currentMusicClip = null;
	}
}

