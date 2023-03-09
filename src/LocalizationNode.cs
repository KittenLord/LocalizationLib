using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LocalizationLib.Exceptions;

namespace LocalizationLib
{
    [Flags]
    public enum AdditionResult : short
    {
        FailedAdding = 1,
        SucceededAdding = 2,
        AlreadyExisted = 4
    }

    public enum LocalizationNodeType
    {
        String, Category, Array
    }

    public class LocalizationNode
    {
        public string? Value { get; init; }
        public Dictionary<string, LocalizationNode>? Nodes { get; init; }
        public List<string>? Array { get; init; }


        public LocalizationNodeType Type => Value is not null ? LocalizationNodeType.String : (Nodes is not null ? LocalizationNodeType.Category : LocalizationNodeType.Array);
        public bool IsString => Type == LocalizationNodeType.String;
        public bool IsCategory => Type == LocalizationNodeType.Category;
        public bool IsArray => Type == LocalizationNodeType.Array;


        private LocalizationNode CreateNodeOfType(LocalizationNodeType type, string defaultValue = "")
        {
            if(type == LocalizationNodeType.String) return new LocalizationNode(defaultValue);
            if(type == LocalizationNodeType.Category) return new LocalizationNode(new Dictionary<string, LocalizationNode>());
            if(type == LocalizationNodeType.Array) return new LocalizationNode(new List<string>());

            throw new NotImplementedException();
        }



        public string GetString()
        {
            if(Value is null) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.String, Type);

            return Value;
        }

        public string GetString(LocalizationPath path)
        {
            var node = GetNode(path);
            return node.GetString();
        }

        public List<string> GetArray()
        {
            if(Array is null) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Array, Type);

            return Array;
        }

        public List<string> GetArray(LocalizationPath path)
        {
            var node = GetNode(path);
            if(!node.IsArray) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Array, node.Type);
            if(node.Array is null) throw new NullReferenceException();

            return node.GetArray();
        }

        public string GetArrayElement(LocalizationPath path, int element)
        {
            var list = GetArray(path);
            if(element < 0 || element >= list.Count) throw new IndexOutOfRangeException();

            return list[element];   
        }

        public LocalizationNode GetNode(LocalizationPath path, bool first = true)
        {
            if(path.Path == "") return this;
            if(first)
                if(!DoesNodeExist(path.Copy())) throw new PathDoesNotExistException(path.Path);
            if(Nodes is null) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Category, Type);

            var nextElementName = path.ChopOff();
            if(!Nodes.ContainsKey(nextElementName)) throw new Exception(); // probably won't fire, since the path is verified by this point
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
            if(!node.IsCategory) return AdditionResult.FailedAdding;
            if(node.Nodes is null) return AdditionResult.FailedAdding;

            if(node.Nodes.ContainsKey(stringName)) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            node.Nodes.Add(stringName, new LocalizationNode(stringContent));

            return AdditionResult.SucceededAdding;
        }

        public AdditionResult AddCategory(LocalizationPath categoryPath, string categoryName)
        {
            if(DoesNodeExist(LocalizationPath.Combine(categoryPath, categoryName))) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            var node = GetNode(categoryPath);
            if(!node.IsCategory) return AdditionResult.FailedAdding;
            if(node.Nodes is null) return AdditionResult.FailedAdding;

            if(node.Nodes.ContainsKey(categoryName)) return AdditionResult.FailedAdding | AdditionResult.AlreadyExisted;
            node.Nodes.Add(categoryName, new LocalizationNode(new Dictionary<string, LocalizationNode>()));

            return AdditionResult.SucceededAdding;
        }

        public void SetString(LocalizationPath path, string value)
        {
            if(!DoesNodeExist(path.Copy())) throw new PathDoesNotExistException(path.Path);

            var pathWithoutLast = path.Copy();
            var lastElement = pathWithoutLast.RemoveLast();
            var node = GetNode(pathWithoutLast);

            if(node.Nodes is null) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Category, node.Type);
            if(!node.Nodes.ContainsKey(lastElement)) throw new Exception();
            if(!node.Nodes[lastElement].IsString) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.String, node.Type);
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

        public bool IsEquivalentTo(LocalizationNode b)
        {
            return IsOtherCompleteWIthThis(b) && b.IsOtherCompleteWIthThis(this);
        }

        private bool IsOtherCompleteWIthThis(LocalizationNode b)
        {
            if(!this.IsCategory) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Category, this.Type);
            if(!b.IsCategory) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Category, b.Type);

            if(this.Nodes is not null && b.Nodes is not null)
            {
                foreach(var node in this.Nodes)
                {
                    if(!b.Nodes.ContainsKey(node.Key)) return false;
                    if(b.Nodes[node.Key].Type != node.Value.Type) return false;

                    var nextNode = b.Nodes[node.Key];
                    if(node.Value.IsCategory) return node.Value.IsOtherCompleteWIthThis(nextNode);
                }
            }
            else return false;

            return true;
        }

        public void AddMissingNodes(LocalizationNode source, bool useSourceValues = true, string defaultValue = "")
        {
            if(this.IsString) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Category, this.Type);
            if(source.IsString) throw new NodeIsOfUnexpectedTypeException(LocalizationNodeType.Category, source.Type);

            if(this.Nodes is not null && source.Nodes is not null)
            {
                foreach(var node in source.Nodes)
                {
                    var name = node.Key;
                    var value = node.Value;
                    var isCategory = value.IsCategory;

                    var newValue = useSourceValues ? (value.Value ?? "") : defaultValue;

                    var newNode = CreateNodeOfType(value.Type, newValue);

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




        public LocalizationNode(string value) => Value = value; 
        public LocalizationNode(Dictionary<string, LocalizationNode>? nodes) => Nodes = nodes; 
        public LocalizationNode(List<string>? array) => Array = array; 
    }
}