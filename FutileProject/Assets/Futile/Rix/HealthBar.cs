using System;
using UnityEngine;

public class HealthBar : FContainer
{
    private const float INSET = 1.0f;
	private const float DOUBLE_INSET = INSET*2.0f;
	
	private static Color BAD_COLOR = Color.red;
	private static Color OKAY_COLOR = Color.yellow;
	private static Color GOOD_COLOR = Color.green;
	
	private FSprite _background;
	private FSprite _bar;
	private float _width;
	private float _height;
	
	private float _percentage;
	
	public float offsetX;
	public float offsetY;
	
	private bool _isShown = false;

	private float _redness = 0;
	private float _blueness = 0;

	private float _oldPercentage = 1.0f;

	private float _timeUntilHidden = 0.0f;

	public HealthBar (float offsetX, float offsetY, float width, float height, float percentage)
	{
		this.offsetX = offsetX;
		this.offsetY = offsetY;
		
		_width = width;
		_height = height;
		
		_background = new FSprite("Futile_White");
		
		_background.width = _width;
		_background.height = _height;
		
		_bar = new FSprite("Futile_White");
		
		_bar.height = _height - DOUBLE_INSET;
		_bar.anchorX = 0.0f;
		_bar.x = -_width*0.5f + INSET;
		
		_background.color = new Color(0.15f,0.22f,0.35f);
		
		_percentage = Mathf.Clamp01(percentage);

		this.alpha = 0.0f;
		
		UpdatePercentage();

		ListenForUpdate(HandleUpdate);
	}

	private void HandleUpdate()
	{
		bool needsColorChange = false;
		if(_redness > 0)
		{
			_redness -= 0.02f;
			needsColorChange = true;
		}

		if(_blueness > 0)
		{
			_blueness -= 0.02f;
			needsColorChange = true;
		}

		if(needsColorChange)
		{
			_background.color = new Color(_redness, 0.1f, 0.25f + 0.75f*_blueness);
		}


		//this code handles the fading in and out of the healthbar

		if(_percentage < 1.0f)
		{
			this.alpha += 0.1f;

			_timeUntilHidden = 2.0f;
		} 
		else
		{
			if(_timeUntilHidden > 0)
			{
				_timeUntilHidden -= Time.deltaTime;
			} 
			else 
			{
				this.alpha -= 0.025f;
			}
		}

		if(this.alpha < 0.05f)
		{
			if(_isShown)
			{
				_isShown = false;
				RemoveChild(_background);
				RemoveChild(_bar);	
			}
		} 
		else
		{
			if(!_isShown)
			{
				_isShown = true;
				AddChild(_background);
				AddChild(_bar);
			}
		}
	}
	
	private void UpdatePercentage()
	{
		if(_percentage < _oldPercentage) //hurt!
		{
			_redness = Mathf.Min(1.0f, _redness + 0.8f);
		}

		if(_percentage > _oldPercentage) //healed!
		{
			_blueness = Mathf.Min(1.0f, _blueness + 0.8f);
		}

		_oldPercentage = _percentage;

		if(_percentage < 0.33f)
		{
			_bar.color = BAD_COLOR;	
		}
		else if(_percentage < 0.66)
		{
			_bar.color = OKAY_COLOR;	
		}
		else
		{
			_bar.color = GOOD_COLOR;	
		}
		
		_bar.width = (_width - DOUBLE_INSET) * _percentage;	
	}
	
	public float percentage
	{
		get { return _percentage;}
		
		set 
		{
			value = Mathf.Clamp01(value);
			if(_percentage != value)
			{
				_percentage = value;
				UpdatePercentage();
			}
		}
	}
	
}

