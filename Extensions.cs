public static class ResourceTypeExtensions
{
    public static string ToHeaderString(this RequestHandler.ResourceType type)
    {
        switch(type)
        {
            case RequestHandler.ResourceType.CSS:
                return "text/css";
            case RequestHandler.ResourceType.PNG:
                return "image/png";
            case RequestHandler.ResourceType.JPG:
                return "image/jpg";
            case RequestHandler.ResourceType.HTML:
            default:
                return "text/html";
        }
    }
}

public static class ResponseCodeExtensions
{
    public static string ToHeaderString(this RequestHandler.ResponseCode code)
    {
        switch(code)
        {
            case RequestHandler.ResponseCode.OK:
                return "OK";
            case RequestHandler.ResponseCode.NOT_FOUND:
                return "Not Found";
            case RequestHandler.ResponseCode.SERVER_ERROR:
                return "Server Error";
            default:
                return "Great Scott!";
        }
    }
}