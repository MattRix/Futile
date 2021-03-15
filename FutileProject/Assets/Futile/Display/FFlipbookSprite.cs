using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FFlipbookSprite : FSprite
{
	FAtlasElement[] _flipbookElements = null;
	int[] _flipbookIndices = null;
	int _curIndex = 0;
	float _timer = 0f;
	bool _isPlaying = true;

	public float frameTime = 0.25f; // 4 fps default
	public bool shouldUseRandomFrames = false;


	public FFlipbookSprite(string defaultElementName) : base(defaultElementName)
	{
		ListenForUpdate(HandleUpdate);
	}

	public FFlipbookSprite(params string[] elementNames) : base(elementNames[0])
	{
		SetFlipbookSprites(elementNames);
		ListenForUpdate(HandleUpdate);
	}

	public void SetFlipbookSprites(params string[] elementNames)
	{
		_flipbookElements = new FAtlasElement[elementNames.Length];
		_flipbookIndices = new int[elementNames.Length];
		for (int i = 0; i < elementNames.Length; i++)
		{
			_flipbookElements[i] = Futile.atlasManager.GetElementWithName(elementNames[i]);
			_flipbookIndices[i] = i;
		}
		_curIndex = 0;
		element = _flipbookElements[0];
	}

	public void OverrideFlipbookFrames(params int[] frameIndices)
	{
		_flipbookIndices = frameIndices;
	}

	public void SetIsPlaying(bool play)
	{
		_isPlaying = play;
	}

	void HandleUpdate()
	{
		if (!_isPlaying) return;

		_timer += Time.deltaTime;
		while (_timer >= frameTime)
		{
			_timer -= frameTime;

			if (shouldUseRandomFrames)
			{
				var nextIndex = RXRandom.Int(_flipbookIndices.Length);
				while (nextIndex == _curIndex)
				{
					nextIndex = RXRandom.Int(_flipbookIndices.Length);
				}
				_curIndex = nextIndex;
			}
			else
			{
				_curIndex = (_curIndex + 1) % _flipbookElements.Length;
			}
			element = _flipbookElements[_flipbookIndices[_curIndex]];
		}
	}
}
