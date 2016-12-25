using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuminaviCrawl.Models
{
    public class JsonIo
    {
        public static void SaveText(string filename, string text)
        {
            string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string outdir = Path.GetFullPath(exedir + "\\..\\..\\..");
            string fpath = Path.Combine(outdir, filename);
            File.WriteAllText(fpath, text);
        }

        public static void SaveJson(string filename, object obj)
        {
            string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string outdir = Path.GetFullPath(exedir + "\\..\\..\\..");
            string fpath = Path.Combine(outdir, filename);
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(fpath, json);
            Console.WriteLine(json);
        }
        public static Type LoadJson<Type>(string filename)
        {
            string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string outdir = Path.GetFullPath(exedir + "\\..\\..\\..");
            string fpath = Path.Combine(outdir, filename);
            string json = File.ReadAllText(fpath);
            return JsonConvert.DeserializeObject<Type>(json);
        }
    }
}
