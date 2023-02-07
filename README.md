A C# library for localization, allowing for clean and organized localization tree.

Using this library, localization can be written in a tree form like this one below:

```json
{
  "element1": "value1",
  "element2": "value2",
  "category1": {
    "innerElement1": "value",
    "innerElement2": "value",
    "innerCategory1": {
      "innerInnerElement": "value"
    },
    "innerElement3": "value"
  },
  "element3": "value2",
  "category2": {
    "element": "value"
  }
}
```

Paths using following syntax can be used to access the elements through the code:

```
element1
category1.innerCategory1.innerInnerElement
category2.element
```

Separator in path can be changed from '.' to '/' if needed. Other separators are forbidden (for now?).

To start using the library, initialize an object of class `Localizator`:

```cs
Localizator localizator = new Localizator(new LocalizatorSettings(
    new LocalizatorFileReader("your_path"), // reads from your_path/{localizationName}.json
    new LocalizatorFileWriter("your_path")  // writes to  your_path/{localizationName}.json
));
```

Other classes can be used as a `Reader` and as a `Writer`, as long as they implement `ILocalizatorReader` or `ILocalizatorWriter` interface respectively. `Writer` can be left uninitialized, as it is not strictly required for localization process, but failing to initialize `Reader` will most likely result in exceptions.

Pre-implemented readers (for now) are:
- `LocalizationFileReader(folderPath, fileExtension = ".json")` - reads from a file in the specified folder. Each language requires a separate file, named exactly after the localization name (Example: folderPath/eng.json).
- `LocalizationConstantFileReader(filePath)` - reads only from the specified file, regardless of selected localization (primarily used if all languages are stored in the same file).
- `LocalizationTextReader(text)` - *reads* a constant string value.

Pre-implemented writers (also for now) are:
- `LocalizatorFileWriter(folderPath, fileExtension = ".json")` - writes to a file in the specified folder. File naming rules are the same as in `LocalizationFileReader` (look above).
- `LocalizationConstantFileWriter(filePath)` - writes only to the specified file, regardless of selected localization (primarily used if all languages are stored in the same file).

To get data from the localization file(-s), either set the singleton of a static Localization class, or use the newly created Localizator object as it is.

```cs
// Localization.InitLocalizator(localizator); // Localization.GetSingleton() can be used, to retrieve it back
// Localization.SetLocalization("eng");
// Localization.Get("element"); // reads from file "your_path/eng.json", from path "element"

localizator.SetLocalization("eng");
localizator.GetString("element"); // reads from file "your_path/eng.json", from path "element"

localizator.SetLocalization("rus");
localizator.GetString("category.element"); // reads from file "your_path/rus.json", from path "category.element"
localizator.GetString("category.element", "eng"); // reads from file "your_path/eng.json", from path "category.element"
```
`your_path/eng.json`
```json
{
  "element": "value",
  "category": {
    "element": "value"
  }
}
```
`your_path/rus.json`
```json
{
  "element": "value",
  "category": {
    "element": "value"
  }
}
```

If there is a need to fit all languages in a single file, property `UseSingleFile` of `LocalizatorSettings` object can be set to true:

```cs
Localizator localizator = new Localizator(new LocalizatorSettings(
    new LocalizationConstantFileReader("fileName.json"), // reads from this file only
    new LocalizationConstantFileWriter("fileName.json")) // writes to  this file only
    {
        UseSingleFile = true
    });
```

Localizator can then be used in the exact same way as in the previous example. Note that your file will need a root category for each language:

```cs
// Localization.InitLocalizator(localizator);
// Localization.SetLocalization("eng");
// Localization.Get("element"); // reads from file "fileName.json", from path "eng.element"

localizator.SetLocalization("eng");
localizator.GetString("element"); // reads from file "fileName.json", from path "eng.element"

localizator.SetLocalization("rus");
localizator.GetString("category.element"); // reads from file "fileName.json", from path rus.category.element
localizator.GetString("category.element", "eng"); // reads from file "fileName.json", from path eng.category.element
```
`fileName.json`
```json
{
  "eng": {
    "element": "value",
    "category": {
      "element": "value"
    }
  },
  "rus": {
    "element": "value",
    "category": {
      "element": "value"
    }
  }
}
```

There is also a possibility to modify the existing localization(-s) through code. It is possible to:
  - Add a new element into an existing category.
  - Add a new category into another existing category.
  - Try to get an element, and in case if it doesn't exist, create it and initialize with some value.
  - Verify that two localizations are using the same "schema", that meaning, they have the exact same "folder" structure.
  - Merge two localizations, adding missing elements and categories from one to another. After a successful (!) merge, two structures are guaranteed (I guess? Haven't tested it yet, lol) to have the same structure.

If there is any confusion left, there are examples and tests free to view, download and experiment with.
