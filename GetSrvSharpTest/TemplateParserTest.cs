using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GetSrvSharpTest;
public class TemplateParserTest {
    [Fact]
    public void TestRemoveDelim() {
        Assert.Equal("", TemplateParser.RemoveDelim("````"));
        Assert.Equal("test", TemplateParser.RemoveDelim("  ``test``  "));
        Assert.Equal("out(name)", TemplateParser.RemoveDelim("   ``out(name)  ``"));
    }

    [Fact]
    public void TestParseOut() {
        var parser = new TemplateParser();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        List<string>? warningsMember = parser.GetType().GetField("_warnings", flags)!.GetValue(parser) as List<string>;
        var templateVarsMember = parser.GetType().GetField("_templateVars", flags);
        templateVarsMember!.SetValue(parser, new Dictionary<string, string>() {
            { "name", "Name Value" },
        });
        string original =
            @"<p>
                <b>``out(name)``</b><br>
                <i>``out(age)``</i><br>
            </p>";
        string replaced =
            @"<p>
                <b>Name Value</b><br>
                <i>``out(age)``</i><br>
            </p>";
        Assert.Equal(replaced, parser.ParseOut(original, "``out(name)``"));
        Assert.Empty(warningsMember);
        Assert.Equal(original, parser.ParseOut(original, "``out(age)``"));
        Assert.Contains("Undefined variable 'age'.", warningsMember);
        Assert.Equal(original, parser.ParseOut(original, "``out(invalid)``"));
        Assert.Contains("Undefined variable 'invalid'.", warningsMember);
    }

    [Fact]
    public void TestParseVariable() {
        var parser = new TemplateParser();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var templateVarsMember = parser.GetType().GetField("_templateVars", flags);
        string original =
            @"``@name(Name Value)``
            <b>Other stuff</b>";
        string replaced =
            @"
            <b>Other stuff</b>";
        Assert.Equal(replaced, parser.ParseVariable(original, "``@name(Name Value)``"));
        var templateVars = templateVarsMember!.GetValue(parser) as Dictionary<string, string>;
        Assert.Contains("name", templateVars!.Keys);
        Assert.Contains("Name Value", templateVars.Values);
    }
}