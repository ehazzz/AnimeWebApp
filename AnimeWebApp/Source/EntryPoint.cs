using System.Net;
using System.Net.Sockets;
using System.Text;



#region Helper Functions
var GetBangumiCover = (string path) =>
{
    var filters = new string[] { ".bmp", ".jpg", ".png", ".webp" };

    foreach (var file in Directory.GetFiles(path))
        foreach (var filter in filters)
            if (file.EndsWith(filter))
                return file;

    return null;
};

var GetDirectoryName = (string path) =>
{
    int index = -1;

    index = path.LastIndexOf('\\');
    index = Math.Max(index, path.LastIndexOf('/'));

    return path.Substring(index + 1);
};

var GetFileName = (string path) =>
{
    return GetDirectoryName(path);
};

var GetFileExtension = (string path) =>
{
    var filename = GetFileName(path);

    return filename.Substring(filename.IndexOf('.') + 1);
};
#endregion


var settings = Settings.Parse("Settings.txt");


const string MainPageName = @"index.html";
const string DetailsPageName = @"details.html";
const string SearchPageName = @"search.html";

string AssetRoot = settings.assetDirectory;
string AnimeRoot = settings.animeDirectory;

var MainPagePath = Path.Combine(AssetRoot, MainPageName);
var DetailsPagePath = Path.Combine(AssetRoot, DetailsPageName);
var SearchPagePath = Path.Combine(AssetRoot, SearchPageName);


var GetRequest = (Socket client) =>
{
    if (client.Poll(1000, SelectMode.SelectRead))
    {
        var receiveBuffer = new byte[1024];
        var receiveLength = client.Receive(receiveBuffer);

        return Request.Parse(Encoding.UTF8.GetString(receiveBuffer, 0, receiveLength));
    }

    return null;
};

var SendResponse = (Socket client, string data) =>
{
    var response = new Response();
    response.status.code = 200;
    response.status.description = "OK";
    response.contentType = "text/html;charset=UTF-8";
    response.body = data;

    var binary = Encoding.UTF8.GetBytes(response.Build());
    client.Send(binary);
};

var MainPageHandler = (Socket client) =>
{
    var bangumiCovers = new List<string>();
    var bangumiTitles = new List<string>();

    var navigationURLs = new List<string>();

    foreach(var bangumi in Directory.GetDirectories(AnimeRoot))
    {
        bangumiCovers.Add(GetBangumiCover(bangumi));

        var streamMeta = StreamMeta.Parse(Path.Combine(bangumi, "Stream.meta"));
        bangumiTitles.Add(streamMeta.zhTitle);

        navigationURLs.Add(bangumi);
    }

    const string SelectPlaceHolder = "SelectPlaceHolder";
    const string TitlePlaceHolder = "TitlePlaceHolder";
    const string ImageURLPlaceHolder = "ImageURLPlaceHolder";
    const string bangumeItemTemplate = $"<a href=\"?Selection={SelectPlaceHolder}\" target=\"_blank\"><div class=\"BangumiItem\"><div class=\"BangumiTitle\">{TitlePlaceHolder}</div><img src=\"{ImageURLPlaceHolder}\"/></div></a>";
    
    var bodySB = new StringBuilder();
    for(int i = 0; i < bangumiCovers.Count; i++)
    {
        var cover = bangumiCovers[i];
        var imageData = File.ReadAllBytes(cover);
        var base64 = Convert.ToBase64String(imageData);

        var template = bangumeItemTemplate.
        Replace(SelectPlaceHolder, navigationURLs[i]).
        Replace(TitlePlaceHolder, bangumiTitles[i]).
        Replace(ImageURLPlaceHolder, $"data:{MIME.GetFromFileExtension(GetFileExtension(cover))};base64,{base64}");

        bodySB.Append(template);
    }

    var htmlSB = new StringBuilder();
    foreach (var line in File.ReadAllLines(MainPagePath))
    {
        if (!line.StartsWith("[Body]"))
            htmlSB.Append(line);
        else
            htmlSB.Append(bodySB.ToString());
    }

    SendResponse(client, htmlSB.ToString());
};

