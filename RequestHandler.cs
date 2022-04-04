using System.Text;

public class RequestHandler {
    private string _rawRequest;
    private Config _config;
    private string _resource = "";
    private const string _dateFormat = "yyyy-MM-dd HH:mm";
    private ResourceType _resourceType;
    private ResponseCode _responseCode = ResponseCode.NOT_FOUND;
    private Action<string> Log { get; }
    private Dictionary<int, string> _errorTemplates = new Dictionary<int, string>
    {
        { 404, "<html><title>Not Found</title></head><body>Page Not Found</body></html>" },
        { 500, "<html><title>Server Error</title></head><body>Server Error</body></html>" }
    };

    public enum ResourceType {
        HTML, CSS, PNG, JPG
    }
    public static string ResourceTypeToString(ResourceType value) {
        switch(value) {
            case ResourceType.CSS:
                return "text/css";
            case ResourceType.PNG:
                return "image/png";
            case ResourceType.JPG:
                return "image/jpg";
            case ResourceType.HTML:
            default:
                return "text/html";
        }
    }

    public enum ResponseCode {
        OK = 200,
        NOT_FOUND = 404,
        SERVER_ERROR = 500
    }
    public static string ResponseCodeToString(ResponseCode value) {
        switch(value) {
            case ResponseCode.OK:
                return "OK";
            case ResponseCode.NOT_FOUND:
                return "Not Found";
            case ResponseCode.SERVER_ERROR:
                return "Server Error";
            default:
                return "Great Scott!";
        }
    }

    public RequestHandler(Action<string> log, string rawRequest, Config config) {
        Log = log;
        _rawRequest = rawRequest;
        _config = config;
        Process();
    }

    private void Process() {
        using var reader = new StringReader(_rawRequest);
        string? line = reader.ReadLine();

        if(line == null) {
            _responseCode = ResponseCode.SERVER_ERROR;
            Log("Empty request.");
            return;
        }

        Log($"{DateTime.Now.ToString(_dateFormat)} {line}");

        if(line[..3] != "GET") {
            _responseCode = ResponseCode.SERVER_ERROR;
            Log("Invalid request type.");
            return;
        }

        int start = line.IndexOf("/") + 1;
        int end = line.IndexOf(" ", start);
        _resource = line[start..end];

        start = ++end;
        string httpVersion = line.Substring(start, line.Length - end);
        if(httpVersion != "HTTP/1.1") {
            _responseCode = ResponseCode.SERVER_ERROR;
            Log("Invalid HTTP version.");
            return;
        }


        if(_resource.Length == 0) {
            _resourceType = ResourceType.HTML;
            return;
        }

        int period = _resource.LastIndexOf(".") + 1;
        switch(_resource.Substring(period, _resource.Length - period).ToLower()) {
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

    public string GetResponse() {
        // Default response code to not found; it will get updated later on as necessary
        _responseCode = ResponseCode.NOT_FOUND;
        string body = GetBody();
        return GetHeaders(body.Length) + body;
    }

    private string GetHeaders(int contentLength) {
        var sb = new StringBuilder();
        sb.AppendLine("HTTP/1.1 " + (int)_responseCode + " " + ResponseCodeToString(_responseCode));
        sb.AppendLine("Content-Length: " + contentLength.ToString());
        sb.AppendLine("Content-Type: " + ResourceTypeToString(_resourceType));
        sb.Append('\n');
        return sb.ToString();
    }

    /**
     * Try to get custom error page by convention, otherwise get default.
     * This assumes that the response code is set accordingly prior to being called
     * (It is already defaulted at the class level to a valid value).
     */
    private string GetErrorPage() {
        FileInfo errorFile = new(
            _config.GetErrorTemplateDirectory().FullName + Path.DirectorySeparatorChar + (int)_responseCode + ".htm");

        if(errorFile.Exists) {
            // TODO: Allow template engine to parse this
            return errorFile.OpenText().ReadToEnd();
        }

        string? template;
        if(_errorTemplates.TryGetValue((int)_responseCode, out template)) {
            return template;
        }

        return "";
    }

    public string GetBody() {
        if(_resource == "") {
            _resource = _config.DefaultPage;
        }

        FileInfo requestedFile = new(
            _config.GetContentDirectory().FullName + Path.DirectorySeparatorChar + _resource);

        if(!requestedFile.Exists) {
            Log("Request resource " + _resource + " was not found.");
            _responseCode = ResponseCode.NOT_FOUND;
            return GetErrorPage();
        }

        string content = requestedFile.OpenText().ReadToEnd();

        if(!_config.TemplateEngineEnabled) {
            _responseCode = ResponseCode.OK;
            return content;
        }

        // The base content file was loaded so we're going to count this as a 200
        // even if the template parsing fails. That should gracefully fail and return
        // the unparsed content if so.
        _responseCode = ResponseCode.OK;
        return ParseTemplate(content);
    }

    public string ParseTemplate(string content) {
        if(!_config.GetTemplateDirectory().Exists) {
            return content;
        }

        string title = "";
        const string titleTagToken = "@title ";

        if(content.StartsWith(titleTagToken)) {
            int lineEnd = content.IndexOf(Environment.NewLine);
            title = content.Substring(titleTagToken.Length, lineEnd - titleTagToken.Length);
            content = content.Remove(0, lineEnd + Environment.NewLine.Length);
        }

        const string headerToken = "|header|";
        if(content.Contains(headerToken)) {
            FileInfo headerFile = new FileInfo(
                _config.GetTemplateDirectory().FullName + Path.DirectorySeparatorChar + "header.htm");
            if(headerFile.Exists) {
                content = content.Replace(headerToken, headerFile.OpenText().ReadToEnd());
            }
        }

        const string footerToken = "|footer|";
        if(content.Contains(footerToken)) {
            FileInfo footerFile = new FileInfo(
                _config.GetTemplateDirectory().FullName + Path.DirectorySeparatorChar + "footer.htm");
            if(footerFile.Exists) {
                content = content.Replace(footerToken, footerFile.OpenText().ReadToEnd());
            }
        }

        const string titleToken = "|title|";
        if(content.Contains(titleToken)) {
            content = content.Replace(titleToken, title);
        }

        return content;
    }
}
