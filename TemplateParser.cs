using System.Text.RegularExpressions;

public class TemplateParser
{
    private List<string> _warnings = new List<string>();
    private Dictionary<string, string> _templateVars = new Dictionary<string, string>();
    private const string _delim = "``";
    private const char _openBrace = '(';
    private const char _closeBrace = ')';
    
    public string Parse(string templateDirectory, string content) {
        if(!content.Contains(_delim)) {
            return content;
        }

        content = ParseIncludes(templateDirectory, content);

        var match = Regex.Match(content, $"{_delim}.+{_delim}", RegexOptions.Multiline);

        if(!match.Success) {
            return content;
        }

        while(match.Success) {
            void Next() => match = match.NextMatch();

            var rawMatch = match.Value.ToString();
            string block = rawMatch.Replace(_delim, "").Trim();
            if(block.Length == 0) {
                Next();
                continue;
            }

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
        if(!block.Contains(_openBrace) || !block.Contains(_closeBrace)) {
            return content;
        }
        block = block.Remove(0, "out(".Length);
        int closeBracePosition = block.IndexOf(_closeBrace);
        if(closeBracePosition == 0) {
            return content;
        }
        string varName = block[0..closeBracePosition];
        if(!_templateVars.ContainsKey(varName)) {
            _warnings.Add($"Unexpected variable '{varName}'.");
            return content;
        }

        string extraCruft = block[closeBracePosition..(block.Length - 1)];
        if(extraCruft.Length > 0) {
            _warnings.Add($"Unexpected end of line content '{extraCruft}'.");
        }

        return content.Replace(rawMatch, _templateVars[varName] + extraCruft);
    }

    private string ParseVariable(string content, string rawMatch, string block) {
        if(!block.Contains(_openBrace) || !block.Contains(_closeBrace)) {
            return content;
        }
        block = block.Remove(0, 1);
        int openBracePosition = block.IndexOf(_openBrace);
        if(openBracePosition == 0) {
            return content;
        }
        string varName = block[0..openBracePosition];
        int closeBracePosition = block.IndexOf(_closeBrace);
        string varValue = block[(openBracePosition + 1)..closeBracePosition];
        _templateVars.Add(varName, varValue);

        string extraCruft = block[closeBracePosition..(block.Length - 1)];
        if(extraCruft.Length > 0) {
            _warnings.Add($"Unexpected end of line content '{extraCruft}'.");
        }

        return content.Replace(rawMatch, extraCruft);
    }

    private string ParseIncludes(string templateDirectory, string content) {
        const string token = "include";

        Match match = Regex.Match(content, $"{_delim}{token}\\{_openBrace}.+{_delim}", RegexOptions.Multiline);

        if(!match.Success) {
            return content;
        }

        while(match.Success) {
            void Next() => match = match.NextMatch();

            var rawMatch = match.Value.ToString();
            string block = rawMatch.Replace(_delim, "").Trim();
            if(block.Length == 0) {
                Next();
                continue;
            }

            if(!block.StartsWith(token)) {
                Next();
                continue;
            }

            if(!block.Contains(_openBrace) || !block.Contains(_closeBrace)) {
                Next();
                continue;
            }
            block = block.Remove(0, $"{token}{_openBrace}".Length);

            int closeBracePosition = block.IndexOf(_closeBrace);
            if(closeBracePosition == 0) {
                Next();
                continue;
            }

            string templateName = block[0..closeBracePosition];

            FileInfo templateFile = new FileInfo(
                templateDirectory + Path.DirectorySeparatorChar + templateName + ".htm");

            string extraCruft = block[closeBracePosition..(block.Length - 1)];
            if(extraCruft.Length > 0) {
                _warnings.Add($"Unexpected end of line content '{extraCruft}'.");
            }

            if(!templateFile.Exists) {
                _warnings.Add($"Cannot find requested include file '{templateFile.Name}'.");
                content = content.Replace(rawMatch, "" + extraCruft);
                Next();
                continue;
            }

            content = content.Replace(rawMatch, templateFile.OpenText().ReadToEnd() + extraCruft);

            Next();
        }

        return content;
    }
}
