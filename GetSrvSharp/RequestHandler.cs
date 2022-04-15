using System.Text;

public class RequestHandler {
  public static readonly int OneYear = 1 * 60 * 60 * 24 * 365;

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
    _resourceType = GetResourceTypeFromExtension(
      _resource.Substring(period, _resource.Length - period)
    );
  }

  public static ResourceType GetResourceTypeFromExtension(string extension) {
    switch(extension.ToLower()) {
      case "css":
        return ResourceType.CSS;
      case "png":
        return ResourceType.PNG;
      case "jpg":
      case "jpeg":
        return ResourceType.JPG;
      case "html":
      case "htm":
      default:
        return ResourceType.HTML;
    }
  }

  public string GetResponse() {
    // Default response code to not found; it will get updated later on as necessary
    _responseCode = ResponseCode.NOT_FOUND;
    string body = GetBody();
    return GetHeaders(body.Length, _responseCode, _resourceType) + body;
  }

  public static string GetHeaders(
    int contentLength,
    ResponseCode responseCode,
    ResourceType resourceType
  ) {
    var sb = new StringBuilder();
    sb.AppendLine($"HTTP/1.1 {(int)responseCode} {ResponseCodeToString(responseCode)}");
    sb.AppendLine($"Content-Length: {contentLength.ToString()}");
    sb.AppendLine($"Content-Type: {ResourceTypeToString(resourceType)}");
    sb.AppendLine($"Cache-Control: max-age={OneYear}");
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
      _config.GetErrorTemplateDirectory().FullName +
      Path.DirectorySeparatorChar +
      (int)_responseCode +
      ".htm"
    );

    if(errorFile.Exists) {
      // TODO: Allow template engine to parse this
      using var reader = errorFile.OpenText();
      return reader.ReadToEnd();
    }

    if(_errorTemplates.TryGetValue((int)_responseCode, out string? template)) {
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

    using var reader = requestedFile.OpenText();
    string content = reader.ReadToEnd();

    if(!_config.TemplateEngineEnabled || !_config.GetTemplateDirectory().Exists) {
      _responseCode = ResponseCode.OK;
      return content;
    }

    // The base content file was loaded so we're going to count this as a 200
    // even if the template parsing fails. That should gracefully fail and return
    // the unparsed content if so.
    _responseCode = ResponseCode.OK;
    var parser = new TemplateParser();
    return parser.Parse(_config.GetTemplateDirectory().FullName, content);
  }
}