var DetailsPageHandler = (Socket client, Request request) =>
{
    var url = request.url;
    var selectionParam = WebUtility.UrlDecode(url.Substring(url.LastIndexOf('=') + 1));

    var imageURL = "";
    {
        var cover = GetBangumiCover(selectionParam);

        var imageData = File.ReadAllBytes(cover);
        var base64 = Convert.ToBase64String(imageData);

        var filename = GetFileName(cover);
        var extension = GetFileExtension(filename);
        imageURL = $"data:{MIME.GetFromFileExtension(extension)};base64,{base64}";
    }

    var streamMeta = StreamMeta.Parse(Path.Combine(selectionParam, "Stream.meta"));

    var bodySB = new StringBuilder();
    {
        bodySB.Append($"<div class=\"BangumiCover\"><img src=\"{imageURL}\"/></div>");

        bodySB.Append($"<p>释放日期：{streamMeta.releaseDate}</p>");
        bodySB.Append($"<div class=\"Description\"><h1>简介</h1><p>{streamMeta.description}</p></div>");

        if(streamMeta.episodes.Count > 1)
        {
            bodySB.Append("<div class=\"Episodes\">");
            bodySB.Append($"<h1>总话数: {streamMeta.episodes.Count}</h1>");
            for (int i = 0; i < streamMeta.episodes.Count; i++)
            {
                bodySB.Append($"<p>第{i + 1}话：{streamMeta.episodes[i]}</p>");
            }
            bodySB.Append("</div>");
        }
    }

    var htmlSB = new StringBuilder();
    foreach (var line in File.ReadAllLines(DetailsPagePath))
    {
        if (!line.StartsWith("[Body]"))
            htmlSB.Append(line);
        else
            htmlSB.Append(bodySB.ToString());
    }

    SendResponse(client, htmlSB.ToString());
};

var SearchPageHandler = (Socket client, Request request) =>
{
    var url = request.url;
    var searchParam = WebUtility.UrlDecode(url.Substring(url.LastIndexOf('=') + 1));

    var searches = Directory.GetDirectories(AnimeRoot, $"*{searchParam}*");
    {
        var bangumiCovers = new List<string>();
        var bangumiTitles = new List<string>();

        var navigationURLs = new List<string>();

        foreach (var search in searches)
        {
            bangumiCovers.Add(GetBangumiCover(search));

            var streamMeta = StreamMeta.Parse(Path.Combine(search, "Stream.meta"));
            bangumiTitles.Add(streamMeta.zhTitle);

            navigationURLs.Add(search);
        }

        const string SelectPlaceHolder = "SelectPlaceHolder";
        const string TitlePlaceHolder = "TitlePlaceHolder";
        const string ImageURLPlaceHolder = "ImageURLPlaceHolder";
        const string bangumeItemTemplate = $"<a href=\"?Selection={SelectPlaceHolder}\" target=\"_blank\"><div class=\"BangumiItem\"><div class=\"BangumiTitle\">{TitlePlaceHolder}</div><img src=\"{ImageURLPlaceHolder}\"/></div></a>";

        var bodySB = new StringBuilder();
        for (int i = 0; i < bangumiCovers.Count; i++)
        {
            var cover = bangumiCovers[i];
            var imageData = File.ReadAllBytes(cover);
            var base64 = Convert.ToBase64String(imageData);

            var template = bangumeItemTemplate.
            Replace(SelectPlaceHolder, navigationURLs[i]).
            Replace(TitlePlaceHolder, bangumiTitles[i]).
            Replace(ImageURLPlaceHolder, $"data:{MIME.GetFromFileExtension(GetFileExtension(cover))};base64,{base64}");

            bodySB.Append(template);
        }

        var htmlSB = new StringBuilder();
        foreach (var line in File.ReadAllLines(SearchPagePath))
        {
            if (!line.StartsWith("[Body]"))
                htmlSB.Append(line);
            else
            {
                htmlSB.Append(bodySB.ToString());
            }
        }

        SendResponse(client, htmlSB.ToString());
    }
};


var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(
    new IPEndPoint(IPAddress.Parse(settings.ip),
    int.Parse(settings.port))
);
server.Listen(1024);

while (true)
{
    var client = server.Accept();

    Request request = GetRequest(client);
    if(request != null && client.Poll(1000, SelectMode.SelectWrite))
        switch (request.method)
        {
            case Request.Method.Get:
                {
                    if (request.url == "/")
                        MainPageHandler(client);

                    if (request.url.StartsWith("/?Selection"))
                        DetailsPageHandler(client, request);

                    if (request.url.StartsWith("/?Search"))
                        SearchPageHandler(client, request);
                }
                break;
        }

    client.Shutdown(SocketShutdown.Both);
    client.Close();
}