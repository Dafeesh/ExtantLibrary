Title: Extant Library
Author: Blake Scherschel
License: See 'LICENSE.txt' in this repository

Purpose: A set of tools to help run long-term asynchronous work, Debug information,
			and host TCP networking connections.
Notes:
- Currently targeted for 3.5 subset (Unity3d), but can support Mono (2.0) and above.

ThreadRun.cs:		Base class to make an object run on its own thread.
ThreadTask.cs:		Very similar to .NET 4.5's Task class. To be used in Mono or
						previous versions of .NET where Task was originally intended.
Debug.cs:			Encapsulated way of handling console logs. Output is streamed through
						an Event.

[Networking]
NetConnection.cs:	Encapsulated way of hosting a TCPConnection using Packet.cs.
Packet.cs:			Convention of sending and receiving byte packets over a network.