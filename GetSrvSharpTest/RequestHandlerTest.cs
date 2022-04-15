using Xunit;
using static RequestHandler;

namespace GetSrvSharpTest;
public class RequestHandlerTest {
  [Fact]
  public void TestResponseCodeValues() {
    Assert.Equal(200, (int)ResponseCode.OK);
    Assert.Equal(404, (int)ResponseCode.NOT_FOUND);
    Assert.Equal(500, (int)ResponseCode.SERVER_ERROR);
  }

  [Fact]
  public void TestGetResourceTypeFromExtension() {
    Assert.Equal(ResourceType.CSS, GetResourceTypeFromExtension("css"));
    Assert.Equal(ResourceType.CSS, GetResourceTypeFromExtension("CSS"));
    Assert.Equal(ResourceType.PNG, GetResourceTypeFromExtension("png"));
    Assert.Equal(ResourceType.PNG, GetResourceTypeFromExtension("PNG"));
    Assert.Equal(ResourceType.JPG, GetResourceTypeFromExtension("jpg"));
    Assert.Equal(ResourceType.JPG, GetResourceTypeFromExtension("JPG"));
    Assert.Equal(ResourceType.JPG, GetResourceTypeFromExtension("jpeg"));
    Assert.Equal(ResourceType.JPG, GetResourceTypeFromExtension("JPEG"));
    Assert.Equal(ResourceType.HTML, GetResourceTypeFromExtension("htm"));
    Assert.Equal(ResourceType.HTML, GetResourceTypeFromExtension("HTM"));
    Assert.Equal(ResourceType.HTML, GetResourceTypeFromExtension("html"));
    Assert.Equal(ResourceType.HTML, GetResourceTypeFromExtension("HTML"));
    Assert.Equal(ResourceType.HTML, GetResourceTypeFromExtension("default"));
  }

  [Fact]
  public void TestResourceTypeToString() {
    Assert.Same("text/css", ResourceTypeToString(ResourceType.CSS));
    Assert.Same("image/png", ResourceTypeToString(ResourceType.PNG));
    Assert.Same("image/jpg", ResourceTypeToString(ResourceType.JPG));
    Assert.Same("text/html", ResourceTypeToString(ResourceType.HTML));
    Assert.Same("text/html", ResourceTypeToString((ResourceType)(-1)));
  }

  [Fact]
  public void TestResponseCodeToString() {
    Assert.Same("OK", ResponseCodeToString(ResponseCode.OK));
    Assert.Same("Not Found", ResponseCodeToString(ResponseCode.NOT_FOUND));
    Assert.Same("Server Error", ResponseCodeToString(ResponseCode.SERVER_ERROR));
    Assert.Same("Great Scott!", ResponseCodeToString((ResponseCode)(-1)));

  }

  [Fact]
  public void TestGetHeaders() {
    string headers = GetHeaders(
        100,
        ResponseCode.OK,
        ResourceType.HTML
    );
    const string nl = "\r\n";

    Assert.StartsWith($"HTTP/1.1 200 OK{nl}", headers);
    Assert.Contains($"{nl}Content-Length: 100{nl}", headers);
    Assert.Contains($"{nl}Content-Type: text/html{nl}", headers);
    Assert.Contains($"{nl}Cache-Control: max-age={OneYear}{nl}", headers);
    Assert.Equal("\n\n", headers.Substring(headers.Length - 2, 2));
  }
}