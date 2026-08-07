using System.IO;

namespace AliceInCradleHack.utils.client
{
    public static class MainFolder
    {
        /// <summary>
        /// Reads the main folder path from the first line of C:\AliceInCradleHack\path.txt.
        /// </summary>
        public static string GetMainFolder()
        {
            string folderPath = File.ReadAllLines("C:\\AliceInCradleHack\\path.txt")[0];
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("Main folder not found: " + folderPath);
            }
            return folderPath;
        }
    }
}
