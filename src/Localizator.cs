using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using LocalizationLib.Exceptions;

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
        private Dictionary<string, LocalizationNode> CachedNodes = new Dictionary<string, LocalizationNode>();
        
        




        public string GetString(string path) => GetStringInternal(path, CurrentLocalization);
        public string GetString(string path, string localization) => GetStringInternal(path, localization);
        private string GetStringInternal(string path, string localization)
        {
            path = GetPath(path, localization);
            var node = GetNode(path, localization);
            return node.GetString();
        }





        public string GetStringFormat(string path, params object[] obj)
        {
            var str = GetString(path);
            str = string.Format(str, obj);
            return str;
        }


        


        public string GetStringInit(string path, string initValue) => GetStringInitInternal(path, CurrentLocalization, initValue);
        public string GetStringInit(string path, string localization, string initValue) => GetStringInitInternal(path, localization, initValue);
        private string GetStringInitInternal(string path, string localization, string initValue)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);
            var str = node.GetStringInit(new LocalizationPath(path), initValue, out bool createdNew);

            return SaveAfterModifying(str, createdNew, localization, node);
        }





        public List<string> GetArray(string path) => GetArrayInternal(path, CurrentLocalization);
        public List<string> GetArray(string path, string localization) => GetArrayInternal(path, localization);
        private List<string> GetArrayInternal(string path, string localization)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);
            var array = node.GetArray(new LocalizationPath(path));

            return array;
        }
        
        
        
        
        
        public string GetArrayElement(string path, int elementIndex) => GetArrayElementInternal(path, CurrentLocalization, elementIndex);
        public string GetArrayElement(string path, string localization, int elementIndex) => GetArrayElementInternal(path, localization, elementIndex);
        private string GetArrayElementInternal(string path, string localization, int elementIndex)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);
            var element = node.GetArrayElement(new LocalizationPath(path), elementIndex);

            return element;
        }





        public AdditionResult AddString(string path, string stringName, string stringContent) => AddStringInternal(path, CurrentLocalization, stringName, stringContent);
        public AdditionResult AddString(string path, string localization, string stringName, string stringContent) => AddStringInternal(path, localization, stringName, stringContent);
        private AdditionResult AddStringInternal(string path, string localization, string stringName, string stringContent)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);

            var response = node.AddString(new LocalizationPath(path), stringName, stringContent);
            var createdNew = response.HasFlag(AdditionResult.SucceededAdding);

            return SaveAfterModifying(response, createdNew, localization, node);
        }





        public bool AddStrings(string path, params StringInfo[] strings) => AddStringsInternal(path, CurrentLocalization, strings);
        public bool AddStrings(string path, string localization, params StringInfo[] strings) => AddStringsInternal(path, localization, strings);
        private bool AddStringsInternal(string path, string localization, params StringInfo[] strings)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);

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





        public AdditionResult AddCategory(string path, string categoryName) => AddCategoryInternal(path, CurrentLocalization, categoryName);
        public AdditionResult AddCategory(string path, string localization, string categoryName) => AddCategoryInternal(path, localization, categoryName);
        private AdditionResult AddCategoryInternal(string path, string localization, string categoryName)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);

            var response = node.AddCategory(new LocalizationPath(path), categoryName);
            var createdNew = response.HasFlag(AdditionResult.SucceededAdding);

            return SaveAfterModifying(response, createdNew, localization, node);
        }





        public bool AddCategories(string path, List<string> categoryNames) => AddCategoriesInternal(path, CurrentLocalization, categoryNames);
        public bool AddCategories(string path, string localization, List<string> categoryNames) => AddCategoriesInternal(path, localization, categoryNames);
        private bool AddCategoriesInternal(string path, string localization, List<string> categoryNames)
        {
            path = GetPath(path, localization);
            var node = GetNode("", localization);

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
            if(!Settings.CanWrite) throw new CannotWriteException();

            LocalizationNode node1 = GetNode("", loc1);
            LocalizationNode node2 = GetNode("", loc2);

            var mergeNode1 = node1.GetNode(new LocalizationPath(Settings.UseSingleFile ? loc1 : ""));
            var mergeNode2 = node2.GetNode(new LocalizationPath(Settings.UseSingleFile ? loc2 : ""));

            mergeNode1.MergeNodes(mergeNode2, useSourceValues, defaultValue);

            Settings.Writer.Write(loc1, Localization.SerializeNode(node1));
            Settings.Writer.Write(loc2, Localization.SerializeNode(node2));
        }

        public bool AreLocalizationsEquivalent(string loc1, string loc2)
        {
            LocalizationNode a = GetNode("", loc1);
            LocalizationNode b = GetNode("", loc2);

            a = a.GetNode(new LocalizationPath(Settings.UseSingleFile ? loc1 : ""));
            b = b.GetNode(new LocalizationPath(Settings.UseSingleFile ? loc2 : ""));

            return a.IsEquivalentTo(b);
        }

        public void UpdateLocalization(string localization)
        {
            if(!Settings.CanRead) throw new CannotReadException();
            if(!Settings.Reader.CanRead(localization)) throw new CannotReadLocalizationException(localization);
            
            string text = Settings.Reader.Read(localization);
            var node  =   Localization.DeserializeNode(text);
            CacheNode(localization, node);
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


        private LocalizationNode GetNode(string path, string localization)
        {
            if(!Settings.CanRead) throw new CannotReadException();
            if(!Settings.Reader.CanRead(localization)) throw new CannotReadLocalizationException(localization);
            LocalizationPath.Validate(path);

            var node = GetNodeWithCaching(localization);
            return node.GetNode(new LocalizationPath(path));
        }




        private LocalizationNode GetNodeWithCaching(string localization, bool forceCache = false)
        {
            if(Settings.EnableCaching && CachedNodes.ContainsKey(localization)) return CachedNodes[localization];
            if(Settings.EnableCaching && Settings.UseSingleFile && CachedNodes.Count != 0) return CachedNodes.Single().Value;
            
            string text = Settings.Reader.Read(localization);
            var node  =   Localization.DeserializeNode(text);
            if(Settings.EnableCaching) CacheNode(localization, node);
            return node;
        }

        private void CacheNode(string localization, LocalizationNode node)
        {
            CachedNodes[localization] = node;
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