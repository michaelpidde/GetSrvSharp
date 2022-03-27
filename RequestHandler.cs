using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RequestHandler
{
    private string _rawRequest;
    private string _resource;
    private ResourceType _resourceType;
    private ResponseCode _responseCode;
    private Action<string> Log { get; }

    private enum ResourceType
    {
        HTML, CSS, PNG, JPG
    }

    private enum ResponseCode
    {
        OK = 200,
        NOT_FOUND = 404,
        SERVER_ERROR = 500
    }

    public RequestHandler(Action<string> log, string rawRequest)
    {
        Log = log;
        _rawRequest = rawRequest;
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
        return "";
    }
}
