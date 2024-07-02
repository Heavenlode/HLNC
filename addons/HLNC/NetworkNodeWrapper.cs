using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Godot;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;

namespace HLNC {
    public partial class NetworkNodeWrapper {

        // Custom operator overload to validate null
        public static bool operator ==(NetworkNodeWrapper a, NetworkNodeWrapper b) {
            if (!ReferenceEquals(a, null)) {
                if (ReferenceEquals(b, null)) {
                    return a.Node == null;
                }
            }
            if (!ReferenceEquals(b, null)) {
                if (ReferenceEquals(a, null)) {
                    return b.Node == null;
                }
            }
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) {
                return true;
            }
            return a.Equals(b) || a.Node == b.Node;
        }

        public static bool operator !=(NetworkNodeWrapper a, NetworkNodeWrapper b) {
            return !(a == b);
        }

        public Node Node { get; }
        private Dictionary<string, StringName> properties = new Dictionary<string, StringName>();
        private Dictionary<string, StringName> methods = new Dictionary<string, StringName>();
        public NetworkNodeWrapper(Node node) {
            // TODO: Validate the node implements the interface correctly
            Node = node;
            if (node == null) return;
            var requiredProperties = new HashSet<string> {
                "InputAuthority",
                "NetworkId",
                "NetworkParentId",
                "DynamicSpawn",
            };
            foreach (var prop in node.GetPropertyList()) {
                var pascalName = ToPascalCase(prop["name"].AsString());

                if (requiredProperties.Contains(pascalName)) {
                    properties[pascalName] = new StringName(prop["name"].AsString());
                }

                foreach (var requiredProperty in requiredProperties) {
                    if (!properties.ContainsKey(requiredProperty)) {
                        // Assume the default property is an non-exported PascalCase
                        properties[requiredProperty] = new StringName(requiredProperty);
                    }
                }
            }
        }

        // Convert from snake_case to PascalCase
        private static string ToPascalCase(string name) {
            var pascalCase = "";
            var capitalize = true;
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '_')
                {
                    capitalize = true;
                }
                else if (capitalize)
                {
                    pascalCase += char.ToUpper(name[i]);
                    capitalize = false;
                }
                else
                {
                    pascalCase += name[i];
                }
            }
            return pascalCase;
        }

        private static string ToSnakeCase(string name) {
            var snakeCase = "";
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    if (i > 0 && name[i - 1] != '_')
                    {
                        snakeCase += "_";
                    }
                    snakeCase += char.ToLower(name[i]);
                }
                else
                {
                    snakeCase += name[i];
                }
            }
            return snakeCase;
        }
        private Variant Get(string name) {
            return Node.Get(properties[name]);
        }

        private void Set(string name, Variant value) {
            Node.Set(properties[name], value);
        }

        private Variant Call(string name, params Variant[] args) {
            if (methods.ContainsKey(name)) {
                return Node.Call(methods[name], args);
            }

            if (Node.HasMethod(name)) {
                var result = Node.Call(name, args);
                methods[name] = new StringName(name);
                return result;
            }
            
            var snakeCase = ToSnakeCase(name);
            if (Node.HasMethod(snakeCase)) {
                methods[name] = new StringName(snakeCase);
                return Node.Call(snakeCase, args);
            }

            throw new Exception($"Method {snakeCase} not found on {Node.GetPath()}");
        }

        public PeerId InputAuthority {
            get {
                return Get("InputAuthority").AsInt64();
            }

            internal set {
                Set("InputAuthority", value);
            }
        }

        public NetworkId NetworkId {
            get {
                return Get("NetworkId").AsInt64();
            }

            internal set {
                Set("NetworkId", value);
            }
        }

        public NetworkNodeWrapper NetworkParent => NetworkRunner.Instance.GetFromNetworkId(NetworkParentId);
        
        // TODO: Handle null?
        internal byte NetworkSceneId => NetworkScenesRegister.SCENES_PACK[Node.SceneFilePath];

        public NetworkId NetworkParentId {
            get {
                return Get("NetworkParentId").AsInt64();
            }

            internal set {
                Set("NetworkParentId", value);
            }
        }

        public bool DynamicSpawn {
            get {
                return Get("DynamicSpawn").AsBool();
            }

            internal set {
                Set("DynamicSpawn", value);
            }
        }

        public IStateSerailizer[] Serializers {
            get {
                // TODO: Support serializers across other languages / node types
                if (Node is NetworkNode3D networkNode) {
                    return networkNode.Serializers;
                }
                return [];
            }
        }

        internal void _NetworkPrepare() {
            Call("_NetworkPrepare");
        }

        public void _NetworkProcess(Tick tick) {
            Call("_NetworkProcess", tick);
        }

        public List<NetworkNodeWrapper> StaticNetworkChildren {
            get {
                return NetworkScenesRegister.STATIC_NETWORK_NODE_PATHS[Node.SceneFilePath].Aggregate(new List<NetworkNodeWrapper>(), (acc, path) => {
                    var child = Node.GetNodeOrNull(path);
                    if (child != null) {
                        acc.Add(new NetworkNodeWrapper(child));
                    }
                    return acc;
                });
            }
        }
    }
}