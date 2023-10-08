



using System.Net.WebSockets;

class Request
{
    public enum Method
    {
        Connect,
        Delete,
        Get,
        Head,
        Options,
        Patch,
        Post,
        Put,
        Trace,
    }


    public Method method;
    public string url;
    public Dictionary<string, string> headers;
    public string contentType;
    public int contentLength;
    public string body;


    public Request()
    {
        method = default;
        url = "";
        headers = new Dictionary<string, string>();
        contentType = "";
        contentLength = 0;
        body = "";
    }


    public static Request Parse(string data)
    {
        var ret = new Request();

        var rows = data.Split("\r\n");

        var startline = rows[0];
        {
            var splits = startline.Split(' ');

            if(splits.Length >= 3)
            {
                var method = splits[0];
                var url = splits[1];
                var protocolVersion = splits[2];

                switch(method)
                {
                    case "GET": ret.method = Method.Get; break;
                    default: break;
                }

                ret.url = url;
            }
        }

        for(int i = 1; i < rows.Length; i++)
        {
            var row = rows[i];

            if(row.StartsWith("Content-Type"))
                ret.contentType = row.Substring(row.IndexOf(':')).Trim();

            if (row.StartsWith("Content-Length"))
            {
                int.TryParse(row.Substring(row.IndexOf(':')).Trim(), out ret.contentLength);
            }
        }

        ret.body = rows[rows.Length - 1];

        return ret;
    }
}