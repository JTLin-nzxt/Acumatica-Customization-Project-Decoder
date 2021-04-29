using System;
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace AcumaticaCustomizatiomProjectDecoder
{
    class AcumaticaCustomizatiomProjectDecoder
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the folder path :");

            string rootPath = Console.ReadLine();

            string[] documentPaths = Directory.GetFiles(rootPath, "", SearchOption.AllDirectories);
            // if project.xml not exist, do it in child paths.
            if (Array.IndexOf(documentPaths, String.Format("{0}\\project.xml", rootPath)) == -1)
            {
                documentPaths = Directory.GetDirectories(rootPath, "", SearchOption.TopDirectoryOnly);
                foreach (string documentPath in documentPaths)
                {
                    Console.WriteLine(documentPath);
                    string filePath = String.Format("{0}.zip", documentPath);
                    try 
                    {
                        await DecodeProjectXMLAsync(documentPath);
                        await ZipFile(filePath, documentPath);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(String.Format("Error : {0}", e.Message));
                    }
                }
            }
            // if project.xml exit, just do it in rootPath.
            else
            {
                try
                {
                    string filePath = String.Format("{0}.zip", rootPath);
                    await DecodeProjectXMLAsync(rootPath);
                    await ZipFile(filePath, rootPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(String.Format("Error : {0}", e.Message));
                }
            }
        }

        public static Task ZipFile(string filePath, string folderPath)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(folderPath, filePath);
            return Task.CompletedTask;
        }  

        public static Task DecodeProjectXMLAsync(string folderPath)
        {
            var xmldoc = new XmlDocument();
            string xmlFilePath = string.Format("{0}\\project.xml", folderPath);
            try
            {
                xmldoc.Load(xmlFilePath);
                // trans cs class
                XmlNodeList graphNodes = xmldoc.GetElementsByTagName("Graph");
                foreach (XmlNode node in graphNodes)
                {
                    string csResult = "";
                    string csFile = string.Format("{0}\\{1}", folderPath, node.InnerText);
                    //open the .cs file and save to csResult
                    using (StreamReader sr = File.OpenText(csFile))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            csResult += s + "\n";
                        }
                    }

                    // delete the .cs file
                    File.Delete(csFile);

                    // create CDATA element to save the .cs file
                    XmlElement cdataElement = xmldoc.CreateElement("CDATA");
                    cdataElement.SetAttribute("name", "Source");
                    var cdata = xmldoc.CreateCDataSection(Convert.ToString(csResult));
                    cdataElement.AppendChild(cdata);

                    // save change to project.xml
                    node.InnerXml = cdataElement.OuterXml;
                }
                xmldoc.Save(xmlFilePath);
            }
            catch(Exception e) 
            {
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }
    }
}