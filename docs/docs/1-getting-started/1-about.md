---
title: About QuestNav
---

# About QuestNav

QuestNav enables streaming Oculus VR headset pose information to an FRC robot using the Network Tables 4 (NT4) protocol. This pose data provides robot control systems with accurate mapping and navigation capabilities in competition fields, practice spaces, or any location.

## Thanks
Thanks to our great team of project managers that made this possible!
- [@juchong](https://github.com/juchong)
- [@SunnyBat](https://github.com/SunnyBat)
- [@ThadHouse](https://github.com/ThadHouse)
- [@SeanErn](https://github.com/SeanErn)

## Key Benefits

- More stable and reliable tracking than other FRC vision solutions
- Lower cost compared to most FRC vision solutions
- Multiple SLAM cameras for redundant, calibrated VIO (Visual Inertial Odometry)
- Camera tracking fused with onboard IMU for accurate position and velocity estimates
- Up to 120Hz robot odometry refresh rate
- Powered by Qualcomm XR2G2 platform with multiple CPU/GPU cores, 8GB RAM, dedicated video hardware, and 128GB+ storage
- Well-supported ecosystem with off-the-shelf 3D mapping libraries
- Self-contained, rechargeable battery

## Software Architecture

QuestNav implements a simple bidirectional communication structure between the VR headset and robot, enabling:
- Heading/pose reset requests
- Configuration updates
- Ping functionality
- Real-time pose data streaming

## Development

QuestNav is built using Unity and can be customized to fit specific team requirements. Full development environment setup instructions are available in the documentation.

## Support

[Ask questions and get support in the Chief Delphi thread!](https://www.chiefdelphi.com/t/questnav-the-best-robot-pose-tracking-system-in-frc/476083)