using Microsoft.Extensions.Configuration;
using ServiceStack;
using Spotify.Library;
using System.IO;
using System.Text.RegularExpressions;

namespace Spotify.Web.Services
{
    public class CustomFileSystemCache : ICustomCache
    {
        private string _directoryPath;
        public CustomFileSystemCache(IConfiguration config) => _directoryPath = config.Get<string>("CacheDirectory");


        private static readonly char[] _invalidChars = Path.GetInvalidFileNameChars();


        private string CacheFileName(string username, string key)
        {
            foreach (var invalidChar in _invalidChars)
            {
                key = key.Replace(invalidChar, '-');
            }

            return Path.Combine(_directoryPath, $"{username}.{key}.json");
        }





        public T Get<T>(string username) => Get<T>(username, typeof(T).Name);
        public T Get<T>(string username, string key) => File.ReadAllText(CacheFileName(username, key)).FromJson<T>();

        public bool HasKey<T>(string username) => HasKey(username, typeof(T).Name);
        public bool HasKey(string username, string key) => File.Exists(CacheFileName(username, key));

        public void Save<T>(string username, T dto) => Save(username, typeof(T).Name, dto);
        public void Save<T>(string username, string key, T dto) => File.WriteAllText(CacheFileName(username, key), dto.ToJson()); 
    }
}
