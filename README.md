# QuestNav
This project enables streaming Oculus VR headset pose information to an FRC robot using the Network Tables 4 (NT4) protocol. This pose information can be used by the robot control system to accurately map the it's surroundings and navigate around a competition field, practice space, or any other location. QuestNav produces a more stable and reliable tracking pose than other FRC vision solutions (LimeLight, Photon Vision, etc.)

Using a VR headset for robot localization has many benefits:
- Lower cost than most FRC vision solutions 
- Multiple SLAM cameras enable redundant, calibrated VIO
- Camera tracking is fused with an onboard IMU to provide accurate position and velocity estimates
- Up to 120Hz robot odometry refresh rate (linked to the Oculus headset framerate)
- The [Qualcomm XR2G2 platform](https://www.qualcomm.com/products/mobile/snapdragon/xr-vr-ar/snapdragon-xr2-gen-2-platform) offers several CPU/GPU cores, 8GB RAM, dedicated video encode/decode hardware, and at least 128GB onboard storage
- Well-supported ecosystem and API that enables off-the-shelf libraries for mapping a 3D environment
- Self-contained, rechargeable battery

# Hardware Requirements
QuestNav requires the following to get started:
1. FRC robot and/or control system
	- *This project was tested using REV swerve modules with REV NEOs and Spark Max motor controllers.*
2. Quest 3S headset
	- *A Quest 3S headset is recommended since it is lower cost and offers excellent tracking performance. It's currently not clear whether the depth projector on the Quest 3 benefits FRC applications.*
3. USB C to Ethernet adapter dongle
	- *I'm actively working on compiling a list of known-good dongles. Please submit a pull request if you found one that works with your headset!*

# Software Flow
A high-level overview of the software architecture is shown below.

![QuestNav Software Block Diagram](docs/QuestNav-Example-Flow.png)

# Testing QuestNav
At it's heart, QuestNav is merely a VR app designed to push data to Network Tables. However, some one-time setup is required before we're able to push a custom app to a VR headset. 

## One-Time Setup
1. Follow [this](https://medium.com/sidequestvr/how-to-turn-on-developer-mode-for-the-quest-3-509244ccd386) guide to set up "Developer Mode" 
2. Sign into your Meta developer account and download the [Meta Quest Developer Hub (MQDH)](https://developers.meta.com/horizon/develop)
3. Configure the following settings on your Quest headset:
	- Set the display timeout to 4 hours
	- Enable the following developer settings under `Experimental Settings > Advanced`
		- Disable guardian for development purposes
4. Plug the headset into your PC and install the example .apk using MQDH
	- **NOTE!** The example app team number is hard-coded to 9999

## Once-Per-Boot Setup
These setup steps are required *once per boot* and can be prevented by ensuring your headset remains powered on using an external battery.
1. Plug the USB-Ethernet adapter into the USB port on the Quest headset
2. Start the QuestNav app using MQDH or by selecting the app icon in the launcher
3. Check that the Quest headset has connected to your robot and is writing pose data. 

# Unity Development Environment Setup
### Install Unity
- Download and install Unity Hub from the official website ([link](https://unity.com/download))
- Open Unity Hub, sign in, and install Unity 6 (6000.0.25f1) LTS
	- Select the following:
		- "Microsoft Visual Studio Community 2022"
		- Android Build Support"
		- "OpenJDK"
		- "Android SDK & NDK Tools"
- Click "Install" and wait for the installation to finish

### Install Quest Link

- Install the Quest Link software ([link](https://www.meta.com/help/quest/articles/headsets-and-accessories/oculus-rift-s/install-app-for-link/))

### Install Git for Windows

- Install Git using whatever method you prefer. You can download Git for Windows [here](https://git-scm.com/downloads/win).

### Fork, clone, and import this repository into Unity

The main editing window will only open if a project is active. 
- Click `Add > Add project from disk` and select the unity subfolder in this repository
- Click on the newly imported project
- Wait for Unity to compile assets and open the main interface

### Install the Git for Unity plugin

Installing Git for Unity will make managing the Unity source code much easier. 
- Follow the instructions on [this](https://github.com/spoiledcat/git-for-unity) page
- Git for Unity should detect your forked repository

### Install the MessagePack plugin for Unity

This package is required by the C# Network Tables library. 
- Download the latest release with a `.unitypackage` extension from [here](https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases/tag/v2.5.187)
- In the main unity window, select `Assets > Import Package > Custom Package`
- Browse to the package you downloaded and click `Import`

### Explore the C# Code

- That's it! You'll find the core streamer application in `Assets > Robot > MotionStreamer.cs`. 

# FAQ
### Q: Are you doing anything to initialize its location? Or do you have an idea how you'd recommend teams to initialize its location?

A: QuestNav currently does not do anything to initialize the robot position beyond reseting it so the "field oriented drive" is correct. I'm sure someone will come up with a way to use April tags for initialization. There's a Quest development API that enables placing locations on a 3D map and sharing that map between headsets.

### Q: How does the Quest headset do with hard hits from other robots? What happens if it loses its FOV by sitting in front of a wall for awhile (aligning or whatever to score)?

A: It does okay from what I've found. The test robot don't have bumpers so robot-to-robot testing has been limited. The headset has a very wide FOV, so some occlusion is okay. We've even gone as far as covering the two front cameras and the robot localized successfully. 

### Q: Does the Quest headset need to see one side of your robot or does it just need to see out of one side of your robot?

A: The headset just needs to see out one side of the robot. 

### Q: How is data getting to the Rio? Ethernet?

A: QuestNav uses a USB-Ethernet cable connected to the Oculus headset's USB port to communicate with the RIO. 

### Q: What's the pre match setup like? 

A: Make sure the headset is powered on and the app is running. You can also check that the headset is shipping data to the robot by checking the variables in the Driver Station.

### Q: How does it zero? 

A: Currently, there are two levels of "zeroing". There's the local "zero" that sets the robot heading for field-oriented drive. There's also a pose "zero" that's equivalent to long-pressing the quest button for a few seconds that is computed on the headset. The latter is currently triggered using network tables to indicate to the Quest headset that the user has requested a pose "zero". 

### Q: Can I reset the pose to some value?

A: Pull requests adding this feature are always welcome!

### Q: Can I push this around on the cart during camera calibration, or do I need to push with wheels on carpet or wait until a practice match?

A: The headset doesn't necessarily require that you show it the field before using it in a match. Much in the same way that you don't need to calibrate anything before playing a VR game.

### Q: Does running it in the pits or practice field mess with its understanding of a competition field's spaces? I don't know how long it holds on to remembering a space, especially without an internet connection.

A: Nope, it doesn't mess with anything. In fact, it will probably infinitely expand the map as you travel from the field to the pits with the headset powered on. I've seen my headset accurately map our entire house! 

### Q: Do you have any numbers on the frequency of published NT data & measurement latency? 

A: QuestNav is designed to publish the pose information to network tables on every VR display frame update. This means that the update rate is tied to the display frame rate, which by default is 90Hz. You can push it to 120Hz without any issues.

### Q: Is measurement latency / timestamp something that the headset can provide with the measurement?

A: QuestNav currently ships back a frame count and headset timestamp along with each pose estimate packet. The headset latency has to be very low, otherwise, we'd make people very sick very quickly. Robot code will probably be the larger latency contributor. 

### Q: Which Quest headset do you recommend?

A: I recommend using the Quest 3S on robots. 

