# ToBSONDocument Method




## Definition
**Namespace:** <a href="N_Nebula_Serialization">Nebula.Serialization</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public BsonDocument ToBSONDocument(
	INetNode netNode,
	Variant context = default,
	bool recurse = true,
	HashSet<Type> skipNodeTypes = null,
	HashSet<Tuple<Type, string>> propTypes = null,
	HashSet<Tuple<Type, string>> skipPropTypes = null
)
```



#### Parameters
<dl><dt>  <a href="T_Nebula_INetNode">INetNode</a></dt><dd> </dd><dt>  Variant  (Optional)</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.boolean" target="_blank" rel="noopener noreferrer">Boolean</a>  (Optional)</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1" target="_blank" rel="noopener noreferrer">HashSet</a>(<a href="https://learn.microsoft.com/dotnet/api/system.type" target="_blank" rel="noopener noreferrer">Type</a>)  (Optional)</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1" target="_blank" rel="noopener noreferrer">HashSet</a>(<a href="https://learn.microsoft.com/dotnet/api/system.tuple-2" target="_blank" rel="noopener noreferrer">Tuple</a>(Type, <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>))  (Optional)</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1" target="_blank" rel="noopener noreferrer">HashSet</a>(<a href="https://learn.microsoft.com/dotnet/api/system.tuple-2" target="_blank" rel="noopener noreferrer">Tuple</a>(Type, <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>))  (Optional)</dt><dd> </dd></dl>

#### Return Value
BsonDocument

## See Also


#### Reference
<a href="T_Nebula_Serialization_BsonTransformer">BsonTransformer Class</a>  
<a href="N_Nebula_Serialization">Nebula.Serialization Namespace</a>  
