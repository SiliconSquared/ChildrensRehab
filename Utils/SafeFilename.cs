using System;

namespace System.IO
{
    public class SafeFilename
    {

        public static string GenerateUniqueFileName(string path, ref int count)
        {
            if (count == 0 || count == 1)
            {
                if (!File.Exists(path))
                    return path;
            }
            else
            {
                var candidatePath = string.Format(@"{0}\{1}_{2}{3}", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), count, Path.GetExtension(path));
                if (!File.Exists(candidatePath))
                    return candidatePath;
            }
            count++;
            return GenerateUniqueFileName(path, ref count);
        }



        public static string GetSafeFilename(string desiredNameWithoutExtension, string extension = null)
        {
            // Trim leading and trailing white space.
            var name = desiredNameWithoutExtension;
            name = name.Trim();

            // Replace invalid characters with underscores.
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            // Use "Untitled" instead of an empty name.
            if (name == null || name.Length == 0)
                name = "Untitled";

            if(extension != null)
                name += extension;
            return name;
        }
    }
}


