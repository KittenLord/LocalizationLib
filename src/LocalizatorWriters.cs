using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationLib.Exceptions;

namespace LocalizationLib
{
    public interface ILocalizatorWriter
    {
        public void Write(string localization, string text);
    }

    public class LocalizatorFileWriter : ILocalizatorWriter
    {
        public string FolderPath { get; set; }
        public string FileExtension { get; set; }

        private string GetPath(string localization) => Path.Combine(FolderPath, localization + FileExtension);

        public void Write(string localization, string text)
        {
            File.WriteAllText(GetPath(localization), text);
        }
        
        public LocalizatorFileWriter(string folder, string fileExtension = ".json")
        {
            if(!fileExtension.StartsWith(".")) throw new IncorrectFileExtenstionException(fileExtension);
            FolderPath = folder;
            FileExtension = fileExtension;
        }
    }

    public class LocalizationConstantFileWriter : ILocalizatorWriter
    {
        public string FilePath { get; set; }

        public void Write(string _, string text) => File.WriteAllText(FilePath, text);

        public LocalizationConstantFileWriter(string filePath)
        {
            FilePath = filePath;
        }
    }
}