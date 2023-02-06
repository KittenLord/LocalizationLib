using System;
using LocalizationLib;

namespace Examples
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Localizator localizator = new Localizator(new LocalizatorSettings(
                new LocalizatorFileReader("localization"), // reads from localization/{localization}.json
                new LocalizatorFileWriter("localization")  // writes to  localization/{localization}.json
            ));
            Localization.InitLocalizator(localizator);





            string[] supportedLocalizations = { "eng", "esp", "deu", "rus" };
            Console.Write("Select your localization. ");
            Console.WriteLine("Supported localizations are: " + string.Join(", ", (supportedLocalizations)));
            string? localization = Console.ReadLine();
            if(localization is null || !supportedLocalizations.Contains(localization))
            {
                Console.Write("You selected invalid localization! ");
                Console.WriteLine("Supported localizations are: " + string.Join(", ", (supportedLocalizations)));
                return;
            }
            Localization.SetLocalization(localization);





            string[] paths = { "welcome", 
                               "fruit.apple", "fruit.banana", "fruit.pear", 
                               "subject.math.name", "subject.math.description", 
                               "subject.literature.name", "subject.literature.description", 
                               "subject.music.name", "subject.music.description" };
            string[] results = paths.Select(path => Localization.Get(path)).ToArray();
            int longest = paths.OrderBy(path => path.Length).Last().Length;





            Console.WriteLine();
            Console.WriteLine("Localization: " + localization);
            for(int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                string result = results[i];
                Console.WriteLine($"Path: {path + new string(' ', longest - path.Length)} | Result: {result}");
            }
        }
    }
}