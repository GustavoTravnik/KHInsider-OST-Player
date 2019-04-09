using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WSOST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OSTListController : ControllerBase
    {
        HttpClient WebClientVar { get; set; } = new HttpClient();
        static List<String> OSTList { get; set; } = new List<string>();
        static List<Source> OSTSources { get; set; } = new List<Source>();
        static List<Source> CachedOSTSources { get; set; } = new List<Source>();
        public String OST_LIST_DUMP = Environment.CurrentDirectory;

        public static WebClient GetWebClient()
        {
            WebClient wc = new WebClient();
            
            return wc;
        }

        public void FillListBySimbol(Object simbol)
        {
            string source = GetWebClient().DownloadString("https://downloads.khinsider.com/game-soundtracks/browse/" + simbol);
            source = Regex.Split(source, "<p align=\"left\">")[1];
            String[] sourceParts = Regex.Split(source, "<a href=\"");
            sourceParts = sourceParts.Where(k => k.StartsWith("/game-soundtracks") && k.Contains("album")).ToArray();
            for (int i = 0; i < sourceParts.Length; i++)
            {
                sourceParts[i] = "https://downloads.khinsider.com" + sourceParts[i].Split('"')[0];
            }
            foreach (String s in sourceParts)
            {
                string name = s.Split('/')[s.Split('/').Length - 1].Replace('-', ' ');
                OSTList.Add(name + "¨" + s);
                OSTSources.Add(new Source(s));
            }
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            try
            {
                if (System.IO.File.Exists(Path.Combine(OST_LIST_DUMP, "_DUMP_.dmp")))
                {
                    OSTList = System.IO.File.ReadAllLines(Path.Combine(OST_LIST_DUMP, "_DUMP_.dmp")).ToList();
                    OSTList.Select(k => k.Split('¨')[1]).ToList().ForEach(k =>
                    {
                        OSTSources.Add(new Source(k));
                    });
                }
                if (!OSTList.Any())
                {
                    FillListBySimbol(HttpUtility.UrlEncode("#"));
                    for (int i = 65; i <= 90; i++)
                    {
                        char letter = Convert.ToChar(i);
                        FillListBySimbol(letter.ToString());
                    }
                    System.IO.File.WriteAllLines(Path.Combine(OST_LIST_DUMP, "_DUMP_.dmp"), OSTList.ToArray());
                }
            }
            catch(Exception ex)
            {
                System.IO.File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "Erro.txt"), ex.Message + Environment.NewLine);
            }
            return OSTList.Select(k => k.Split('¨')[0]).ToList();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<IDictionary<string, Dictionary<string, string>>> Get(string id)
        {
            try
            {
                Source currentSource = OSTSources.Find(k => k.Nome.Equals(id));
                Source searchedSource = CachedOSTSources.Find(x => x.Nome.Equals(id));
                if (searchedSource != null)
                {
                    currentSource = searchedSource;
                }
                else
                {
                    currentSource.LoadMusics(GetWebClient());
                    CachedOSTSources.Add(currentSource);
                }
                Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
                result.Add(currentSource.Nome, currentSource.Tracks);
                return result;
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "Erro.txt"), ex.Message + Environment.NewLine);
                return new Dictionary<string, Dictionary<string, string>>();
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
