using Microsoft.Extensions.Configuration;
using ServiceStack;
using Spotify.Library;
using System.IO;

namespace Spotify.Web.Services
{
    public class CustomFileSystemCache : ICustomCache
    {
        public CustomFileSystemCache(IConfiguration config) => _directoryPath = config.Get<string>("CacheDirectory");

        private string CacheFileName(string username, string key) => Path.Combine(_directoryPath, $"{username}.{key}.json");
        private string _directoryPath;


        public T Get<T>(string username) => Get<T>(username, typeof(T).Name);
        public T Get<T>(string username, string key) => File.ReadAllText(CacheFileName(username, key)).FromJson<T>();

        public bool HasKey<T>(string username) => HasKey(username, typeof(T).Name);
        public bool HasKey(string username, string key) => File.Exists(CacheFileName(username, key));

        public void Save<T>(string username, T dto) => Save(username, typeof(T).Name, dto);
        public void Save<T>(string username, string key, T dto) => File.WriteAllText(CacheFileName(username, key), Helpers.PrettyPrintJson(dto.ToJson())); 
    }
}
