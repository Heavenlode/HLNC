# NetRunner Class


The primary network manager for server and client. NetRunner handles the ENet stream and passing that data to the correct objects. For more information on what kind of data is sent and received on what channels, see <a href="T_Nebula_NetRunner_ENetChannelId">NetRunner.ENetChannelId</a>.



## Definition
**Namespace:** <a href="N_Nebula">Nebula</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public class NetRunner : Node
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  NetRunner</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_Nebula_NetRunner__ctor">NetRunner</a></td>
<td>Initializes a new instance of the NetRunner class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_Nebula_NetRunner_Instance">Instance</a></td>
<td>The singleton instance.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_IsClient">IsClient</a></td>
<td> </td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_IsServer">IsServer</a></td>
<td>This is set after <a href="M_Nebula_NetRunner_StartClient">StartClient()</a> or <a href="M_Nebula_NetRunner_StartServer">StartServer()</a> is called, i.e. when <a href="P_Nebula_NetRunner_NetStarted">NetStarted</a> == true. Before that, this value is unreliable.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_MTU">MTU</a></td>
<td>Maximum Transferrable Unit. The maximum number of bytes that should be sent in a single ENet UDP Packet (i.e. a single tick) Not a hard limit.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_NetStarted">NetStarted</a></td>
<td>This is set to true once <a href="M_Nebula_NetRunner_StartClient">StartClient()</a> or <a href="M_Nebula_NetRunner_StartServer">StartServer()</a> have succeeded.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_Worlds">Worlds</a></td>
<td>The current World ID. This is mainly used for Blastoff.</td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_Nebula_NetRunner__EnterTree">_EnterTree</a></td>
<td><p>Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.</p><p>

Corresponds to the NotificationEnterTree notification in _Notification(Int32).</p><br />(Overrides Node._EnterTree())</td></tr>
<tr>
<td><a href="M_Nebula_NetRunner__OnPeerDisconnected">_OnPeerDisconnected</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> parameter will <em>generally</em> be constant (see exceptions below). <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Processing happens in order of ProcessPhysicsPriority, lower priority values are called first. Nodes with the same priority are processed in tree order, or top to bottom as seen in the editor (also known as pre-order traversal).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><p><b>

Note:</b><em>delta</em> will be larger than expected if running at a framerate lower than PhysicsTicksPerSecond / MaxPhysicsStepsPerFrame FPS. This is done to avoid "spiral of death" scenarios where performance would plummet due to an ever-increasing number of physics steps per frame. This behavior affects both _Process(Double) and _PhysicsProcess(Double). As a result, avoid using <em>delta</em> for time measurements in real-world seconds. Use the Time singleton's methods for this purpose instead, such as GetTicksUsec().</p><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_Nebula_NetRunner__Ready">_Ready</a></td>
<td><br />(Overrides Node._Ready())</td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_CreateWorldPacked">CreateWorldPacked</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_EmitSignalOnWorldCreated">EmitSignalOnWorldCreated</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_GetPeer">GetPeer</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_GetPeerId">GetPeerId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_SetupWorldInstance">SetupWorldInstance</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_StartClient">StartClient</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_StartServer">StartServer</a></td>
<td> </td></tr>
</table>

## Events
<table>
<tr>
<td><a href="E_Nebula_NetRunner_OnWorldCreated">OnWorldCreated</a></td>
<td> </td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_Nebula_NetRunner_DebugPort">DebugPort</a></td>
<td>The port for the debug server to listen on.</td></tr>
<tr>
<td><a href="F_Nebula_NetRunner_MaxPeers">MaxPeers</a></td>
<td>The maximum number of allowed connections before the server starts rejecting clients.</td></tr>
<tr>
<td><a href="F_Nebula_NetRunner_PhysicsTicksPerNetworkTick">PhysicsTicksPerNetworkTick</a></td>
<td>This determines how fast the network sends data. When physics runs at 60 ticks per second, then at 2 PhysicsTicksPerNetworkTick, the network runs at 30hz.</td></tr>
<tr>
<td><a href="F_Nebula_NetRunner_Port">Port</a></td>
<td>The port for the server to listen on, and the client to connect to. If BlastoffClient is installed, this will be overridden to 20406, the Blastoff port.</td></tr>
<tr>
<td><a href="F_Nebula_NetRunner_ServerAddress">ServerAddress</a></td>
<td>A fully qualified domain (www.example.com) or IP address (192.168.1.1) of the host. Used for client connections.</td></tr>
<tr>
<td><a href="F_Nebula_NetRunner_TPS">TPS</a></td>
<td>Ticks Per Second. The number of Ticks which are expected to elapse every second.</td></tr>
</table>

## See Also


#### Reference
<a href="N_Nebula">Nebula Namespace</a>  
