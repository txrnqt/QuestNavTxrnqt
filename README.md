# QuestNav
This project enables streaming Oculus VR headset pose information to an FRC robot using the Network Tables 4 (NT4) protocol. This pose information can be used by the robot control system to accurately map it's surroundings and navigate around a competition field, practice space, or any other location. QuestNav produces a more stable and reliable tracking pose than any other FRC vision solution (LimeLight, Photon Vision, etc.)

![Demo Video](https://nextcloud.chongflix.tv/public.php/dav/files/Ey4NL4NgTfDXW68/)

[Check out the full video here!](https://youtu.be/Mo0p1GGeasM)

Using a VR headset for robot localization has several advantages:
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
	- *This project was tested using REV swerve modules assembled with REV NEOs and Spark Max motor controllers.*
2. Quest 3S headset
	- *A Quest 3S headset is recommended due to its lower cost and excellent tracking performance. It's currently not clear whether the depth projector on the Quest 3 benefits FRC applications.*
3. Supported USB-C to Ethernet + power passthrough adapter

# Power Requirements

TL;DR: An external 5V supply is **REQUIRED** for the headset to operate properly on an FRC robot. 5V can be supplied from any stable source - including the RoboRIO's USB ports. 

### USB to Ethernet Adapters

As per the FRC robot rules (R707 in the 2024 ruels), wireless communication is not allowed within the robot. In order to comply with this rule, all communication with the Quest headset must be done over a wired link at competition events. USB to Ethernet adapters offer a convenient way to communicate with the headset, however, the correct style of adapter must be chosen to avoid connectivity issues. 

The USB port on Quest headsets was never designed to constantly supply 5V at "high power" (5V @ 500mA as per the USB spec), so supplemental power must be provided to ensure a reliable connection. **Only USB to Ethernet adapters that support power passthrough should be used on a robot!** These products are often sold as "USB C port replicators", "USB C docks", "USB C charging adapters", etc. A list of recommended, tested adapters can be found [here](ADAPTERS.md).

### 5V Power Recommendations

There are several ways to power the Quest headset + USB to Ethernet adapter on an FRC robot: 

**1. Power the headset using a USB port on the RoboRIO**

The USB ports on the RoboRIO provide a stable 5V source that the headset can use to power the Ethernet adapter. This port will not supply enough power to charge the Quest headset, so you'll need to keep an eye on charge levels throughout the event. This is the quickest and easiest option. 

**2. Power the headset using a 5V USB battery bank**

This approach has several benefits:

- It provides the Quest headset and USB to Ethernet adapter with clean, reliable power 
- The battery should supply enough power to sustain the Quest headset's internal battery indefinitely
- Charge state can be monitored externally using the power meter on the battery bank
- Battery banks can be replaced without interrupting (powering off) the headset
- No soldering required

**NOTE:** It's important to make sure **only** 5V is supplied to the robot. You should avoid power banks that support USB C Power Delivery (PD). In our testing, we've noticed that some USB to Ethernet adapters will boot loop when a voltage greater than 5V is applied. Alternatively, you can use a USB A to USB C cable to foce a USB PD power bank to deliver 5V to the headset. 

**3. Power the headset using a good quality 5V regulator**

This approach is likely the most convenient. However, if implemented incorrectly, it may lead to several issues that might be difficult to debug. 

Recommended USB-compliant 5V regulators:
- [Redux Robotics Zinc-V](https://shop.reduxrobotics.com/zinc-v/)

Recommended 5V regulators:
- [Grapple Robotics MitoCANdria](https://www.thethriftybot.com/products/mitocandria)
- [Pololu D36V50F5 Regulator](https://www.pololu.com/product/4091)

For non-compliant regulators, you'll need to use a USB breakout board like [this](https://a.co/d/gLUZN0Z) one that includes onboard sense resistors so that the headset knows that it's connected to a 5V power source. 

# Software Flow
A high-level overview of the software architecture is shown below.

![QuestNav Software Block Diagram](docs/QuestNav-Example-Flow.png)

# Testing QuestNav
At it's heart, QuestNav is merely a VR app designed to push data to Network Tables. However, some one-time setup is required before we're able to push a custom app to a VR headset. 

## One-Time Setup
1. Follow [this](https://medium.com/sidequestvr/how-to-turn-on-developer-mode-for-the-quest-3-509244ccd386) guide to set up "Developer Mode" 
2. Sign into your Meta developer account and download the [Meta Quest Developer Hub (MQDH)](https://developers.meta.com/horizon/develop)
3. Configure the following settings on your Quest headset:
	- Enable travel mode ([link](https://www.meta.com/help/quest/articles/in-vr-experiences/oculus-features/travel-mode/) to instructions)
	- Set the display timeout to 4 hours in `Settings > General > Power > Display off`
	- Disable WiFi in `Settings > WiFi`
		- **NOTE:** Be sure to completely turn off WiFi, otherwise the headset will constantly disconnect from the robot network as it tries to look for the internet.
	- Disable Bluetooth in `Settings > Bluetooth`
	- Disable the guardian for development purposes `Settings > Advanced > Experimental Settings > Enable custom settings`
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

### Explore the C# Code and Build The Project

- The main scene is located in `Assets > Scenes > QuestNav`
- The pose streaming code is located in `Assets > Robot > MotionStreamer.cs`.

Unity will need to download and install several Android packages during the first build, so it might be helpful to connect to the headset over USB the first time to make sure it can download everything from the internet. Subsequent builds should be faster and will not require additional downloads. 

The project should be configured for Android out of the box. If not, you may need to manually change the build target to Android in `File > Build Profiles`

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

