using System.Text.RegularExpressions;

public class TemplateParser
{
    private List<string> _warnings = new List<string>();
    private Dictionary<string, string> _templateVars = new Dictionary<string, string>();
    private const string _delim = "``";
    private const char _openBrace = '(';
    private const char _closeBrace = ')';
    private const string _whitespace = @"[ \t]";
    
    public string Parse(string templateDirectory, string content) {
        if(!content.Contains(_delim)) {
            return content;
        }

        content = ParseIncludes(templateDirectory, content);

        var match = Regex.Match(content, @$"{_delim}{_whitespace}*.+\{_openBrace}.+\{_closeBrace}{_delim}", RegexOptions.Multiline);

        if(!match.Success) {
            return content;
        }

        while(match.Success) {
            void Next() => match = match.NextMatch();

            var rawMatch = match.Value.ToString();
            string block = rawMatch.Replace(_delim, "").Trim();

            if(block.StartsWith('@')) {
                content = ParseVariable(content, rawMatch, block);
                Next();
                continue;
            }

            if(block.StartsWith("out")) {
                content = ParseOut(content, rawMatch, block);
                Next();
                continue;
            }

            Next();
        }

        return content;
    }

    private string ParseOut(string content, string rawMatch, string block) {
        block = block.Remove(0, $"out{_openBrace}".Length);
        block = block.Remove(block.Length - 1, 1);
        if(!_templateVars.ContainsKey(block)) {
            _warnings.Add($"Unexpected variable '{block}'.");
            return content;
        }

        return content.Replace(rawMatch, _templateVars[block]);
    }

    private string ParseVariable(string content, string rawMatch, string block) {
        block = block.Remove(0, 1);
        int openBracePosition = block.IndexOf(_openBrace);
        string varName = block[0..openBracePosition];
        int closeBracePosition = block.IndexOf(_closeBrace);
        string varValue = block[(openBracePosition + 1)..closeBracePosition];
        _templateVars.Add(varName, varValue);

        return content.Replace(rawMatch, "");
    }

    private string ParseIncludes(string templateDirectory, string content) {
        const string token = "include";

        Match match = Regex.Match(content, @$"{_whitespace}*{_delim}{token}\{_openBrace}.+\{_closeBrace}{_delim}", RegexOptions.Multiline);

        if(!match.Success) {
            return content;
        }

        while(match.Success) {
            void Next() => match = match.NextMatch();

            var rawMatch = match.Value.ToString();
            string block = rawMatch.Replace(_delim, "").Trim();
            block = block.Remove(0, $"{token}{_openBrace}".Length);
            block = block.Remove(block.Length - 1, 1);

            FileInfo templateFile = new FileInfo(
                templateDirectory + Path.DirectorySeparatorChar + block + ".htm");

            if(!templateFile.Exists) {
                _warnings.Add($"Cannot find requested include file '{templateFile.Name}'.");
                content = content.Replace(rawMatch, "");
                Next();
                continue;
            }

            content = content.Replace(rawMatch, templateFile.OpenText().ReadToEnd());

            Next();
        }

        return content;
    }
}
