using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Modules.LocalSaveModule.Services
{
    /// <summary>
    /// The save file on disk, under the persistent data path. It holds text and nothing else -
    /// what the text means is the service's business - and it writes the way a save has to be
    /// written: to a temporary file first, moved over the old one only when it is whole, so a
    /// player killed mid-write is left with the previous save rather than half of a new one.
    ///
    /// With a password the bytes go through <see cref="LocalSaveCipher"/>. Reading decides by
    /// the file, not by the password: a plain file is read plain whatever the password says,
    /// which is what lets a game turn encryption on later and keep every save it already has -
    /// the next write is the encrypted one.
    /// </summary>
    internal class LocalSaveFile
    {
        private const string TEMPORARY_SUFFIX = ".tmp";

        private readonly LocalSaveCipher _cipher = new LocalSaveCipher();

        internal LocalSaveFile(string fileName)
        {
            Path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        }

        internal string Path { get; }

        /// <summary>
        /// The text the file holds, or null when there is no file - which is what a first run
        /// looks like, and not a failure. An encrypted file read with no password, or the wrong
        /// one, throws: the caller decides what an unreadable save means.
        /// </summary>
        internal string ReadText(string password)
        {
            string source = File.Exists(Path) ? Path : RecoverablePath();

            if (source == null)
                return null;

            byte[] bytes = File.ReadAllBytes(source);

            if (!_cipher.IsEncrypted(bytes))
                return Encoding.UTF8.GetString(bytes);

            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("the save file is encrypted and no password is set");

            return _cipher.Decrypt(bytes, password);
        }

        internal void WriteText(string text, string password)
        {
            byte[] bytes = string.IsNullOrEmpty(password)
                ? Encoding.UTF8.GetBytes(text)
                : _cipher.Encrypt(text, password);

            string temporary = Path + TEMPORARY_SUFFIX;

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            File.WriteAllBytes(temporary, bytes);

            // One rename, never a delete followed by a move: a player killed between those two
            // steps - a swipe from the recents half a second after leaving the game is enough - was
            // left with no save file at all, and the next start was a fresh install.
            if (File.Exists(Path))
                File.Replace(temporary, Path, null);
            else
                File.Move(temporary, Path);
        }

        /// <summary>
        /// The temporary file, when it is all that is left. A build that deleted the save file before
        /// moving the temporary one over it did so only once the temporary one was whole, so what
        /// such a kill left behind is the newest save. An empty one is a write that never began, and
        /// a first write killed halfway fails to parse and starts empty, as no file at all would.
        /// </summary>
        private string RecoverablePath()
        {
            string temporary = Path + TEMPORARY_SUFFIX;

            if (!File.Exists(temporary) || new FileInfo(temporary).Length == 0)
                return null;

            return temporary;
        }
    }
}
