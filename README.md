#Futile 0.2

Futile is a super simple 2D engine for Unity. 

This is for those of you who want to do everything in code and want as little editor integration as possible. 
It'll be especially familiar if you come from a background using Cocos2D or Flash.

It's still super rough, under development, and completely undocumented... but it works. 

Futile contains many ideas (as well as the MiniJSON parser) from Prime 31's UIToolkit: [github.com/Prime31/UIToolkit](http://github.com/Prime31/UIToolkit)

The demo project also uses Prime31's fantastic GoKit tweening library: [github.com/Prime31/GoKit](http://github.com/Prime31/GoKit)

___

##How to try the demo project##
_(There will be proper tutorials and stuff coming later, but for now this is it)_

####How to open the project
- Checkout the project from github and put it somewhere
- Run Unity
- Go to File -> Open Project -> Open Other -> and then choose the "BananaDemoProject" folder

####How to make sure you're running it at the right resolution
- Go to File -> Build Settings -> Click "PC and Mac Standalone" -> Click "Switch Platform" (if it's already greyed out, you're good)
- On the Build Settings page, choose Player Settings
- Under Resolution and Presentation, set the size to 960x640 (or 480x320, or 1024x768)
- Go to the "Game" tab 
- In the top left dropdown, choose your resolution (instead of "Free aspect")
- In the top right, make sure "Maximize on Play" is enabled.

Notes: 
- If you choose a specific resolution, but the game window isn't large enough to contain that resolution, it'll open in some random scaled resolution, and everything will be wonky, which is annoying. 
- If you're on OSX and you own iOS Basic or Pro, you can choose iOS instead of standalone



