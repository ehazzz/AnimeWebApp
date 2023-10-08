


struct Settings
{
    public string ip;
    public string port;
    public string assetDirectory;
    public string animeDirectory;


    public static Settings Parse(string path)
    {
        Settings ret = new Settings();
        
        var lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if(line.StartsWith("[IP]"))
            {
                line = lines[++i];

                ret.ip = line;
            }

            if (line.StartsWith("[Port]"))
            {
                line = lines[++i];

                ret.port = line;
            }

            if (line.StartsWith("[AssetDirectory]"))
            {
                line = lines[++i];

                ret.assetDirectory = line;
            }

            if (line.StartsWith("[AnimeDirectory]"))
            {
                line = lines[++i];

                ret.animeDirectory = line;
            }
        }

        return ret;
    }
}