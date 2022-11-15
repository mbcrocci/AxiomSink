namespace Serilog.Sinks.Axiom.Tests;

public class FormatterTests
{
    [Fact]
    public void FixesLevel()
    {
        var f = new LogFormatter();

        Assert.Equal("warn", f.FixLevel("warning"));
        Assert.Equal("info", f.FixLevel("information"));
        Assert.Equal("error", f.FixLevel("error"));
    }

    [Fact]
    public void RemovesKeys()
    {
        var f = new LogFormatter();

        var dict = new Dictionary<string, object> { { "key1", new { } }, { "key2", new { } }, { "key3", new { } } };
        f.RemoveKey(dict, "key1");
        f.RemoveKey(dict, "key3");

        Assert.Contains("key2", dict.Keys);
        Assert.DoesNotContain("key1", dict.Keys);
        Assert.DoesNotContain("key3", dict.Keys);
    }

    [Fact]
    public void RemovesKeysFromRemovals()
    {
        var f = new LogFormatter(removals: new List<string> { "key1", "key3" }, renames: new()); ;

        Assert.Contains("key1", f.Removals);
        Assert.Contains("key3", f.Removals);

        var dict = new Dictionary<string, object> { { "key1", new { } }, { "key2", new { } }, { "key3", new { } } };
        f.ApplyRemovals(dict);

        Assert.Contains("key2", dict.Keys);
        Assert.DoesNotContain("key1", dict.Keys);
        Assert.DoesNotContain("key3", dict.Keys);
    }

    [Fact]
    public void RemovesSupersetOfKeysFromRemovals()
    {
        var f = new LogFormatter(removals: new List<string> { "key1", "key3" }, renames: new()); ;

        Assert.Contains("key1", f.Removals);
        Assert.Contains("key3", f.Removals);

        var dict = new Dictionary<string, object> { { "key1", new { } }, { "key2", new { } }, { "key3", new { } }, { "key1.subkey", new { } } };
        f.ApplyRemovals(dict);

        Assert.Contains("key2", dict.Keys);
        Assert.DoesNotContain("key1", dict.Keys);
        Assert.DoesNotContain("key3", dict.Keys);
        Assert.DoesNotContain("key1.subkey", dict.Keys);
    }

    [Fact]
    public void RenamesKeys()
    {
        var f = new LogFormatter();

        var dict = new Dictionary<string, object> { { "key1", new { } }, { "key2", new { } }, { "key3", new { } } };
        f.RenameKey(dict, "key1", "mod1");
        f.RenameKey(dict, "key3", "mod3");

        Assert.Contains("key2", dict.Keys);
        Assert.Contains("mod1", dict.Keys);
        Assert.Contains("mod3", dict.Keys);

        Assert.DoesNotContain("key1", dict.Keys);
        Assert.DoesNotContain("key3", dict.Keys);
    }
}
