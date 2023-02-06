using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocalizationLib
{
    [Flags]
    public enum AdditionResult : short
    {
        FailedAdding = 1,
        SucceededAdding = 2,
        AlreadyExisted = 4
    }

    public class LocalizationNode
    {
        public string? Value { get; init; }
        public Dictionary<string, LocalizationNode>? Nodes { get; init; }



        public bool IsString => Value is not null;
        public bool IsCategory => Nodes is not null;



        public string GetString()
        {
            if(Value is null) throw new Exception();

            return Value;
        }

        public string GetString(LocalizationPath path)
        {
            var node = GetNode(path);
            return node.GetString();
        }

        public LocalizationNode GetNode(LocalizationPath path, bool first = true)
        {
            if(path.Path == "") return this;
            if(first)
                if(!DoesNodeExist(path.Copy())) throw new Exception();
            if(Nodes is null) throw new Exception();

            var nextElementName = path.ChopOff();
            if(!Nodes.ContainsKey(nextElementName)) throw new Exception();
            var nextElement = Nodes[nextElementName];
            return nextElement.GetNode(path, false);
        }

        public bool DoesNodeExist(LocalizationPath path)
        {
            if(!path.IsValid()) return false;
            if(path.Path == "") return true;
            if(Nodes is null) return false;

            var nextElement = path.ChopOff();
            if(!Nodes.ContainsKey(nextElement)) return false;

            return Nodes[nextElement].DoesNodeExist(path);        
        }

        public AdditionResult AddString(LocalizationPath categoryPath, string stringName, string stringContent)
        {
            if(DoesNodeExist(LocalizationPath.Combine(categoryPath, stringName))) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            var node = GetNode(categoryPath);
            if(node.IsString) return AdditionResult.FailedAdding;
            if(node.Nodes is null) return AdditionResult.FailedAdding;

            if(node.Nodes.ContainsKey(stringName)) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            node.Nodes.Add(stringName, new LocalizationNode(stringContent));

            return AdditionResult.SucceededAdding;
        }

        public AdditionResult AddCategory(LocalizationPath categoryPath, string categoryName)
        {
            if(DoesNodeExist(LocalizationPath.Combine(categoryPath, categoryName))) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            var node = GetNode(categoryPath);
            if(node.IsString) return AdditionResult.FailedAdding;
            if(node.Nodes is null) return AdditionResult.FailedAdding;

            if(node.Nodes.ContainsKey(categoryName)) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            node.Nodes.Add(categoryName, new LocalizationNode(new Dictionary<string, LocalizationNode>()));

            return AdditionResult.SucceededAdding;
        }

        public void SetString(LocalizationPath path, string value)
        {
            if(!DoesNodeExist(path.Copy())) throw new Exception();

            var pathWithoutLast = path.Copy();
            var lastElement = pathWithoutLast.RemoveLast();
            var node = GetNode(pathWithoutLast);

            if(node.Nodes is null) throw new Exception();
            if(!node.Nodes.ContainsKey(lastElement)) throw new Exception();
            if(node.Nodes[lastElement].IsCategory) throw new Exception();
            node.Nodes[lastElement] = new LocalizationNode(value);
        }

        public string GetStringInit(LocalizationPath path, string initValue)
        {
            return GetStringInit(path, initValue, out _);
        }

        public string GetStringInit(LocalizationPath path, string initValue, out bool createdNew)
        {
            var pathRemoveLast = path.Copy();
            var last = pathRemoveLast.RemoveLast();

            createdNew = false;
            if(!DoesNodeExist(path.Copy())) createdNew = AddString(pathRemoveLast, last, initValue).HasFlag(AdditionResult.SucceededAdding);
            return GetString(path);
        }

        public void AddMissingNodes(LocalizationNode source, bool useSourceValues = true, string defaultValue = "")
        {
            if(this.IsString || source.IsString) throw new Exception();

            if(this.Nodes is not null && source.Nodes is not null)
            {
                foreach(var node in source.Nodes)
                {
                    var name = node.Key;
                    var value = node.Value;
                    var isCategory = value.IsCategory;

                    var newNode = isCategory ? new LocalizationNode(new Dictionary<string, LocalizationNode>()) : new LocalizationNode(useSourceValues ? (value.Value ?? "") : defaultValue);

                    if(!this.Nodes.ContainsKey(node.Key))
                        this.Nodes.Add(node.Key, newNode);
                    else
                        newNode = this.Nodes[node.Key];

                    if(isCategory)
                        newNode.AddMissingNodes(node.Value, useSourceValues, defaultValue);
                }
            }
        }

        public void MergeNodes(LocalizationNode another, bool useSourceValues = true, string defaultValue = "")
        {
            this.AddMissingNodes(another, useSourceValues, defaultValue);
            another.AddMissingNodes(this, useSourceValues, defaultValue);
        }




        public LocalizationNode(string value) { Value = value; }
        public LocalizationNode(Dictionary<string, LocalizationNode>? nodes) { Nodes = nodes; }
    }
}