# Oculus FRC Streamer
This project enables streaming Oculus headset (Quest 3 / Quest 3S) pose information to an FRC robot on the same network using Network Tables. The pose information can be used to accurately map the robot's surroundings in much the same way that a LimeLight or other FRC vision tracking device would. 

Using a VR headset has many benefits:
- Cheaper than most FRC SLAM solutions
- Fuses six calibrated SLAM cameras and an IMU to provide very accurate position and velocity estimates
- Provides a ~180 degree FoV and up to 120Hz odometry refresh rate
- Includes 12 CPU/GPU ARM cores, 8GB RAM, and 256GB onboard storage
- Well supported by many software platforms and includes off-the-shelf libraries for mapping a 3D environment

# Getting Started
Getting started requires two parts, the Unity application and the FRC robot code. Both are included in this repository. The sample app will also be included as a tag in case you want to give this a try without installing Unity. 

# Setting up the Unity Development Environment
## 1. Install Unity
- Download and install Unity Hub from the official website ([link](https://unity.com/download))
- Open Unity Hub, sign in, and install Unity 6 (6000.0.25f1) LTS
	- Select the following:
		- "Microsoft Visual Studio Community 2022"
		- Android Build Support"
		- "OpenJDK"
		- "Android SDK & NDK Tools"
- Click "Install" and wait for the installation to finish

# 2. Install Git for Windows
- Install Git using whatever method you prefer. You can download Git for Windows [here](https://git-scm.com/downloads/win).
# 3. Fork, clone, and import this repository into Unity
The main editing window will only open if a project is active. 
- Click `Add > Add project from disk` and select the unity subfolder in this repository
- Click on the newly imported project
- Wait for Unity to compile assets and open the main interface
# 4. Install the Git for Unity plugin
Installing Git for Unity will make managing the Unity source code much easier. 
- Follow the instructions on [this](https://github.com/spoiledcat/git-for-unity) page
- Git for Unity should detect your forked repository
# 5. Install the MessagePack plugin for Unity
This package is required by the C# Network Tables library. 
- Download the latest release with a `.unitypackage` extension from [here](https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases/tag/v2.5.187)
- In the main unity window, select `Assets > Import Package > Custom Package`
- Browse to the package you downloaded and click `Import`
# 6. Explore the C# Code
That's it! You'll find the core streamer application in `Assets > Robot > MotionStreamer.cs`. 