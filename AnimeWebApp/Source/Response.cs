using System.Text;



class Response
{
    public struct Status
    {
        public int code;
        public string description;
    }


    public Status status;
    public Dictionary<string, string> headers;
    public string contentType;
    public int contentLength;
    public string body;


    public Response()
    {
        status = new Status();
        headers = new Dictionary<string, string>();
        contentType = "";
        contentLength = 0;
        body = "";
    }


    public string Build()
    {
        var sb = new StringBuilder();
        // startline
        //sb.Append($"{ProtocolVersion.DefaultValue} {status.code} {status.description}\r\n");
        sb.Append($"{ProtocolVersion.DefaultValue} {status.code} {GetStatusCodeDescription(status.code)}\r\n");
        // headers
        foreach (var header in headers) sb.Append($"{header.Key}: {header.Value}\r\n");
        // content
        sb.Append($"Content-Type: {contentType}\r\n");
        if (contentLength > 0) sb.Append($"Content-Length: {contentLength}\r\n");
        // empty line
        sb.Append("\r\n");
        // body
        if (body.Length > 0) sb.Append(body);

        return sb.ToString();
    }

    private string GetStatusCodeDescription(int statusCode)
    {
        switch(statusCode)
        {
            case 200: return "OK";
            case 201: return "Created";
            case 301: return "Move Permanently";
            case 302: return "Move Temporarily";
            case 304: return "Not Modified";
            case 400: return "Bad Request";
            case 401: return "Unauthorized";
            case 403: return "Forbidden";
            case 404: return "Not Found";
            case 405: return "Method Not Allowed";
            case 500: return "Internal Server Error";
            case 501: return "Not Implemented";
            case 503: return "Service Unavailable";
            default: return "Unknown";
        }
    }
}