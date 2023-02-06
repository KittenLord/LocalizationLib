using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace LocalizationLib
{
    internal class CachedNode
    {
        public string Localization { get; set; }
        public LocalizationNode Node { get; set; }

        public CachedNode(string localization, LocalizationNode node)
        {
            Localization = localization;
            Node = node;
        }
    }

    public struct StringInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public StringInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class Localizator
    {
        private const string defaultLocalization = "default";
        public string CurrentLocalization { get; set; } = defaultLocalization;
        public Localizator SetLocalization(string localization)
        {
            CurrentLocalization = localization;
            return this;
        }

        
        public LocalizatorSettings Settings { get; set; }
        private CachedNode? CachedNode;
        
        




        public string GetString(string path) => GetString(path, CurrentLocalization, true);
        public string GetString(string path, string localization) => GetString(path, localization, false);
        private string GetString(string path, string localization, bool cacheNode = false)
        {
            path = GetPath(path, localization);
            var node = GetNode(path, localization, cacheNode);
            return node.GetString();
        }


        


        public string GetStringInit(string path, string initValue) => GetStringInit(path, CurrentLocalization, initValue, true);
        public string GetStringInit(string path, string localization, string initValue) => GetStringInit(path, localization, initValue, false);
        private string GetStringInit(string path, string localization, string initValue, bool cacheNode = false)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization, cacheNode);
            var str = node.GetStringInit(new LocalizationPath(path), initValue, out bool createdNew);

            return SaveAfterModifying(str, createdNew, localization, node);
        }





        public AdditionResult AddString(string path, string stringName, string stringContent) => AddString(path, CurrentLocalization, stringName, stringContent, true);
        public AdditionResult AddString(string path, string localization, string stringName, string stringContent) => AddString(path, localization, stringName, stringContent, false);
        private AdditionResult AddString(string path, string localization, string stringName, string stringContent, bool cacheNode = false)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization, cacheNode);

            var response = node.AddString(new LocalizationPath(path), stringName, stringContent);
            var createdNew = response.HasFlag(AdditionResult.SucceededAdding);

            return SaveAfterModifying(response, createdNew, localization, node);
        }





        public bool AddStrings(string path, params StringInfo[] strings) => AddStrings(path, CurrentLocalization, true, strings);
        public bool AddStrings(string path, string localization, params StringInfo[] strings) => AddStrings(path, localization, false, strings);
        private bool AddStrings(string path, string localization, bool cacheNode, params StringInfo[] strings)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization, cacheNode);

            bool success = true;
            bool createdNew = false;
            foreach(var s in strings)
            {
                var r = node.AddString(new LocalizationPath(path), s.Name, s.Value);
                createdNew |= r.HasFlag(AdditionResult.SucceededAdding);

                success &= r.HasFlag(AdditionResult.AlreadyExisted) || r.HasFlag(AdditionResult.SucceededAdding);
            }

            return SaveAfterModifying(success, createdNew, localization, node);
        }





        public AdditionResult AddCategory(string path, string categoryName) => AddCategory(path, CurrentLocalization, categoryName, true);
        public AdditionResult AddCategory(string path, string localization, string categoryName) => AddCategory(path, localization, categoryName, false);
        private AdditionResult AddCategory(string path, string localization, string categoryName, bool cacheNode)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization, cacheNode);

            var response = node.AddCategory(new LocalizationPath(path), categoryName);
            var createdNew = response.HasFlag(AdditionResult.SucceededAdding);

            return SaveAfterModifying(response, createdNew, localization, node);
        }





        public bool AddCategories(string path, List<string> categoryNames) => AddCategories(path, CurrentLocalization, true, categoryNames);
        public bool AddCategories(string path, string localization, List<string> categoryNames) => AddCategories(path, localization, false, categoryNames);
        private bool AddCategories(string path, string localization, bool cacheNode, List<string> categoryNames)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization, cacheNode);

            bool success = true;
            bool createdNew = false;
            foreach(var s in categoryNames)
            {
                var r = node.AddCategory(new LocalizationPath(path), s);
                createdNew |= r.HasFlag(AdditionResult.SucceededAdding);

                success &= r.HasFlag(AdditionResult.AlreadyExisted) || r.HasFlag(AdditionResult.SucceededAdding);
            }

            return SaveAfterModifying(success, createdNew, localization, node);
        }





        public void MergeLocalizations(string loc1, string loc2, bool useSourceValues = true, string defaultValue = "")
        {
            if(!Settings.CanWrite) throw new Exception();

            LocalizationNode node1 = GetNode("", loc1, true);
            LocalizationNode node2 = GetNode("", loc2, true);

            var mergeNode1 = node1.GetNode(new LocalizationPath(Settings.UseSingleFile ? loc1 : ""));
            var mergeNode2 = node2.GetNode(new LocalizationPath(Settings.UseSingleFile ? loc2 : ""));

            mergeNode1.MergeNodes(mergeNode2, useSourceValues, defaultValue);

            Settings.Writer.Write(loc1, Localization.SerializeNode(node1));
            Settings.Writer.Write(loc2, Localization.SerializeNode(node2));
        }






        private string GetPath(string path, string localization)
        {
            if(Settings.PathPrefix != "") path = LocalizationPath.Combine(Settings.PathPrefix, path).Path;
            if(Settings.UseSingleFile) path = LocalizationPath.Combine(localization, path).Path;
            
            return path;
        }


        private T SaveAfterModifying<T>(T returnValue, bool createdNew, string localization, LocalizationNode node)
        {
            if(!Settings.CanWrite) return returnValue;
            if(!createdNew) return returnValue;

            Settings.Writer.Write(localization, Localization.SerializeNode(node));

            return returnValue;
        }


        private LocalizationNode GetNode(string path, string localization, bool cacheNode = false)
        {
            if(!Settings.CanRead) throw new Exception();
            if(!Settings.Reader.CanRead(localization)) throw new Exception();
            LocalizationPath.Validate(path);

            var node = GetNodeWithCaching(cacheNode && Settings.EnableCaching, localization);
            return node.GetNode(new LocalizationPath(path));
        }




        private LocalizationNode GetNodeWithCaching(bool cachingEnabled, string localization) // very pretty
        {
            if(Settings.EnableCaching && CachedNode is not null && localization == CachedNode.Localization) return CachedNode.Node;
            if(Settings.EnableCaching && CachedNode is not null && Settings.UseSingleFile) return CachedNode.Node;
            
            string text = Settings.Reader.Read(localization);
            var node  =   Localization.DeserializeNode(text);
            if(cachingEnabled) CacheNode(localization, node);
            return node;
        }

        private void CacheNode(string localization, LocalizationNode node)
        {
            CachedNode = new (localization, node);
        }




        public Localizator(LocalizatorSettings settings)
        {
            Settings = settings;
            if(Settings is null) Settings = new();
        }

        public Localizator(ILocalizatorReader reader) : this(new LocalizatorSettings(reader)) {}
        public Localizator(ILocalizatorWriter writer) : this(new LocalizatorSettings(writer)) {}
    }
}