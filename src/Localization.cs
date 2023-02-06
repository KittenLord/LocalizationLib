using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace LocalizationLib
{
    public static class Localization
    {
        private static Localizator? _singleton;
        
        public static Localizator GetSingleton()
        {
            if(_singleton is null) throw new NullReferenceException();
            return _singleton;
        }
        public static Localizator InitLocalizator(LocalizatorSettings settings)
        {
            _singleton = new Localizator(settings);
            return _singleton;
        }
        public static void InitLocalizator(Localizator loc)
        {
            if(loc is null) throw new Exception();
            if(loc.Settings is null) loc.Settings = new LocalizatorSettings();
            _singleton = loc;
        }






        private static T PerformSingletonMethod<T>(Func<Localizator, T> action)
        {
            if(_singleton is null) throw new Exception();
            return action(_singleton);
        }



        public static void SetLocalization(string localization) => PerformSingletonMethod(loc => loc.SetLocalization(localization));


        public static string Get(string path) => GetString(path);
        public static string Get(string path, string localization) => GetString(path, localization);
        public static string GetInit(string path, string initValue) => GetStringInit(path, initValue);
        public static string GetInit(string path, string localization, string initValue) => GetStringInit(path, localization, initValue);




        public static string GetString(string path) => PerformSingletonMethod(loc => loc.GetString(path));
        public static string GetString(string path, string localization) => PerformSingletonMethod(loc => loc.GetString(path, localization));
        public static string GetStringInit(string path, string initValue) => PerformSingletonMethod(loc => loc.GetStringInit(path, initValue));
        public static string GetStringInit(string path, string localization, string initValue) => PerformSingletonMethod(loc => loc.GetStringInit(path, localization, initValue));
        public static AdditionResult AddString(string categoryPath, string stringName, string stringValue) => PerformSingletonMethod(loc => loc.AddString(categoryPath, stringName, stringValue));
        public static AdditionResult AddString(string categoryPath, string localization, string stringName, string stringValue) => PerformSingletonMethod(loc => loc.AddString(categoryPath, localization, stringName, stringValue));
        public static AdditionResult AddCategory(string categoryPath, string categoryName) => PerformSingletonMethod(loc => loc.AddCategory(categoryPath, categoryName));
        public static AdditionResult AddCategory(string categoryPath, string localization, string categoryName) => PerformSingletonMethod(loc => loc.AddCategory(categoryPath, localization, categoryName));
        public static bool AddStrings(string categoryPath, params StringInfo[] strings) => PerformSingletonMethod(loc => loc.AddStrings(categoryPath, strings));
        public static bool AddStrings(string categoryPath, string localization, params StringInfo[] strings) => PerformSingletonMethod(loc => loc.AddStrings(categoryPath, localization, strings));
        public static bool AddCategories(string categoryPath, List<string> categories) => PerformSingletonMethod(loc => loc.AddCategories(categoryPath, categories));
        public static bool AddCategories(string categoryPath, string localization, List<string> categories) => PerformSingletonMethod(loc => loc.AddCategories(categoryPath, localization, categories));
        
        






        internal static LocalizationNode DeserializeNode(string json)
        {
            var node = JsonConvert.DeserializeObject<LocalizationNode>(json, new LocalizationConverter());
            if(node is null) node = new LocalizationNode(new Dictionary<string, LocalizationNode>());
            return node;
        }

        internal static string SerializeNode(LocalizationNode node)
        {
            var json = JsonConvert.SerializeObject(node, Formatting.Indented, new LocalizationConverter());
            return json;
        }
    }
}