using Xunit;

namespace GetSrvSharpTest;
public class RequestHandlerTest {
    [Fact]
    public void TestResponseCodeValues() {
        Assert.Equal(200, (int)RequestHandler.ResponseCode.OK);
        Assert.Equal(404, (int)RequestHandler.ResponseCode.NOT_FOUND);
        Assert.Equal(500, (int)RequestHandler.ResponseCode.SERVER_ERROR);
    }

    [Fact]
    public void TestGetResourceTypeFromExtension() {
        Assert.Equal(RequestHandler.ResourceType.CSS, RequestHandler.GetResourceTypeFromExtension("css"));
        Assert.Equal(RequestHandler.ResourceType.CSS, RequestHandler.GetResourceTypeFromExtension("CSS"));
        Assert.Equal(RequestHandler.ResourceType.PNG, RequestHandler.GetResourceTypeFromExtension("png"));
        Assert.Equal(RequestHandler.ResourceType.PNG, RequestHandler.GetResourceTypeFromExtension("PNG"));
        Assert.Equal(RequestHandler.ResourceType.JPG, RequestHandler.GetResourceTypeFromExtension("jpg"));
        Assert.Equal(RequestHandler.ResourceType.JPG, RequestHandler.GetResourceTypeFromExtension("JPG"));
        Assert.Equal(RequestHandler.ResourceType.JPG, RequestHandler.GetResourceTypeFromExtension("jpeg"));
        Assert.Equal(RequestHandler.ResourceType.JPG, RequestHandler.GetResourceTypeFromExtension("JPEG"));
        Assert.Equal(RequestHandler.ResourceType.HTML, RequestHandler.GetResourceTypeFromExtension("htm"));
        Assert.Equal(RequestHandler.ResourceType.HTML, RequestHandler.GetResourceTypeFromExtension("HTM"));
        Assert.Equal(RequestHandler.ResourceType.HTML, RequestHandler.GetResourceTypeFromExtension("html"));
        Assert.Equal(RequestHandler.ResourceType.HTML, RequestHandler.GetResourceTypeFromExtension("HTML"));
        Assert.Equal(RequestHandler.ResourceType.HTML, RequestHandler.GetResourceTypeFromExtension("default"));
    }

    [Fact]
    public void TestResourceTypeToString() {
        Assert.Same("text/css", RequestHandler.ResourceTypeToString(RequestHandler.ResourceType.CSS));
        Assert.Same("image/png", RequestHandler.ResourceTypeToString(RequestHandler.ResourceType.PNG));
        Assert.Same("image/jpg", RequestHandler.ResourceTypeToString(RequestHandler.ResourceType.JPG));
        Assert.Same("text/html", RequestHandler.ResourceTypeToString(RequestHandler.ResourceType.HTML));
        Assert.Same("text/html", RequestHandler.ResourceTypeToString((RequestHandler.ResourceType)(-1)));
    }

    [Fact]
    public void TestResponseCodeToString() {
        Assert.Same("OK", RequestHandler.ResponseCodeToString(RequestHandler.ResponseCode.OK));
        Assert.Same("Not Found", RequestHandler.ResponseCodeToString(RequestHandler.ResponseCode.NOT_FOUND));
        Assert.Same("Server Error", RequestHandler.ResponseCodeToString(RequestHandler.ResponseCode.SERVER_ERROR));
        Assert.Same("Great Scott!", RequestHandler.ResponseCodeToString((RequestHandler.ResponseCode)(-1)));
        
    }

    [Fact]
    public void TestGetHeaders() {
        string headers = RequestHandler.GetHeaders(
            100,
            RequestHandler.ResponseCode.OK,
            RequestHandler.ResourceType.HTML
        );
        const string nl = "\r\n";

        Assert.StartsWith($"HTTP/1.1 200 OK{nl}", headers);
        Assert.Contains($"{nl}Content-Length: 100{nl}", headers);
        Assert.Contains($"{nl}Content-Type: text/html{nl}", headers);
        Assert.Contains($"{nl}Cache-Control: max-age={RequestHandler.OneYear}{nl}", headers);
        Assert.Equal("\n\n", headers.Substring(headers.Length - 2, 2));
    }
}