# ProtocolRegistry Methods




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
<td><a href="M_HLNC_Serialization_ProtocolRegistry_GetStaticMethodCallable_1">GetStaticMethodCallable(Int32, StaticMethodType)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_ProtocolRegistry_GetStaticMethodCallable">GetStaticMethodCallable(ProtocolNetProperty, StaticMethodType)</a></td>
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
<a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
