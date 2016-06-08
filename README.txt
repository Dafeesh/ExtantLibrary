Title: Extant Library
Author: Blake Scherschel
License: See 'LICENSE.txt' in this repository

Purpose: A set of tools for long-term asynchronous aid, Debug logging,
		 and client/host for TCP/UDP networking connections.

Modules to consider:
- Extant/Threading/ThreadRun.cs - Virtual class to make a object contain and run on it's own thread.
- Extant/Net/DualSocketConnection.cs - Easy way to setup a Tcp/Udp hybrid connection.
- Extant/Logging/DebugLogger.cs - Thread safe way to log reports and messages.

Notes:
- Currently targeted for 3.5 subset (Unity3d), but can support Mono 3.5+.
