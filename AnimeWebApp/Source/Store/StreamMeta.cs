using System.Text;



struct StreamMeta
{
    public enum Type
    {
        TV,
        Movie,
    }

    public string enTitle;
    public string jpTitle;
    public string zhTitle;

    public Type type;
    public string releaseDate;
    public List<string> episodes;
    public string description;

    public StreamMeta()
    {
        type = default;
        releaseDate = default;
        episodes = new List<string>();
        description = default;
    }

    public static StreamMeta Parse(string path)
    {
        var ret = new StreamMeta();

        var lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.StartsWith("[Publish]"))
            {
                ++i;

                for (; i < lines.Length; i++)
                {
                    line = lines[i];

                    if (line.Length == 0)
                        break;

                    if (line.StartsWith("Type"))
                    {
                        var typeDescriptor = line.Substring(line.IndexOf('=') + 1);

                        switch (typeDescriptor)
                        {
                            case "TV": ret.type = Type.TV; break;
                            case "Movie": ret.type = Type.Movie; break;
                        }
                    }

                    if (line.StartsWith("Release"))
                    {
                        ret.releaseDate = line.Substring(line.IndexOf('=') + 1);
                    }
                }
            }

            if (line.StartsWith("[Title]"))
            {
                ++i;

                for (; i < lines.Length; i++)
                {
                    line = lines[i];

                    if (line.Length == 0)
                        break;

                    if (line.StartsWith("En"))
                    {
                        ret.enTitle = line.Substring(line.IndexOf('=') + 1);
                    }

                    if (line.StartsWith("Jp"))
                    {
                        ret.jpTitle = line.Substring(line.IndexOf('=') + 1);
                    }

                    if (line.StartsWith("Zh"))
                    {
                        ret.zhTitle = line.Substring(line.IndexOf('=') + 1);
                    }
                }
            }

            if (line.StartsWith("[Episode]"))
            {
                i += 2;

                for (; i < lines.Length; i++)
                {
                    line = lines[i];

                    if (line.Length == 0)
                        break;

                    var episode = line.Substring(line.IndexOf('=') + 1);
                    ret.episodes.Add(episode);
                }
            }

            if (line.StartsWith("[Descripton]"))
            {
                ++i;

                var descriptionSB = new StringBuilder();
                for (; i < lines.Length; i++)
                {
                    line = lines[i];

                    /*if (line.Length == 0)
                        break;*/

                    descriptionSB.AppendLine(line);
                }

                ret.description = descriptionSB.ToString();
            }
        }

        return ret;
    }
}