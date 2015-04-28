# KaiserAmulation
A project developed in Kinesis Studio. The goal is tracking people and objects in the patient's room。
All code uploaded above are written by me.

The most of the code are about object tracking. At begining, I used the SURF algorithm but it run very slow in the unity scene. 
So the final solution is to use infrad camera to track the reflective material instead of tracking a specific image pattern.

The userController.cs is the main script tracking people in the room. The script is used to recognize people and assign their id.
For the most part of the script, it is trying to write a logic for kinect2 to tracking person properly (without missing a person while they are walking)


