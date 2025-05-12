# NetworkController Class


Manages the network state of a <a href="T_HLNC_NetNode">NetNode</a> (including <a href="T_HLNC_NetNode2D">NetNode2D</a> and <a href="T_HLNC_NetNode3D">NetNode3D</a>).



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

**C#**
``` C#
public class NetworkController : RefCounted
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  RefCounted  →  NetworkController</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetworkController__ctor">NetworkController</a></td>
<td>Initializes a new instance of the NetworkController class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_NetworkController_CurrentWorld">CurrentWorld</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_InputAuthority">InputAuthority</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_IsClientSpawn">IsClientSpawn</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_IsCurrentOwner">IsCurrentOwner</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_IsWorldReady">IsWorldReady</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_NetId">NetId</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_NetParent">NetParent</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_NetParentId">NetParentId</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkController_Owner">Owner</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetworkController__NetworkProcess">_NetworkProcess</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController__Notification">_Notification</a></td>
<td><br />(Overrides GodotObject._Notification(Int32))</td></tr>
<tr>
<td><a href="M_HLNC_NetworkController__WorldReady">_WorldReady</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_Despawn">Despawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_EmitSignalInterestChanged">EmitSignalInterestChanged</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_EmitSignalNetPropertyChanged">EmitSignalNetPropertyChanged</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_FindFromChild">FindFromChild</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_GetInput">GetInput</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_GetNetworkChildren">GetNetworkChildren</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_GetNetworkInput">GetNetworkInput</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_IsNetScene">IsNetScene</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_NodePathFromNetScene">NodePathFromNetScene</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_PrepareSpawn">PrepareSpawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_SetNetworkInput">SetNetworkInput</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkController_SetPeerInterest">SetPeerInterest</a></td>
<td> </td></tr>
</table>

## Events
<table>
<tr>
<td><a href="E_HLNC_NetworkController_InterestChanged">InterestChanged</a></td>
<td> </td></tr>
<tr>
<td><a href="E_HLNC_NetworkController_NetPropertyChanged">NetPropertyChanged</a></td>
<td> </td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_NetworkController_InterestLayers">InterestLayers</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
