using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Godot;

namespace Nebula.Internal.Utility
{
    internal class ConfigParser
    {
        public Dictionary<string, string> resourceToPathMap = new Dictionary<string, string>(); 
        public class GdScene
        {
            public int LoadSteps { get; set; }
            public int Format { get; set; }
            public string Uid { get; set; }
        }

        public class ExtResource
        {
            public string Type { get; set; }
            public string Path { get; set; }
            public string Id { get; set; }
        }

        public class SubResource
        {
            public string Type { get; set; }
            public string Id { get; set; }
            public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        }

        public class Node
        {
            public string Name { get; set; }
            public string Type { get; set; }

            #nullable enable
            public string? Parent { get; set; }
            public string? Instance { get; set; }
            #nullable disable
            public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        }

        public class ParsedTscn
        {
            public GdScene GdScene { get; set; }
            public List<ExtResource> ExtResources { get; set; } = new List<ExtResource>();
            public List<SubResource> SubResources { get; set; } = new List<SubResource>();
            public List<Node> Nodes { get; set; } = new List<Node>();
            public Node RootNode { get; set; }
        }

        public ParsedTscn ParseTscnFile(string fileText)
        {
            var parsedTscn = new ParsedTscn();
            var lines = fileText.Split('\n', (char)StringSplitOptions.RemoveEmptyEntries);
            SubResource currentSubResource = null;
            Node currentNode = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("[gd_scene"))
                {
                    parsedTscn.GdScene = ParseGdScene(line);
                }
                else if (line.StartsWith("[ext_resource"))
                {
                    parsedTscn.ExtResources.Add(ParseExtResource(line));
                }
                else if (line.StartsWith("[sub_resource"))
                {
                    currentSubResource = ParseSubResource(line);
                    parsedTscn.SubResources.Add(currentSubResource);
                    currentNode = null;
                }
                else if (line.StartsWith("[node"))
                {
                    currentNode = ParseNode(line);
                    parsedTscn.Nodes.Add(currentNode);
                    if (currentNode.Parent == null) {
                        parsedTscn.RootNode = currentNode;
                    }
                    currentSubResource = null;
                }
                else if (currentSubResource != null && line.Contains("="))
                {
                    var parts = line.Split('=');
                    currentSubResource.Properties[parts[0].Trim()] = parts[1].Trim();
                }
                else if (currentNode != null && line.Contains("="))
                {
                    var parts = line.Split('=');
                    var propName = parts[0].Trim();
                    var propValue = parts[1].Trim();
                    if (propName == "script")
                    {
                        var regex = new Regex(@"ExtResource\(""([^']+)""\)");
                        var match = regex.Match(propValue);
                        if (match.Success)
                        {
                            var resourceId = match.Groups[1].Value;
                            propValue = resourceToPathMap[resourceId];
                        }
                    }
                    currentNode.Properties[propName] = propValue;
                }
            }

            return parsedTscn;
        }

        private GdScene ParseGdScene(string line)
        {
            var gdScene = new GdScene();
            var parts = line.Split(' ');
            foreach (var part in parts)
            {
                if (part.StartsWith("load_steps="))
                {
                    gdScene.LoadSteps = int.Parse(part.Split('=')[1].Trim('"', ']'));
                }
                else if (part.StartsWith("format="))
                {
                    gdScene.Format = int.Parse(part.Split('=')[1].Trim('"', ']'));
                }
                else if (part.StartsWith("uid="))
                {
                    gdScene.Uid = part.Split('=')[1].Trim('"', ']');
                }
            }
            return gdScene;
        }

        private ExtResource ParseExtResource(string line)
        {
            var extResource = new ExtResource();
            var parts = line.Split(' ');
            foreach (var part in parts)
            {
                if (part.StartsWith("type="))
                {
                    extResource.Type = part.Split('=')[1].Trim('"', ']');
                }
                else if (part.StartsWith("path="))
                {
                    extResource.Path = part.Split('=')[1].Trim('"', ']');
                }
                else if (part.StartsWith("id="))
                {
                    var regex = new Regex(@"id=""?([^""]+)""?");
                    var match = regex.Match(part);
                    if (match.Success)
                    {
                        extResource.Id = match.Groups[1].Value;
                    }
                }
            }
            resourceToPathMap[extResource.Id] = extResource.Path;
            return extResource;
        }

        private SubResource ParseSubResource(string line)
        {
            var subResource = new SubResource();
            var parts = line.Split(' ');
            foreach (var part in parts)
            {
                if (part.StartsWith("type="))
                {
                    subResource.Type = part.Split('=')[1].Trim('"', ']');
                }
                else if (part.StartsWith("id="))
                {
                    subResource.Id = part.Split('=')[1].Trim('"', ']');
                }
            }
            return subResource;
        }

        private Node ParseNode(string line)
        {
            var node = new Node();
            var parts = line.Split(' ');
            foreach (var part in parts)
            {
                if (part.StartsWith("name="))
                {
                    node.Name = part.Split('=')[1].Trim('"', ']');
                }
                else if (part.StartsWith("type="))
                {
                    node.Type = part.Split('=')[1].Trim('"', ']');
                }
                else if (part.StartsWith("parent="))
                {
                    node.Parent = part.Split('=')[1].Trim('"', ']');
                } else if (part.StartsWith("instance="))
                {
                    var inst = part.Split('=')[1].Trim('"', ']');
                    var regex = new Regex(@"ExtResource\(""([^']+)""\)");
                    var match = regex.Match(inst);
                    if (match.Success)
                    {
                        var resourceId = match.Groups[1].Value;
                        inst = resourceToPathMap[resourceId];
                        if (!inst.EndsWith(".tscn")) continue;
                        node.Instance = inst;
                    }
                }
            }
            return node;
        }
    }
}