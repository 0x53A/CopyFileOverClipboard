using Nessos.FsPickler;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CopyFileOverClipboard
{
    [Serializable]
    public class FileRepresentation
    {
        public string Filename { get; set; }
        public byte[] Bytes { get; set; }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (Clipboard.ContainsFileDropList())
                FilesToText();
            else if (Clipboard.ContainsText())
                TextToFiles();
        }

        private static void TextToFiles()
        {
            var str = Clipboard.GetText();
            var bytes = Convert.FromBase64String(str);
            var pickler = FsPickler.CreateBinarySerializer();
            var fileReps = pickler.UnPickle<FileRepresentation[]>(bytes);

            var files = new StringCollection();

            var tmpDir = Path.GetTempFileName();
            File.Delete(tmpDir);
            Directory.CreateDirectory(tmpDir);

            foreach (var f in fileReps)
            {
                var fileName = Path.GetFileName(f.Filename);
                var fullPath = Path.Combine(tmpDir, fileName);
                File.WriteAllBytes(fullPath, f.Bytes);
                files.Add(fullPath);
            }
            Clipboard.SetFileDropList(files);
        }

        private static void FilesToText()
        {
            var files = Clipboard.GetFileDropList();
            if (files.Count == 0)
            {
                Console.Error.WriteLine("No Files found on Clipboard");
                Environment.Exit(1);
            }

            List<FileRepresentation> fileReps = new List<FileRepresentation>(files.Count);
            foreach (var f in files)
            {
                fileReps.Add(new FileRepresentation { Filename = f, Bytes = File.ReadAllBytes(f) });
            }

            var pickler = FsPickler.CreateBinarySerializer();
            var bytes = pickler.Pickle<FileRepresentation[]>(fileReps.ToArray());
            var str = Convert.ToBase64String(bytes);
            Clipboard.SetText(str);
        }
    }
}
