using System;
using System.IO;
using System.Xml;
using System.Threading.Tasks;

using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;

using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;

namespace ImageRetrevial
{
    class IndexController
    {
        string m_xmlRoot = @"C:\Users\Philipp\Desktop\xml\xml\";
        DirectoryInfo indexDir;
        IndexSearcher m_searcher;

        public IndexController()
        {
            if (!Directory.Exists(m_xmlRoot + @"..\index"))
            {
                Directory.CreateDirectory(m_xmlRoot + @"..\index");
            }

            indexDir = new DirectoryInfo(m_xmlRoot + @"..\index");

            if (indexDir.GetFiles().Length == 0)
            {
                foreach (var fileInfo in new DirectoryInfo(m_xmlRoot).EnumerateFiles())
                    BuildIndex(fileInfo.Name);
            }

            m_searcher = new IndexSearcher(FSDirectory.Open(indexDir));
        }

        public void BuildIndex(string fileName)
        {
            IndexWriter writer = new IndexWriter(FSDirectory.Open(indexDir), new StandardAnalyzer(Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED);

            string xmlText = File.ReadAllText(m_xmlRoot + fileName);
            string topicName = fileName.Substring(0, fileName.IndexOf('.'));
            XmlReader reader = XmlReader.Create(new StringReader(xmlText));

            // Parse the file and display each of the nodes.
            while (reader.Read())
            {
                Document doc = new Document();
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    doc.Add(new Field(reader.Name, reader.Value, Field.Store.YES, Field.Index.ANALYZED));
                }
                doc.Add(new Field("Topic", topicName, Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc);
            }

            writer.Optimize();
            writer.Commit();
            writer.Dispose();
        }

        

    }
}
