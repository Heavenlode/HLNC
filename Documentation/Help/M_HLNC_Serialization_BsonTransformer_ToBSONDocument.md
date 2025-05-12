# ToBSONDocument Method


\[Missing &lt;summary&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

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
<dl><dt>  <a href="T_HLNC_INetNode">INetNode</a></dt><dd>\[Missing &lt;param name="netNode"/&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]</dd><dt>  Variant  (Optional)</dt><dd>\[Missing &lt;param name="context"/&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.boolean" target="_blank" rel="noopener noreferrer">Boolean</a>  (Optional)</dt><dd>\[Missing &lt;param name="recurse"/&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1" target="_blank" rel="noopener noreferrer">HashSet</a>(<a href="https://learn.microsoft.com/dotnet/api/system.type" target="_blank" rel="noopener noreferrer">Type</a>)  (Optional)</dt><dd>\[Missing &lt;param name="skipNodeTypes"/&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1" target="_blank" rel="noopener noreferrer">HashSet</a>(<a href="https://learn.microsoft.com/dotnet/api/system.tuple-2" target="_blank" rel="noopener noreferrer">Tuple</a>(Type, <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>))  (Optional)</dt><dd>\[Missing &lt;param name="propTypes"/&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1" target="_blank" rel="noopener noreferrer">HashSet</a>(<a href="https://learn.microsoft.com/dotnet/api/system.tuple-2" target="_blank" rel="noopener noreferrer">Tuple</a>(Type, <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>))  (Optional)</dt><dd>\[Missing &lt;param name="skipPropTypes"/&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]</dd></dl>

#### Return Value
BsonDocument  
\[Missing &lt;returns&gt; documentation for "M:HLNC.Serialization.BsonTransformer.ToBSONDocument(HLNC.INetNode,Godot.Variant,System.Boolean,System.Collections.Generic.HashSet{System.Type},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}},System.Collections.Generic.HashSet{System.Tuple{Godot.Variant.Type,System.String}})"\]

## See Also


#### Reference
<a href="T_HLNC_Serialization_BsonTransformer">BsonTransformer Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
