using System;
using UnityEngine;

public class FDebugFrameRateGraph : FContainer
{
	private const int BASE_TEX_WIDTH = 128;
	private const int BASE_TEX_HEIGHT = 32;

	private const float IDEAL_FRAME_TIME = 1.0f / 60.0f;
	private const float MAX_FRAME_TIME = IDEAL_FRAME_TIME * 3.0f;

	private bool _useSmoothDeltaTime = false;

	private int _texWidth = 0;
	private int _texHeight = 0;
	private int _numPixels = 0;

	private int _targetRow = 0;
	private int _doubleTargetRow = 0;

	private Color[] _newFramePixels;

	private Texture2D _graphTex = null;
	private FSprite _graphSprite = null;

	private Color _blank = new Color(0, 0, 0, 0.5f);

	public FDebugFrameRateGraph() : base()
	{
		_texWidth = BASE_TEX_WIDTH;
		_texHeight = BASE_TEX_HEIGHT;
		_numPixels = _texWidth * _texHeight;

		_newFramePixels = new Color[_texHeight];

		_graphTex = new Texture2D(_texWidth, _texHeight, TextureFormat.ARGB32, false);
		_graphTex.filterMode = FilterMode.Point;
		// Initialize the graph
		Color[] blank = new Color[_numPixels];
		for (int i = 0; i < _numPixels; i++)
		{
			blank[i] = _blank;
		}
		_graphTex.SetPixels(blank);

		// Draw lines for the target frame rate
		_targetRow = Mathf.FloorToInt(IDEAL_FRAME_TIME / (MAX_FRAME_TIME / (float)_texHeight));
		_doubleTargetRow = Mathf.FloorToInt(2.0f * IDEAL_FRAME_TIME / (MAX_FRAME_TIME / (float)_texHeight));

		Color[] targetColor = new Color[_texWidth];
		Color[] doubleTargetColor = new Color[_texWidth];
		Color[] maxTargetColor = new Color[_texWidth];
		for (int i = 0; i < _texWidth; i++)
		{
			targetColor[i] = Color.black;
			doubleTargetColor[i] = Color.black;
			maxTargetColor[i] = Color.black;
		}
		_graphTex.SetPixels(0, _targetRow, _texWidth, 1, targetColor);
		_graphTex.SetPixels(0, _doubleTargetRow, _texWidth, 1, doubleTargetColor);
		_graphTex.SetPixels(0, _texHeight - 1, _texWidth, 1, maxTargetColor);
		_graphTex.Apply();

		Futile.atlasManager.LoadAtlasFromTexture("debugFrameGraph", _graphTex);
		_graphSprite = new FSprite("debugFrameGraph");
		_graphSprite.SetAnchor(0.0f, 0.0f);
		_graphSprite.scale = Futile.resourceScale;
		AddChild(_graphSprite);

		ListenForUpdate(HandleUpdate);
	}

	private void HandleUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (_useSmoothDeltaTime)
		{
			deltaTime = Time.smoothDeltaTime;
		}

		// Grab all the pixels except the first row and copy them one pixel to the left
		Color[] prevPixels = _graphTex.GetPixels(1, 0, _texWidth - 1, _texHeight);
		_graphTex.SetPixels(0, 0, _texWidth - 1, _texHeight, prevPixels);

		// Now draw the new frame time
		int frameRow = Mathf.FloorToInt(deltaTime / (MAX_FRAME_TIME / (float)_texHeight));
		Color frameColor = Color.red;
		if (frameRow <= _targetRow)
		{
			frameColor = Color.green;
		}
		else if (frameRow <= _doubleTargetRow)
		{
			frameColor = Color.yellow;
		}

		for (int y = 0; y <= frameRow && y < _texHeight; y++)
		{
			_newFramePixels[y] = frameColor;
		}
		for (int y = frameRow + 1; y < _texHeight; y++)
		{
			_newFramePixels[y] = _blank;
		}

		_newFramePixels[_targetRow] = Color.black;
		_newFramePixels[_doubleTargetRow] = Color.black;
		_newFramePixels[_texHeight - 1] = Color.black;

		_graphTex.SetPixels(_texWidth - 1, 0, 1, _texHeight, _newFramePixels);
		_graphTex.Apply();
	}
}

