# NetworkRunner.BlastoffCommands Enumeration


These are commands which the server may send to Blastoff, which informs Blastoff how to act upon the client connection.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public enum BlastoffCommands
```



## Members
<table>
<tr>
<td>NewInstance</td>
<td>0</td>
<td>Requests Blastoff to create a new server instance, i.e. of the game.</td></tr>
<tr>
<td>ValidateClient</td>
<td>1</td>
<td>Informs Blastoff that the client is valid and communication may be bridged.</td></tr>
<tr>
<td>RedirectClient</td>
<td>2</td>
<td>Requests Blastoff to redirect the user to another zone Id.</td></tr>
<tr>
<td>InvalidClient</td>
<td>3</td>
<td>Requests Blastoff to disconnect the client.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
