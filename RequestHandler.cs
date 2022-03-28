using System.Text;

public class RequestHandler
{
    private string _rawRequest;
    private Config _config;
    private string _resource;
    private ResourceType _resourceType;
    private ResponseCode _responseCode;
    private Action<string> Log { get; }

    public enum ResourceType
    {
        HTML, CSS, PNG, JPG
    }

    public enum ResponseCode
    {
        OK = 200,
        NOT_FOUND = 404,
        SERVER_ERROR = 500
    }

    public RequestHandler(Action<string> log, string rawRequest, Config config)
    {
        Log = log;
        _rawRequest = rawRequest;
        _config = config;
        Process();
    }

    private void Process()
    {
        using(var reader = new StringReader(_rawRequest))
        {
            string? line = reader.ReadLine();

            if(line == null)
            {
                _responseCode = ResponseCode.SERVER_ERROR;
                Log("Empty request.");
                return;
            }

            if(line.Substring(0, 3) != "GET")
            {
                _responseCode = ResponseCode.SERVER_ERROR;
                Log("Invalid request type.");
                return;
            }

            int start = line.IndexOf("/") + 1;
            int end = line.IndexOf(" ", start);
            _resource = line.Substring(start, end - start);

            start = ++end;
            string httpVersion = line.Substring(start, line.Length - end);
            if(httpVersion != "HTTP/1.1")
            {
                _responseCode = ResponseCode.SERVER_ERROR;
                Log("Invalid HTTP version.");
                return;
            }
        }

        if(_resource.Length == 0)
        {
            _resourceType = ResourceType.HTML;
            return;
        }

        int period = _resource.LastIndexOf(".") + 1;
        switch(_resource.Substring(period, _resource.Length - period).ToLower())
        {
            case "css":
                _resourceType = ResourceType.CSS;
                break;
            case "png":
                _resourceType = ResourceType.PNG;
                break;
            case "jpg":
                _resourceType = ResourceType.JPG;
                break;
            case "html":
            case "htm":
            default:
                _resourceType = ResourceType.HTML;
                break;
        }
    }

    public string GetResponse()
    {
        _responseCode = ResponseCode.NOT_FOUND;
        string body = GetBody();
        return GetHeaders(body.Length) + body;
    }

    public string GetHeaders(int contentLength)
    {
        var sb = new StringBuilder();
        sb.AppendLine("HTTP/1.1 " + (int)_responseCode + " " + _responseCode.ToHeaderString());
        sb.AppendLine("Content-Length: " + contentLength.ToString());
        sb.AppendLine("Content-Type: " + _resourceType.ToHeaderString());
        sb.Append('\n');
        return sb.ToString();
    }

    public string GetBody()
    {
        if(!_config.GetContentDirectory().Exists)
        {
            _responseCode = ResponseCode.NOT_FOUND;
            // TODO: Get default or custom 404 page
            return "<html><title>Not Found</title></head><body>Not Found</body></html>";
        }

        if(_resource == "")
        {
            _resource = _config.DefaultPage;
        }

        FileInfo requestedFile = new FileInfo(
            _config.GetContentDirectory().FullName + Path.DirectorySeparatorChar + _resource);

        if(!requestedFile.Exists)
        {
            Log("Request resource " + _resource + " was not found.");
            _responseCode = ResponseCode.NOT_FOUND;
            // TODO: Get default or custom 404 page
            return "<html><title>Not Found</title></head><body>Not Found</body></html>";
        }

        string content = requestedFile.OpenText().ReadToEnd();

        if(!_config.TemplateEngineEnabled)
        {
            _responseCode = ResponseCode.OK;
            return content;
        }

        // The base content file was loaded so we're going to count this as a 200
        // even if the template parsing fails. That should gracefully fail and return
        // the unparsed content if so.
        _responseCode = ResponseCode.OK;
        return ParseTemplate(content);
    }

    public string ParseTemplate(string content)
    {
        if(!_config.GetTemplateDirectory().Exists)
        {
            return content;
        }

        string title = "";
        const string titleTagToken = "@title ";

        if(content.StartsWith(titleTagToken))
        {
            int lineEnd = content.IndexOf(Environment.NewLine);
            title = content.Substring(titleTagToken.Length, lineEnd - titleTagToken.Length);
            content = content.Remove(0, lineEnd + Environment.NewLine.Length);
        }

        const string headerToken = "|header|";
        if(content.Contains(headerToken))
        {
            FileInfo headerFile = new FileInfo(
                _config.GetTemplateDirectory().FullName + Path.DirectorySeparatorChar + "header.htm");
            if(headerFile.Exists)
            {
                content = content.Replace(headerToken, headerFile.OpenText().ReadToEnd());
            }
        }

        const string footerToken = "|footer|";
        if(content.Contains(footerToken))
        {
            FileInfo footerFile = new FileInfo(
                _config.GetTemplateDirectory().FullName + Path.DirectorySeparatorChar + "footer.htm");
            if (footerFile.Exists)
            {
                content = content.Replace(footerToken, footerFile.OpenText().ReadToEnd());
            }
        }

        const string titleToken = "|title|";
        if(content.Contains(titleToken))
        {
            content = content.Replace(titleToken, title);
        }

        return content;
    }
}
