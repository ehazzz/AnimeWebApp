#region Helper Functions
using System.IO;

var GetBangumiCover = (string path) =>
{
    var filters = new string[] { "*.bmp", "*.jpg", "*.png", "*.webp" };

    foreach (var file in Directory.GetFiles(path))
        foreach (var filter in filters)
            if (file.EndsWith(file))
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



var source = @"D:\Achive\Anime";
var destination = @"C:\Users\oneha\source\repos\AnimeWebApp\AnimeWebApp\AnimeSample\";


foreach(var anime in Directory.GetDirectories(source))
{
    var directory = GetDirectoryName(anime);
    var toAnime = Path.Combine(destination, directory);

    var cover = Directory.GetFiles(anime, "Cover.*")[0];
    var stream = Directory.GetFiles(anime, "Stream.meta")[0];

    
    if(!Directory.Exists(toAnime))Directory.CreateDirectory(toAnime);

    var toCover = Path.Combine(toAnime, GetFileName(cover));
    var toStream = Path.Combine(toAnime, GetFileName(stream));

    File.Copy(cover, toCover);
    File.Copy(stream, toStream);
}