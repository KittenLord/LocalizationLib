using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationLib.Exceptions;

namespace LocalizationLib
{   
    public interface ILocalizatorReader
    {
        public bool CanRead(string localization);
        public string Read(string localization);
    }




    public class LocalizatorFileReader : ILocalizatorReader
    {
        public string FolderPath { get; set; }
        public string FileExtension { get; set; } = ".json";
        
        private string GetPath(string localization) => Path.Combine(FolderPath, localization + FileExtension);
        public bool CanRead(string localization) => File.Exists(GetPath(localization));
        public string Read(string localization) => File.ReadAllText(GetPath(localization));

        public LocalizatorFileReader(string folder, string fileExtension = ".json")
        {
            if(!fileExtension.StartsWith(".")) throw new IncorrectFileExtenstionException(fileExtension);
            FolderPath = folder;
            FileExtension = fileExtension;
        }
    }

    public class LocalizationTextReader : ILocalizatorReader
    {
        public string JsonText { get; set; }
        public bool CanRead(string localization) => JsonText is not null;
        public string Read(string localization) { return JsonText; }

        public LocalizationTextReader(string jsonText)
        {
            JsonText = jsonText;
        }
    }

    public class LocalizationConstantFileReader : ILocalizatorReader
    {
        public string FilePath { get; set; }

        public bool CanRead(string localization) => File.Exists(FilePath);
        public string Read(string localization) => File.ReadAllText(FilePath);

        public LocalizationConstantFileReader(string filePath)
        {
            FilePath = filePath;
        }
    }
}