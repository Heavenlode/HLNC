# FromBSON&lt;T&gt;(ProtocolRegistry, Variant, BsonDocument, T) Method




## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+f8729a03e7629d74435a0f1f1b469444b44e5bbc

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
<dl><dt>  <a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry</a></dt><dd> </dd><dt>  Variant</dt><dd> </dd><dt>  BsonDocument</dt><dd> </dd><dt>  T  (Optional)</dt><dd> </dd></dl>

#### Type Parameters
<dl><dt /><dd /></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1" target="_blank" rel="noopener noreferrer">Task</a>(T)

## See Also


#### Reference
<a href="T_HLNC_Serialization_BsonTransformer">BsonTransformer Class</a>  
<a href="Overload_HLNC_Serialization_BsonTransformer_FromBSON">FromBSON Overload</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
