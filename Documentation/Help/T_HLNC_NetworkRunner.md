# NetworkRunner Class


The primary network manager for server and client. NetworkRunner handles the ENet stream and passing that data to the correct objects. For more information on what kind of data is sent and received on what channels, see <a href="T_HLNC_NetworkRunner_ENetChannelId">NetworkRunner.ENetChannelId</a>.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public class NetworkRunner : Node
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  NetworkRunner</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetworkRunner__ctor">NetworkRunner</a></td>
<td>Initializes a new instance of the NetworkRunner class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_NetworkRunner_CurrentTick">CurrentTick</a></td>
<td>The current network tick. On the client side, this does not represent the server's current tick, which will always be slightly ahead.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_InputStore">InputStore</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_Instance">Instance</a></td>
<td>The singleton instance.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_IsServer">IsServer</a></td>
<td>This is set after <a href="M_HLNC_NetworkRunner_StartClient">StartClient()</a> or <a href="M_HLNC_NetworkRunner_StartServer">StartServer()</a> is called, i.e. when <a href="P_HLNC_NetworkRunner_NetStarted">NetStarted</a> == true. Before that, this value is unreliable.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_NetStarted">NetStarted</a></td>
<td>This is set to true once <a href="M_HLNC_NetworkRunner_StartClient">StartClient()</a> or <a href="M_HLNC_NetworkRunner_StartServer">StartServer()</a> have succeeded.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_ZoneInstanceId">ZoneInstanceId</a></td>
<td>The current Zone ID. This is mainly used for Blastoff.</td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetworkRunner__EnterTree">_EnterTree</a></td>
<td><p>Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.</p><p>

Corresponds to the NotificationEnterTree notification in _Notification(Int32).</p><br />(Overrides Node._EnterTree())</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__OnPeerConnected">_OnPeerConnected</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__OnPeerDisconnected">_OnPeerDisconnected</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> variable should be constant. <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__Ready">_Ready</a></td>
<td><p>Called when the node is "ready", i.e. when both the node and its children have entered the scene tree. If the node has children, their _Ready() callbacks get triggered first, and the parent node will receive the ready notification afterwards.</p><p>

Corresponds to the NotificationReady notification in _Notification(Int32). See also the <code>@onready</code> annotation for variables.</p><p>

Usually used for initialization. For even earlier initialization, #ctor() may be used. See also _EnterTree().</p><p><b>

Note:</b> This method may be called only once for each node. After removing a node from the scene tree and adding it again, _Ready() will <b>not</b> be called a second time. This can be bypassed by requesting another call with RequestReady(), which may be called anywhere before adding the node again.</p><br />(Overrides Node._Ready())</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ChangeSceneInstance">ChangeSceneInstance</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ChangeScenePacked">ChangeScenePacked</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ChangeZone">ChangeZone</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_GetAllNetworkNodes">GetAllNetworkNodes</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_GetFromNetworkId">GetFromNetworkId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_InstallBlastoffClientDriver">InstallBlastoffClientDriver</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_InstallBlastoffServerDriver">InstallBlastoffServerDriver</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_RegisterSpawn">RegisterSpawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ServerProcessTick">ServerProcessTick</a></td>
<td>This method is executed every tick on the Server side, and kicks off all logic which processes and sends data to every client.</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_Spawn">Spawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_StartClient">StartClient</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_StartServer">StartServer</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_TransferInput">TransferInput</a></td>
<td> </td></tr>
</table>

## Events
<table>
<tr>
<td><a href="E_HLNC_NetworkRunner_OnAfterNetworkTick">OnAfterNetworkTick</a></td>
<td> </td></tr>
<tr>
<td><a href="E_HLNC_NetworkRunner_PlayerConnected">PlayerConnected</a></td>
<td> </td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_NetworkRunner_CurrentScene">CurrentScene</a></td>
<td>The currently active root network scene. This should only be set via <a href="M_HLNC_NetworkRunner_ChangeSceneInstance">ChangeSceneInstance(NetworkNodeWrapper)</a> or <a href="M_HLNC_NetworkRunner_ChangeScenePacked">ChangeScenePacked(PackedScene)</a>.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_MaxPeers">MaxPeers</a></td>
<td>The maximum number of allowed connections before the server starts rejecting clients.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_MTU">MTU</a></td>
<td>Maximum Transferrable Unit. The maximum number of bytes that should be sent in a single ENet UDP Packet (i.e. a single tick) Not a hard limit.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_PhysicsTicksPerNetworkTick">PhysicsTicksPerNetworkTick</a></td>
<td>This determines how fast the network sends data. When physics runs at 60 ticks per second, then at 2 PhysicsTicksPerNetworkTick, the network runs at 30hz.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_Port">Port</a></td>
<td>The port for the server to listen on, and the client to connect to.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_ServerAddress">ServerAddress</a></td>
<td>A fully qualified domain (www.example.com) or IP address (192.168.1.1) of the host. Used for client connections.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_TPS">TPS</a></td>
<td>Ticks Per Second. The number of Ticks which are expected to elapse every second.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
