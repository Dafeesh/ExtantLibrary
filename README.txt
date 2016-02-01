Title: Extant Library
Author: Blake Scherschel
License: See 'LICENSE.txt' in this repository

Purpose: A set of miscellaneous tools, long-term asynchronous aids, Debug logger,
		 and client/host for TCP/UDP networking connections.

Modules to consider:
- Extant/Threading/ThreadRun.cs - Virtual class to make a object contain and run on it's own thread.
- Extant/Net/ClientConnection.cs - Easy way to setup a Tcp/Udp connection.
- Extant/Logging/DebugLogger.cs - Thread safe way to log reports and messages.

Notes:
- Currently targeted for 3.5 subset (Unity3d), but can support Mono (2.0) and above.
