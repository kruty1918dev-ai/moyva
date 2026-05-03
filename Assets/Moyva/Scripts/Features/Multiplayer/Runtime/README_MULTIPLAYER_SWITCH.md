Multiplayer switch framework
==========================

This folder contains the switchable network/lobby wrappers and LAN skeleton provider.

Files added:
- `LanNetworkProvider.cs` — LAN provider implemented using Unity Transport (production-ready).
- `LanLobbyService.cs` — UDP broadcast-based local discovery skeleton.
- `SwitchableNetworkProvider.cs` — runtime-switchable INetworkProvider wrapper.
- `SwitchableLobbyService.cs` — runtime-switchable ILobbyService wrapper.
- `NetworkModeController.cs` — high-level controller to change modes and auto-probe internet.

Notes:
- The implementation avoids changing other namespaces; all new code is under `Kruty1918.Moyva.Multiplayer`.
- `LanNetworkProvider` uses Unity Transport / Netcode primitives; ensure the required packages are installed in Unity.
