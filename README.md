#Futile (0.92.0 Beta)

Futile is a code-centric 2D framework for Unity. 

This is for those of you who want to do everything in code with as little editor integration as possible. 

If you've used Cocos2D or Flash you should feel right at home.

It's in development and completely undocumented... but it works. 
___

##Go to http://struct.ca/futile for UnityPackages and instructional videos

##Ask questions and share stuff you've made on http://reddit.com/r/futile

##Submit bugs and feature requests to http://github.com/MattRix/Futile/issues

##Futile works great with Unity 3.5 and 4.*

##How to try the demo project:##

####How to open the project

- Grab the project from github and put it somewhere - [For the lazy, here's a zip of the whole repo](https://github.com/MattRix/Futile/zipball/master)
- Make sure you have Unity installed
- Go into FutileProject/Assets and open FutileScene.unity

####How to make sure you're running it at the right resolution
- Go to File -> Build Settings -> Click "PC and Mac Standalone" -> Click "Switch Platform" (if it's already greyed out, you're good)
- On the Build Settings page, choose Player Settings
- Under Resolution and Presentation, set the size to 960x640 (or 480x320, or 1024x768)
- Go to the "Game" tab 
- In the top left dropdown, choose your resolution (instead of "Free aspect")
- In the top right, make sure "Maximize on Play" is enabled.

Notes: 
- If you choose a specific resolution, but the game window isn't large enough to contain that resolution, Unity will open in some random scaled resolution, and everything will be wonky, which is annoying. 
- If you're on OSX and you own iOS Basic or Pro, you can choose iOS instead of standalone

##Third Party add-ons for Futile

- https://github.com/ManaOrb/FSceneManager (Futile Scene Manager and Parallax Scrolling Layer)
- https://github.com/mattfox12/FutileAdditionalClasses (including animated sprites and TMX tilemaps)
- https://github.com/Grizzlage/Futile-SpineSprite (for using animations made with Spine)
- https://gist.github.com/jpsarda/4573831 (FDrawingSprite.cs, for drawing lines)


##Legal stuff##

Futile contains many ideas from Prime 31's UIToolkit: [github.com/Prime31/UIToolkit](http://github.com/Prime31/UIToolkit)

The MiniJSON parser is by http://github.com/darktable

The demo project also uses Prime31's fantastic GoKit tweening library: [github.com/Prime31/GoKit](http://github.com/Prime31/GoKit)

####The code and art assets (except for the font) can be used for anything, however the sound effects and music are not to be used in anything else
####GoKit's license is here: https://github.com/prime31/GoKit
####The font is [Franchise](http://www.losttype.com/font/?name=franchise)

##MIT License##

Source code for Futile is Copyright © 2013 Matt Rix and contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS,” WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

