using System.IO;
using System.Text;

namespace AliceInCradleHack.Utils.Client
{
    public static class MainFolder
    {
        public static string GetMainFolder()
        {
            //get the path of the main folder, which is in C:\AliceInCradleHack\path.txt, first line.
            string folderPath = File.ReadAllLines("C:\\AliceInCradleHack\\path.txt")[0];
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("Main folder not found: " + folderPath);
            }
            return folderPath;
        }
    }
}
