using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalizationLib
{
    public class LocalizedObject<T>
    {
        public string Path { get; init; }
        public T Value => GetValue();


        private Func<Localizator> GetLocalizator { get; init; }
        private Func<string, T> Converter { get; init; }
        

        private T GetValue()
        {
            if(GetLocalizator is null) throw new Exception();
            if(Path is null) throw new Exception();
            if(Converter is null) throw new Exception();

            var localizator = GetLocalizator();
            string value = localizator.GetStringSafe(Path);

            var obj = Converter(value);

            return obj;
        }


        public override string? ToString() => Value?.ToString();
        public static implicit operator T(LocalizedObject<T> obj) => obj.Value;

        
        public LocalizedObject(string path) : this(path, () => Localization.GetSingleton(), (s) => (T)(object)s) {}
        public LocalizedObject(string path, Func<Localizator> getLocalizator) : this(path, getLocalizator, (s) => (T)(object)s) {}
        public LocalizedObject(string path, Func<string, T> converter) : this(path, () => Localization.GetSingleton(), converter) {}
        public LocalizedObject(string path, Func<Localizator> getLocalizator, Func<string, T> converter)
        {
            Path = path;
            GetLocalizator = getLocalizator;
            Converter = converter;
        }
    }

    public class LocalizedString : LocalizedObject<string>
    {
        public LocalizedString(string path) : base(path, () => Localization.GetSingleton(), (s) => s) {}
    }
}