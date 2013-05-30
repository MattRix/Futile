using UnityEngine;
using System.Collections;
using System;

public class BTestPage : BPage
{
    private FSprite _background;
    private FButton _startButton;

    public BTestPage()
    {
        ListenForUpdate(HandleUpdate);
    }

    private FSprite _banana;
    private FLabel _testLabel;
    
    override public void Start()
    {
        _background = new FSprite("JungleClearBG");
        AddChild(_background);
        
        //this will scale the background up to fit the screen
        //but it won't let it shrink smaller than 100%
      
        
        _startButton = new FButton("YellowButton_normal", "YellowButton_down", "YellowButton_over", "ClickSound");
        _startButton.AddLabel("Franchise","START",new Color(0.45f,0.25f,0.0f,1.0f));
        _startButton.y = -Futile.screen.halfHeight + 50.0f;
        AddChild(_startButton);
        
        _startButton.SignalRelease += HandleStartButtonRelease;

        _banana = new FSprite("Banana");
        AddChild(_banana);

        FStage superStage = new FStage("SuperStage");
        Futile.AddStage(superStage);

        superStage.layer = 31;

        _testLabel = new FLabel("Franchise", "1");
        _testLabel.y = Futile.screen.halfHeight - 50.0f;
        _testLabel.color = Color.blue;
        superStage.AddChild(_testLabel);

        Futile.instance.camera.cullingMask = 31;
    }

    private void HandleStartButtonRelease (FButton button)
    {
        BMain.instance.GoToPage(BPageType.InGamePage);
    }
    
    protected void HandleUpdate ()
    {
        int count = 3;
        int index = Time.frameCount % count;

        _banana.x = -Futile.screen.halfWidth + index * (Futile.screen.width / (float)(count - 1));

        _testLabel.text = "ITEM " + index;
    }
    
}

