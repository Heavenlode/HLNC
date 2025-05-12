# NetNode Class


Node, extended with HLNC networking capabilities. This is the most basic networked object. On every network tick, all NetNode nodes in the scene tree automatically have their <a href="T_HLNC_NetProperty">network properties</a> updated with the latest data from the server. Then, the special <a href="M_HLNC_NetNode__NetworkProcess">NetworkProcess</a> method is called, which indicates that a network Tick has occurred. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the [!:INetworkInputHandler] interface, or network function calls via <a href="T_HLNC_NetFunction">NetFunction</a> attributes. The server receives client inputs, can access them via <a href="M_HLNC_NetworkController_GetInput">GetInput()</a>, and handle them accordingly within <a href="M_HLNC_NetNode__NetworkProcess">NetworkProcess</a> to mutate state.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

**C#**
``` C#
public class NetNode : Node, INetNode, 
	INotifyPropertyChanged, INetSerializable<NetNode>, IBsonSerializable<NetNode>, 
	IBsonSerializableBase
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  NetNode</td></tr>
<tr><td><strong>Implements</strong></td><td><a href="T_HLNC_INetNode">INetNode</a>, <a href="T_HLNC_Serialization_IBsonSerializable_1">IBsonSerializable</a>(NetNode), <a href="T_HLNC_Serialization_IBsonSerializableBase">IBsonSerializableBase</a>, <a href="T_HLNC_Serialization_INetSerializable_1">INetSerializable</a>(NetNode), <a href="https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged" target="_blank" rel="noopener noreferrer">INotifyPropertyChanged</a></td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetNode__ctor">NetNode</a></td>
<td>Initializes a new instance of the NetNode class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_NetNode_Network">Network</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNode_Serializers">Serializers</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetNode__NetworkProcess">_NetworkProcess</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> parameter will <em>generally</em> be constant (see exceptions below). <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Processing happens in order of ProcessPhysicsPriority, lower priority values are called first. Nodes with the same priority are processed in tree order, or top to bottom as seen in the editor (also known as pre-order traversal).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><p><b>

Note:</b><em>delta</em> will be larger than expected if running at a framerate lower than PhysicsTicksPerSecond / MaxPhysicsStepsPerFrame FPS. This is done to avoid "spiral of death" scenarios where performance would plummet due to an ever-increasing number of physics steps per frame. This behavior affects both _Process(Double) and _PhysicsProcess(Double). As a result, avoid using <em>delta</em> for time measurements in real-world seconds. Use the Time singleton's methods for this purpose instead, such as GetTicksUsec().</p><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_HLNC_NetNode__WorldReady">_WorldReady</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_BsonDeserialize">BsonDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_BsonSerialize">BsonSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_NetworkDeserialize">NetworkDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_NetworkSerialize">NetworkSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_NodePathFromNetScene">NodePathFromNetScene</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_OnPropertyChanged">OnPropertyChanged(PropertyChangedEventArgs)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_OnPropertyChanged_1">OnPropertyChanged(String)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode_SetupSerializers">SetupSerializers</a></td>
<td> </td></tr>
</table>

## Events
<table>
<tr>
<td><a href="E_HLNC_NetNode_PropertyChanged">PropertyChanged</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
