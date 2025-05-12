# FromBSON&lt;T&gt;(ProtocolRegistry, Variant, Byte[], T) Method




## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

**C#**
``` C#
public Task<T> FromBSON<T>(
	ProtocolRegistry protocolRegistry,
	Variant context,
	byte[] data,
	T fillNode = null
)
where T : Node, INetNode

```



#### Parameters
<dl><dt>  <a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry</a></dt><dd> </dd><dt>  Variant</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.byte" target="_blank" rel="noopener noreferrer">Byte</a>[]</dt><dd> </dd><dt>  T  (Optional)</dt><dd> </dd></dl>

#### Type Parameters
<dl><dt /><dd /></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1" target="_blank" rel="noopener noreferrer">Task</a>(T)

## See Also


#### Reference
<a href="T_HLNC_Serialization_BsonTransformer">BsonTransformer Class</a>  
<a href="Overload_HLNC_Serialization_BsonTransformer_FromBSON">FromBSON Overload</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
