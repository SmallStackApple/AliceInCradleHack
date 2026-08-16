using System.IO;

namespace AliceInCradleHack.utils.client
{
    public static class MainFolder
    {
        private const string PathFile = "C:\\AliceInCradleHack\\path.txt";
        private static readonly object _lock = new();
        private static string _mainFolder;

        /// <summary>
        /// Reads the main folder path from the first line of C:\AliceInCradleHack\path.txt.
        /// </summary>
        public static string GetMainFolder()
        {
            lock (_lock)
            {
                if (_mainFolder != null) return _mainFolder;
                if (!File.Exists(PathFile))
                    throw new FileNotFoundException("Main folder path file not found.", PathFile);

                var lines = File.ReadAllLines(PathFile);
                if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0]))
                    throw new InvalidDataException($"Main folder path file is empty: {PathFile}");

                string folderPath = lines[0].Trim();
                if (!Directory.Exists(folderPath))
                    throw new DirectoryNotFoundException($"Main folder not found: {folderPath}");

                _mainFolder = folderPath;
                return _mainFolder;
            }
        }
    }
}
