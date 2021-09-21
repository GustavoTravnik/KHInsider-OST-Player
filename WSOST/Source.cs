using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WSOST
{
    public class Source
    {
        private string url = String.Empty;
        public string Nome { get; set; } = string.Empty;
        public Dictionary<string, string> Tracks { get; set; } = new Dictionary<string, string>();
        public String OST_LIST_DUMP = Environment.CurrentDirectory;

        public Source(String url)
        {
            this.url = url;
            Nome = url.Split('/')[url.Split('/').Length - 1].Replace('-', ' ');
            OST_LIST_DUMP = Path.Combine(OST_LIST_DUMP, Nome + ".dmp");
        }

        public void LoadMusics(WebClient wc)
        {
            if (!Tracks.Any())
            {
                if (File.Exists(OST_LIST_DUMP))
                {
                    foreach (string s in File.ReadAllLines(OST_LIST_DUMP))
                    {
                        Tracks.Add(s.Split('/')[s.Split('/').Length - 1].Replace('-', ' ').Replace("%20", " "), s);
                    }
                }
                else
                {
                    String source = wc.DownloadString(url);
                    String[] sourceList = Regex.Split(source, "<a href=\"");
                    sourceList = sourceList.Where(k => k.Contains(".mp3") && !k.Contains("forums/member") && !k.Contains("cp/add_album")).ToArray();
                    for (int i = 0; i < sourceList.Length; i++)
                    {
                        sourceList[i] = sourceList[i].Split('"')[0];
                    }

                    sourceList = (from d in sourceList select d).Distinct().ToArray();

                    for (int i = 0; i < sourceList.Length; i++)
                    {
                        if (!sourceList[i].StartsWith("http"))
                        {
                            sourceList[i] = "https://downloads.khinsider.com" + sourceList[i];
                        }
                        sourceList[i] = WebUtility.UrlDecode(ResolveFileName(sourceList[i], wc));
                    }

                    foreach (string s in sourceList)
                    {
                        Tracks.Add(WebUtility.UrlDecode(s.Split('/')[s.Split('/').Length - 1].Replace('-', ' ')), s);
                    }

                    File.WriteAllLines(OST_LIST_DUMP, sourceList);
                }
            }
        }

        public String ResolveFileName(String url, WebClient wc)
        {
            String source = wc.DownloadString(url);
            String[] sourceList = Regex.Split(source, "src=\"");
            for (int i = 0; i < sourceList.Length; i++)
            {
                sourceList[i] = sourceList[i].Split('"')[0];
            }
            sourceList = sourceList.Where(k => k.Contains(".mp3") && (k.Contains("/ost/") || k.Contains("soundtracks"))).ToArray();


            return sourceList[0];
        }
    }
}
