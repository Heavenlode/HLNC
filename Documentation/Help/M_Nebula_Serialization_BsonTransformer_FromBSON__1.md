# FromBSON&lt;T&gt;(ProtocolRegistry, Variant, BsonDocument, T) Method




## Definition
**Namespace:** <a href="N_Nebula_Serialization">Nebula.Serialization</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public Task<T> FromBSON<T>(
	ProtocolRegistry protocolRegistry,
	Variant context,
	BsonDocument data,
	T fillNode = null
)
where T : Node, INetNode

```



#### Parameters
<dl><dt>  <a href="T_Nebula_Serialization_ProtocolRegistry">ProtocolRegistry</a></dt><dd> </dd><dt>  Variant</dt><dd> </dd><dt>  BsonDocument</dt><dd> </dd><dt>  T  (Optional)</dt><dd> </dd></dl>

#### Type Parameters
<dl><dt /><dd /></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1" target="_blank" rel="noopener noreferrer">Task</a>(T)

## See Also


#### Reference
<a href="T_Nebula_Serialization_BsonTransformer">BsonTransformer Class</a>  
<a href="Overload_Nebula_Serialization_BsonTransformer_FromBSON">FromBSON Overload</a>  
<a href="N_Nebula_Serialization">Nebula.Serialization Namespace</a>  
