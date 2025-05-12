# Build Method


Create and store the <a href="T_HLNC_Serialization_ProtocolResource">ProtocolResource</a>, which is used to serialize and deserialize scenes, network properties, and network functions sent across the network.



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

**C#**
``` C#
public bool Build()
```



#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.boolean" target="_blank" rel="noopener noreferrer">Boolean</a>  
True if the resource was built successfully, false otherwise.

## Example
<ul><li>Imagine our game has two network scenes, "Game", and "Character". We compile that into bytecode so that "Game" is represented as 0, and "Character" is 1. Imagine "Character" has a network property "isAlive", which is a boolean. If we want to tell the client about the "isAlive" property of the "Character" scene, We only need to send two bytes across the network: 1 (the scene bytecode for "Character") and 0 (the index of "isAlive") which takes 16 bits.</li></ul>



## See Also


#### Reference
<a href="T_HLNC_Serialization_ProtocolRegistryBuilder">ProtocolRegistryBuilder Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
