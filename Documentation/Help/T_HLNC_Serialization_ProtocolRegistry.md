# ProtocolRegistry Class


The singleton instance of the ProtocolRegistry. This is used to serialize and deserialize scenes, network properties, and network functions sent across the network. The point is that we can't send an entire scene string across the network or some other lengthy identifier because it would take up too much bandwidth. So we use this to classify scenes as bytes which can be sent across the network, using minimal bandwidth. The same goes for network properties and functions. See [!:ProtocolRegistry.Build] for more information.



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+03b6c1d2e487070ae6af3c88edccb51282b75ac1

**C#**
``` C#
public class ProtocolRegistry : Node
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  ProtocolRegistry</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry__ctor">ProtocolRegistry</a></td>
<td>Initializes a new instance of the ProtocolRegistry class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_Serialization_ProtocolRegistry_EditorInstance">EditorInstance</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Serialization_ProtocolRegistry_Instance">Instance</a></td>
<td>The singleton instance.</td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry__EnterTree">_EnterTree</a></td>
<td><p>Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.</p><p>

Corresponds to the NotificationEnterTree notification in _Notification(Int32).</p><br />(Overrides Node._EnterTree())</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_GetPropertyCount">GetPropertyCount</a></td>
<td>Get the number of NetProperties in a scene.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_GetStaticMethodCallable">GetStaticMethodCallable(CollectedNetProperty, StaticMethodType)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_GetStaticMethodCallable_1">GetStaticMethodCallable(Int32, StaticMethodType)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_IsNetScene">IsNetScene</a></td>
<td>Check if a scene is a NetScene.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_ListFunctions">ListFunctions</a></td>
<td>List all NetFunctions for a given NetNode within the scene.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_ListNetworkChildren">ListNetworkChildren</a></td>
<td>List all NetNodes which are children of a given NetNode.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_ListProperties">ListProperties(String)</a></td>
<td>List all NetProperties in a scene.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_ListProperties_1">ListProperties(String, String)</a></td>
<td>List all NetProperties for a given NetNode within the scene.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_ListScenes">ListScenes</a></td>
<td>List all network scenes.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_ListStaticNodes">ListStaticNodes</a></td>
<td>List all NetNodes which are not scenes.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_Load">Load</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_LookupFunction">LookupFunction</a></td>
<td>Lookup a NetFunction by its scene, node, and name.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_LookupProperty">LookupProperty</a></td>
<td>Lookup a property by its scene, node, and name.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_PackNode">PackNode</a></td>
<td>Pack a scene's NetNode by path into a byte to be sent over the network.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_PackScene">PackScene</a></td>
<td>Pack a scene path into a byte to be sent over the network.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_UnpackFunction">UnpackFunction</a></td>
<td>Get a NetFunction by its scene and index (typically received from the network).</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_UnpackNode">UnpackNode</a></td>
<td>Get a NetNode path by its scene and index (typically received from the network).</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_UnpackProperty">UnpackProperty</a></td>
<td>Get a NetProperty by its scene and index (typically received from the network).</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_UnpackScene">UnpackScene</a></td>
<td>Unpack a scene byte into a scene path.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
