# Extant Library

## Author
Blake Scherschel

## Purpose
C# library for networking by-contract using Protobuf over TCP/UDP, and for long-term asynchronous process management.

## Modules to consider
- [Extant/Threading/ThreadRun.cs](projects/Extant/Threading/ThreadRun.cs) - Virtual class to have an object safely run on its own thread.
- [Extant_Networking/NetStreamConnection.cs](projects/Extant_Networking/NetStreamConnection.cs) - Easy way to setup a by-contract network connection.
- [Extant_Networking/NetPacket.cs](projects/Extant_Networking/NetPacket.cs) - The base class for extending to custom data types to automatically build a contract for.
- [Extant/Logging/DebugLogger.cs](projects/Extant/Logging/DebugLogger.cs) - Thread safe way to log reports and messages.

## Notes
- Currently targeted for 3.5 subset (Unity3D), but can support Mono 3.5+.
- Extant/Threading/* has a few modules that seem to recreate what can already be done with Tasks, but these are useful for builds that cannot support Tasks.
