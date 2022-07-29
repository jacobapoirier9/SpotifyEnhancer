using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Spotify.Library;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Spotify.Web.Services
{
    public class CustomFileSystemCache : ICustomCache
    {
        private readonly string _directoryPath;
        private readonly TimeSpan _expiresAfter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomFileSystemCache(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _directoryPath = config.Get<string>("Cache:Directory");
            _expiresAfter = TimeSpan.Parse(config.Get<string>("Cache:ExpiresAfter"));
            _httpContextAccessor = httpContextAccessor;
        }


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

        public bool IsExpired<T>(string username) => IsExpired(username, typeof(T).Name);
        public bool IsExpired(string username, string key)
        {
            if (HasKey(username, key))
            {
                var lastWrite = File.GetLastWriteTime(CacheFileName(username, key));
                var expires = lastWrite.Add(_expiresAfter);
                return DateTime.Now > expires;
            }
            else
                return true;
        }
    }
}
