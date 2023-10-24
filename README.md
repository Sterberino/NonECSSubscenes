# NonECSSubscenes
A system for Subscenes in Unity that mimics Unity's ECS SubScenes, without using the Entity Component System. Not compatible with the Entities package. I made this because I wanted the organizational benefits Unity's SubScene system brought without having to convert my current project to ECS.

## How To use
Make sure that the SubSceneEditor script is located in your Editor folder. More information can be found [here](https://docs.unity3d.com/Manual/SpecialFolders.html). You can create a new Subscene using the following static methods:

- ```SubScene.CreateEmptySubscene()```
- ```SubScene.CreateSubsceneFromGameobject()```
- ```SubScene.CreateSubsceneFromGameobjects()```

You can also right click on GameObjects in the Hierarchy of an existing Scene, and select the Menu Option: **New Subscene From Selection**.

### Opening and Closing SubScenes
Opening and closing the SubScenes is simple.

```
  SubScene subScene;
  subScene.OpenSubscene();
  subScene.CloseSubscene(saveFlag);
```

Please remember that scene loading is Asynchronous, and read the comments on these methods for more information.

## Known Issues
1. There's currently a reflection issue causing an error to be logged to the Unity Console. It doesn't seem to cause any other issues.
2. Sometimes, saving a Scene with an active SubScene prompts a Unity Editor Error Message claiming that the Scene was unable to be saved. Clicking "Try Again" or "Cancel" successfully saves the Scene.
3. Nested SubScenes are not currently tested and not supported (at least insofar as the editor scripting portion is concerned).

## Screens
<img width="447" alt="Hierarchy" src="https://github.com/Sterberino/NonECSSubscenes/assets/91395511/f12c5529-f06d-44f2-889d-0865c288448f">
<br/>
<img width="438" alt="Scene_View" src="https://github.com/Sterberino/NonECSSubscenes/assets/91395511/87279de8-421b-4de7-b3d2-d0c5bd783dee">
<br/>
<img width="184" alt="Inspector" src="https://github.com/Sterberino/NonECSSubscenes/assets/91395511/93750e79-e230-4448-a028-566cc6d7988e">

## References
For drawing the Tree Structure, I referenced [Pretty Hierarchy](https://github.com/NCEEGEE/PrettyHierarchy), StackOverflow, and the Unity Forums.
