using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Additionals
{
    public static class ACIIDSHelper
    {
        public static void CopyFiles()
        {
            var directoryPath = @"D:\Data Scientific\ACIIDS\dane_ober\POP\";
            var files = Directory.GetFiles(directoryPath).ToList();
            var csvFiles = files.Where(x => x.EndsWith(".csv"));
            var destFolder = @"D:\Data Scientific\ACIIDS\Sources\POP\";

            foreach (var filePath in csvFiles)
            {
                var fileName = Path.GetFileName(filePath);

                var dbsPrefix = 0;
                var bmtPrefix = 0;

                if (!fileName.Contains("DBS") && !fileName.Contains("BMT"))
                {
                    var onCount = fileName.Select((c, i) => fileName.Substring(i)).Count(sub => sub.StartsWith("ON"));
                    if (onCount == 2)
                    {
                        dbsPrefix = 1;
                        bmtPrefix = 1;
                    }

                }
                else
                {
                    if (fileName.Contains("DBS ON"))
                    {
                        dbsPrefix = 1;
                    }
                    if (fileName.Contains("BMT ON"))
                    {
                        bmtPrefix = 1;
                    }
                }

                fileName = dbsPrefix + "-" + bmtPrefix + "-" + fileName;

                if (filePath.Contains("nadazny250"))
                {
                    File.Copy(filePath, destFolder + @"POM250Sources/" + fileName);
                }
                else if (filePath.Contains("nadazny500"))
                {
                    File.Copy(filePath, destFolder + @"POM500Sources/" + fileName);
                }
                else if (filePath.Contains("nadazny125"))
                {
                    File.Copy(filePath, destFolder + @"POM125Sources/" + fileName);
                }
                else if (filePath.Contains("antysakady"))
                {
                    File.Copy(filePath, destFolder + @"AntySakady/" + fileName);
                }
                else if (filePath.Contains("sakady"))
                {
                    File.Copy(filePath, destFolder + @"Sakady/" + fileName);
                }
            }
        }
    }
}
