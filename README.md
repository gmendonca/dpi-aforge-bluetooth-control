dpi-aforge-bluetooth-control
============================

Youtube video of the project working properly: http://youtu.be/UzTIzX9d7-s

If used remember to give credits to Gustavo Gomes Mendonca.

C# project that controls a prototype via Bluetooth utilizing Digital Image Processing.

The project consists of the movimentation of the prototype by a trajectory defined
by the user using the computer. I used the Microsoft.Ink.dll for doing that.
The communication of the prototype was serial with BLuetooth, and the control was made
using a webcam and Digital Image Processing with Aforge.NET.

For the prototype, it was implemented a simple control program with Arduino. Unfortunely,
the code is not provide in this repository. But, it is only a program that reads a byte
and if it is a '0' the microprocessor turn on the two motors, '1' for the left motor only
and '2' for the right motor only. Finally '3' turn both of them off. Since I didn't
developed this part much, it is one fo the must do improvements of this project.

The Digital Image Processing was based in this article:
http://www.codeproject.com/Articles/265354/Playing-Card-Recognition-Using-AForge-Net-Framewor

Further, I'll provide more documentation about the project soon.