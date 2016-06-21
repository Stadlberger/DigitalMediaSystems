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
    struct QueryData
    {
        public string field;
        public string value;
    }

    class IndexController
    {
        string m_xmlRoot = @"D:\Eigene Daten\Dokumente\Studium\DigitalMediaSystem\Data\devset\xml\";
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

        void BuildIndex(string fileName)
        {
            IndexWriter writer = new IndexWriter(FSDirectory.Open(indexDir), new StandardAnalyzer(Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED);

            // Read and parse XML file
            string xmlText = File.ReadAllText(m_xmlRoot + fileName);
            string topicName = fileName.Substring(0, fileName.IndexOf('.'));
            XmlReader reader = XmlReader.Create(new StringReader(xmlText));
            
            // Create index data from XML
            while (reader.Read())
            {
                Document doc = new Document();
                doc.Add(new Field("Topic", topicName, Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc);

                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    doc.Add(new Field(reader.Name, reader.Value, Field.Store.YES, Field.Index.ANALYZED));
                }
            }

            writer.Optimize();
            writer.Commit();
            writer.Dispose();
        }

        public void QueryIndex(QueryData[] queries)
        {
            BooleanQuery boolQuery = new BooleanQuery();

            foreach (var query in queries)
            {
                Term term = new Term(query.field, query.value);
                Query query1 = new TermQuery(term);
                boolQuery.Add(query1, Occur.SHOULD);
            }
        }

    }
}
