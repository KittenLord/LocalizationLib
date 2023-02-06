A C# library for localization, allowing for clean and organized localization tree.

You can write localization in json using following example:

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

To access elements you use paths like these:

```
element1
category1.innerCategory1.innerInnerElement
category2.element
```

You can also change the separator from '.' to '/' in LocalizationPath class

To use library, initialize a Localizator object.

```cs
Localizator localizator = new Localizator(new LocalizatorSettings(
    new LocalizatorFileReader("your path"), // reads from localization/{localizationName}.json
    new LocalizatorFileWriter("your path")  // writes to  localization/{localizationName}.json
));
```

You can use other classes inheriting ILocalizatorReader and ILocalizatorWriter, and even create your own.

Then you can use Localization.InitLocalizator(localizator) to set the static class' singleton, or use the Localizator object as it is.

There's also an option to use multiple localizations within a single file, if that's what you need. Initialize your Localizator like this:

```cs
Localizator localizator = new Localizator(new LocalizatorSettings(
    new LocalizationConstantFileReader("fileName.json"), // reads from this file only
    new LocalizationConstantFileWriter("fileName.json")) // writes to  this file only
    {
        UseSingleFile = true
    });
```

You can then use it as the previous one, without needing to use localization's prefixes. For example:

```cs
localizator.SetLocalization("eng");
localizator.GetString("element"); // reads from path "eng.element"

localizator.SetLocalization("rus");
localizator.GetString("category.element"); // reads from path rus.category.element
```

You can also do following things through code
  - Add new element in a category
  - Add new category into another category
  - Try getting element, and if it doesn't exist, create one (needs valid ILocalizatorWriter)
  - Check if to localizations have the same "schema", i.e. share the same elements and categories, no more no less
  - Merge two localizations, i.e. add all the elements which miss from one another. You can also select the default value for newly created elements
