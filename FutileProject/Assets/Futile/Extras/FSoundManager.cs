using System;
using UnityEngine;
using System.Collections.Generic;

public class FSoundManager
{
	public const string SFX_PREFS_KEY = "FSoundManager_IsSFXAudioMuted";
	public const string MUSIC_PREFS_KEY = "FSoundManager_IsMusicAudioMuted";
	static public string resourcePrefix = "Audio/";
	
	static private GameObject _gameObject;
	static private AudioSource _soundSource;
	static private AudioSource _musicSource;
	static private string _currentMusicPath = "";
	static private bool _isSFXMuted = false;
	static private bool _isMusicMuted = false;

	static private Dictionary<string, AudioClip> _soundClips = new Dictionary<string, AudioClip>();
	static private AudioClip _currentMusicClip = null;
	
	static private float _sfxVolume = 1.0f;
	static private float _musicVolume = 1.0f;
	
	static public void Init()
	{
		Debug.Log("THIS INIT SHOULD NOT HAPPEN!");
		if(_gameObject != null) return; //no multiple inits

		_gameObject = new GameObject("FSoundManager");
		_musicSource = _gameObject.AddComponent<AudioSource>();
		_soundSource = _gameObject.AddComponent<AudioSource>();
		_gameObject.AddComponent<AudioListener>(); //we don't need a reference to it, we just need it to exist
		
		if(PlayerPrefs.HasKey(SFX_PREFS_KEY))
		{
			FSoundManager.isSFXMuted = (PlayerPrefs.GetInt(SFX_PREFS_KEY) == 1);
		}
		if(PlayerPrefs.HasKey(MUSIC_PREFS_KEY))
		{
			FSoundManager.isMusicMuted = (PlayerPrefs.GetInt(MUSIC_PREFS_KEY) == 1);
		}
	}
	
	static public void SetResourcePrefix (String prefix)
	{
		resourcePrefix = prefix;
	}

	static public void PreloadSound (String resourceName)
	{
		string fullPath = resourcePrefix+resourceName;

		if(_soundClips.ContainsKey(fullPath))
		{
			return; //we already have it, no need to preload it again!
		}
		else
		{
			AudioClip soundClip = Resources.Load(fullPath) as AudioClip;	

			if(soundClip == null)
			{
				Debug.Log("Couldn't find sound at: " + fullPath);
			}
			else
			{
				_soundClips[fullPath] = soundClip;
			}
		}
	}

	static public void PlaySound (String resourceName, float volume) //it is not necessary to preload sounds in order to play them
	{
		if (_isSFXMuted) return;

		if (_soundSource == null) Init();
		
		string fullPath = resourcePrefix+resourceName;

		AudioClip soundClip;
		
		if(_soundClips.ContainsKey(fullPath))
		{
			soundClip = _soundClips[fullPath];
		}
		else
		{
			soundClip = Resources.Load(fullPath) as AudioClip;	

			if(soundClip == null)
			{
				Debug.Log("Couldn't find sound at: " + fullPath);
				return; //can't play the sound because we can't find it!
			}
			else
			{
				_soundClips[fullPath] = soundClip;
			}
		}
		
		_soundSource.PlayOneShot(soundClip, volume);
	}

	static public void PlaySound (String resourceName)
	{
		PlaySound(resourceName,1.0f);
	}

	static public void PlayMusic (string resourceName, float volume)
	{
		PlayMusic(resourceName,volume,true);
	}

	static public void PlayMusic (string resourceName, float volume, bool shouldRestartIfSameSongIsAlreadyPlaying)
	{
		if (_isMusicMuted) return;

		if (_musicSource == null) Init();
		
		string fullPath = resourcePrefix+resourceName;
		_musicVolume = volume;

		if(_currentMusicClip != null) //we only want to have one music clip in memory at a time
		{
			if(_currentMusicPath == fullPath) //we're already playing this music, just restart it!
			{
				if(shouldRestartIfSameSongIsAlreadyPlaying)
				{
					_musicSource.Stop();
					_musicSource.volume = volume;
					_musicSource.loop = true;
					_musicSource.Play();
				}
				return;
			}
			else //unload the old music
			{
				_musicSource.Stop();
				Resources.UnloadAsset(_currentMusicClip);	
				_currentMusicClip = null;
				_currentMusicPath = "";
			}
		}
		
		_currentMusicClip = Resources.Load(fullPath) as AudioClip;

		if (_currentMusicClip == null)
		{
			Debug.Log("Error! Couldn't find music clip " + fullPath);
		}
		else 
		{
			_currentMusicPath = fullPath;
			_musicSource.clip = _currentMusicClip;
			_musicSource.volume = volume;
			_musicSource.loop = true;
			_musicSource.Play();
		}
	}

	static public void PlayMusic (string resourceName)
	{
		PlayMusic(resourceName,1.0f);
	}

	static public void StopMusic ()
	{
		if (_musicSource != null)
		{
			_musicSource.Stop();
		}
	}

	static public void UnloadSound (String resourceName)
	{
		string fullPath = resourcePrefix+resourceName;

		if(_soundClips.ContainsKey(fullPath)) //check if we have it
		{
			AudioClip clip = _soundClips[fullPath];
			Resources.UnloadAsset(clip);
			_soundClips.Remove(fullPath);
		}
	}

	static public void UnloadMusic()
	{
		if(_currentMusicClip != null)
		{
			Resources.UnloadAsset(_currentMusicClip);
			_currentMusicClip = null;
			_currentMusicPath = "";
		}
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
		UnloadMusic();
	}

	static public GameObject gameObject
	{
		get {return _gameObject;}
	}

	static public AudioSource soundSource
	{
		get {return _soundSource;}
	}

	static public AudioSource musicSource
	{
		get {return _musicSource;}
	}
	
	static public float sfxVolume
	{
		set 
		{ 
			_sfxVolume = value;
			
			if(_isSFXMuted)
			{
				_soundSource.volume = 0.0f;
			}
			else 
			{
				_soundSource.volume = _sfxVolume; 
			}
		}
		get { return _soundSource.volume; }
	}

	static public float musicVolume
	{
		set 
		{ 
			_musicVolume = value;
			
			if(_isMusicMuted)
			{
				_musicSource.volume = 0.0f;
			}
			else 
			{
				_musicSource.volume = _musicVolume; 
			}
		}
		get { return _musicSource.volume; }
	}
	
	static public bool isSFXMuted
	{
		set 
		{ 
			_isSFXMuted = value;

			PlayerPrefs.SetInt(SFX_PREFS_KEY, value ? 1 : 0);
			
			if(_isSFXMuted)
			{
				_soundSource.volume = 0.0f;
			}
			else 
			{
				_soundSource.volume = _sfxVolume; 
			}
		}
		get  {return _isSFXMuted;}
	}

	static public bool isMusicMuted
	{
		set 
		{ 
			_isMusicMuted = value;
			
			PlayerPrefs.SetInt(MUSIC_PREFS_KEY, value ? 1 : 0);
			
			if(_isMusicMuted)
			{
				_musicSource.volume = 0.0f;
				_musicSource.Pause();
			}
			else 
			{
				_musicSource.volume = _musicVolume; 
				_musicSource.Play();
			}
		}
		get  {return _isMusicMuted;}
	}
}

