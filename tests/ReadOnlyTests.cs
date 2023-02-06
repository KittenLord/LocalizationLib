namespace Tests;

public static class Const
{
    public const string SampleFolder = "../../../sample/localization/";
    public const string SampleFile = "../../../sample/localization.json";
}

public class ReadOnlyTests
{
    [Fact]
    public void AreEquivalentSeparateFiles()
    {
        Localizator loc = new Localizator(new LocalizatorSettings(
            new LocalizatorFileReader(Const.SampleFolder)
        ));

        Assert.True(loc.AreLocalizationsEquivalent("eng", "esp"));
        Assert.True(loc.AreLocalizationsEquivalent("eng", "rus"));
        Assert.True(loc.AreLocalizationsEquivalent("eng", "deu"));
        
        Assert.True(loc.AreLocalizationsEquivalent("engs", "esps"));
        Assert.True(loc.AreLocalizationsEquivalent("engs", "russ"));
        Assert.True(loc.AreLocalizationsEquivalent("engs", "deus"));
    }

    [Fact]
    public void AreEquivalentSameFile()
    {
        Localizator loc = new Localizator(new LocalizatorSettings(
            new LocalizationConstantFileReader(Const.SampleFile))
            {
                UseSingleFile = true
            });

        Assert.True(loc.AreLocalizationsEquivalent("eng", "esp"));
        Assert.True(loc.AreLocalizationsEquivalent("eng", "rus"));
        Assert.True(loc.AreLocalizationsEquivalent("eng", "deu"));

        Assert.True(loc.AreLocalizationsEquivalent("engs", "esps"));
        Assert.True(loc.AreLocalizationsEquivalent("engs", "russ"));
        Assert.True(loc.AreLocalizationsEquivalent("engs", "deus"));
    }

    [Fact]
    public void AreNotEquivalentSeparateFiles()
    {
        Localizator loc = new Localizator(new LocalizatorSettings(
            new LocalizatorFileReader(Const.SampleFolder)
        ));

        Assert.False(loc.AreLocalizationsEquivalent("eng", "engs"));
        Assert.False(loc.AreLocalizationsEquivalent("rus", "russ"));
        Assert.False(loc.AreLocalizationsEquivalent("deu", "deus"));
        Assert.False(loc.AreLocalizationsEquivalent("esp", "esps"));
    }
    

    [Fact]
    public void AreNotEquivalentSameFile()
    {
        Localizator loc = new Localizator(new LocalizatorSettings(
            new LocalizationConstantFileReader(Const.SampleFile))
            {
                UseSingleFile = true
            });

        Assert.False(loc.AreLocalizationsEquivalent("eng", "engs"));
        Assert.False(loc.AreLocalizationsEquivalent("rus", "russ"));
        Assert.False(loc.AreLocalizationsEquivalent("deu", "deus"));
        Assert.False(loc.AreLocalizationsEquivalent("esp", "esps"));
    }
}