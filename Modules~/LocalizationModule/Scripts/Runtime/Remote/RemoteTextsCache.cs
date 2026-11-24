using System;
using System.IO;
using Modules.LocalizationModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.LocalizationModule.Remote
{
    /// <summary>
    /// The file the phone keeps the last good download in. A file and not LocalSave, because the
    /// module depends on nothing. It is written beside itself and then moved over, so a launch
    /// that dies mid-write leaves the previous cache rather than half of a new one.
    /// </summary>
    internal class RemoteTextsCache
    {
        private const string FILE_NAME = "flowioc-localization-remote.json";

        private readonly string _path;
        private readonly string _appVersion;

        public RemoteTextsCache(string path, string appVersion)
        {
            _path = path;
            _appVersion = appVersion ?? string.Empty;
        }

        public static string DefaultPath() => Path.Combine(Application.persistentDataPath, FILE_NAME);

        /// <summary>The cache, or null when there is none, it does not read, or another app version wrote it.</summary>
        public RemoteTextsCacheVO Read()
        {
            try
            {
                if (!File.Exists(_path))
                    return null;

                var cache = JsonUtility.FromJson<RemoteTextsCacheVO>(File.ReadAllText(_path));

                return cache != null && cache.AppVersion == _appVersion ? cache : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Write(RemoteTextsCacheVO cache)
        {
            cache.AppVersion = _appVersion;
            string temporary = _path + ".tmp";

            File.WriteAllText(temporary, JsonUtility.ToJson(cache));

            if (File.Exists(_path))
                File.Replace(temporary, _path, null);
            else
                File.Move(temporary, _path);
        }

        /// <summary>Records that the sheet was looked at, keeping what the cache holds.</summary>
        public void Touch(long checkedAtUtcTicks)
        {
            RemoteTextsCacheVO cache = Read() ?? new RemoteTextsCacheVO();
            cache.CheckedAtUtcTicks = checkedAtUtcTicks;
            Write(cache);
        }
    }
}
