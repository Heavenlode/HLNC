# NetNode3D Class


\[Missing &lt;summary&gt; documentation for "T:HLNC.NetNode3D"\]



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+f84931ebd138c456b4e0448f1a8e3814bd665733

**C#**
``` C#
public class NetNode3D : Node3D, INetNode, 
	INotifyPropertyChanged, INetSerializable<NetNode3D>, IBsonSerializable<NetNode3D>, 
	IBsonSerializableBase
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  Node3D  →  NetNode3D</td></tr>
<tr><td><strong>Derived</strong></td><td><a href="T_HLNC_Utilities_NetTransform3D">HLNC.Utilities.NetTransform3D</a></td></tr>
<tr><td><strong>Implements</strong></td><td><a href="T_HLNC_IBsonSerializable_1">IBsonSerializable</a>(NetNode3D), <a href="T_HLNC_IBsonSerializableBase">IBsonSerializableBase</a>, <a href="T_HLNC_INetNode">INetNode</a>, <a href="T_HLNC_INetSerializable_1">INetSerializable</a>(NetNode3D), <a href="https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged" target="_blank" rel="noopener noreferrer">INotifyPropertyChanged</a></td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetNode3D__ctor">NetNode3D</a></td>
<td>Initializes a new instance of the NetNode3D class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_NetNode3D_Network">Network</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNode3D_Serializers">Serializers</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetNode3D__NetworkProcess">_NetworkProcess</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D__PhysicsProcess">_PhysicsProcess</a></td>
<td><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D__WorldReady">_WorldReady</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_BsonDeserialize">BsonDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_BsonSerialize">BsonSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_NetworkDeserialize">NetworkDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_NetworkSerialize">NetworkSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_NodePathFromNetScene">NodePathFromNetScene</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_OnPropertyChanged">OnPropertyChanged(PropertyChangedEventArgs)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_OnPropertyChanged_1">OnPropertyChanged(String)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNode3D_SetupSerializers">SetupSerializers</a></td>
<td> </td></tr>
</table>

## Events
<table>
<tr>
<td><a href="E_HLNC_NetNode3D_PropertyChanged">PropertyChanged</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
