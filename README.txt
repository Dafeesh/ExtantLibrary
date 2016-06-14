Title: Extant Library
Author: Blake Scherschel
License: See 'LICENSE.txt' in this repository

Purpose: A set of tools for long-term asynchronous aid, network by-contract client/host protocol, and thread-safe mass debugging.

Modules to consider:
- Extant/Threading/ThreadRun.cs - Virtual class to have an object safely run on its own thread.
- Extant/Net/NetStreamConnection.cs & NetPacket.cs - Easy way to setup a by-contract network connection.
- Extant/Logging/DebugLogger.cs - Thread safe way to log reports and messages.

Notes:
- Currently targeted for 3.5 subset (Unity3d), but can support Mono 3.5+.
- Extant/Threading/* has a few modules that seem to recreate what can already be done with Tasks, but these are useful for builds that cannot support Tasks.
