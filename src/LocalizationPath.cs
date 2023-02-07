using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationLib.Exceptions;

namespace LocalizationLib
{
    public class LocalizationPath
    {
        public string Path { get; private set; }

        private const string allowedSeparators = "./";
        public static string Separator { get; private set; } = ".";
        public static string DoubleSeparator => Separator + Separator;
        public static void SetSeparator(string newSeparator)
        {
            if(newSeparator.Length != 1) throw new InvalidPathSeparatorException(newSeparator);
            if(!allowedSeparators.Contains(Separator)) throw new InvalidPathSeparatorException(newSeparator);

            Separator = newSeparator;
        }

        private const string forbiddenPathCharacters = " \"\n\r\t\\";
        public bool IsValid() => IsValid(Path);
        public static bool IsValid(string path)
        {
            var forbidden = forbiddenPathCharacters + Separator;
            if(path is null) return false;
            if(path == "") return true;

            var s = path.Split(Separator);

            if(s.Any(c => c.Any(b => forbidden.Contains(b)))) return false;

            return true;
        }
        public void Validate() => Validate(Path);
        public static void Validate(string path)
        {
            if(!IsValid(path)) throw new InvalidPathException(path);
        }

        public static string ClearOfDoubleSeparators(string s)
        {
            while(s.Contains(DoubleSeparator)) s = s.Replace(DoubleSeparator, Separator);
            return s;
        }


        public static LocalizationPath Combine(params string[] a) => new LocalizationPath(ClearOfDoubleSeparators(string.Join(Separator, a)));
        public static LocalizationPath Combine(LocalizationPath a, string s) => Combine(a.Path, s);
        public static LocalizationPath Combine(string a, LocalizationPath s) => Combine(a, s.Path);
        public static LocalizationPath Combine(LocalizationPath a, LocalizationPath s) => Combine(a.Path, s.Path);
        



        public string ChopOff(bool changeThis = true)
        {
            if(Path == "") throw new CannotOperateOnPath(Path);
            var p = Path.Split(Separator).ToList();

            var part = p.ElementAt(0);
            p.RemoveAt(0);

            var newPath = string.Join(Separator, p);

            if(changeThis)
                Path = newPath;
            return part;
        }
        public string RemoveLast(bool changeThis = true)
        {
            if(Path == "") throw new CannotOperateOnPath(Path);
            var p = Path.Split(Separator).ToList();

            var last = p.Last();
            p.RemoveAt(p.Count - 1);

            var newPath = string.Join(Separator, p);

            if(changeThis)
                Path = newPath;
            return last;
        }
        public LocalizationPath Copy()
        {
            return new LocalizationPath(Path);
        }





        public LocalizationPath(string path)
        {
            if(!IsValid(path)) throw new InvalidPathException(path);
            Path = path;
        }
    }
}