## Description

I was able to borrow a HoloLens for 2 weeks. This is what I got so far. 

The idea was that you can place planets in your living room and control a plane in a way that it's orbiting around the planets. 

Actually after implementing it, I realized that it's extremely hard to control the plane with "real" physics enabled.

## Video on youtube:

[![Youtube](http://img.youtube.com/vi/oq26Q-cz4HY/0.jpg)](https://www.youtube.com/watch?v=oq26Q-cz4HY)

## How to run?
* Install HoloLens prerequisites ![Link](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools)
* Deploy the 'Orbiter'-App to the HoloLens
* Change your HoloLens-IP-Adress in the 'GamePadBridgeServer' and run it.

## Controls

![Controls](https://github.com/thomai-d/Orbiter/blob/master/img/gamepad.png)

## Technical information
* C# / UWP Application
* Engine: Urho3D ![Urho3D](https://urho3d.github.io/) using the ![UrhoSharp](https://github.com/xamarin/urho) bindings.
* The Plane has been modeled with ![Blender](https://www.blender.org).
* Sounds are borrowed from AgeOfEmpries2 as well as composed with ![Ableton Live](https://www.ableton.com/de/live/).
