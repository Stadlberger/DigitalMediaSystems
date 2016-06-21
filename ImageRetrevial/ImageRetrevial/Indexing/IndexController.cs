using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Snowball;

using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;

namespace ImageRetrevial
{
    struct QueryData
    {
        public QueryData(string fieldName, string fieldValue)
        {
            m_fieldName = fieldName;
            m_fieldValue = fieldValue;
        }

        public string m_fieldName;
        public string m_fieldValue;
    }

    /// <summary>Used for creating and querying indexes of our data.</summary>
    class IndexController
    {
        string m_xmlRoot = @"C:\Users\Philipp\Desktop\xml\xml\";
        DirectoryInfo m_indexDir;
        IndexSearcher m_searcher;

        public IndexController()
        {
            if (!Directory.Exists(m_xmlRoot + @"..\index"))
            {
                Directory.CreateDirectory(m_xmlRoot + @"..\index");
            }

            m_indexDir = new DirectoryInfo(m_xmlRoot + @"..\index");
            if (m_indexDir.GetFiles().Length == 0)
            {
                //Parallel.ForEach(new DirectoryInfo(m_xmlRoot).EnumerateFiles(), (fileInfo) => {
                //    BuildIndex(fileInfo.Name);
                //});
                BuildIndex();
            }

            m_searcher = new IndexSearcher(FSDirectory.Open(m_indexDir));
        }

        void BuildIndex()
        {
            IndexWriter writer = new IndexWriter(FSDirectory.Open(m_indexDir), new SnowballAnalyzer(Version.LUCENE_30, "Analyzer"), true, IndexWriter.MaxFieldLength.UNLIMITED);
            var documents = new List<Document>();
            
            foreach (var fileInfo in new DirectoryInfo(m_xmlRoot).EnumerateFiles())
            {
                // Read and parse XML file
                string xmlText = File.ReadAllText(m_xmlRoot + fileInfo.Name);
                string topicName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
                XmlReader reader = XmlReader.Create(new StringReader(xmlText));

                // Create index data from XML
                while (reader.Read())
                {
                    documents.Add(new Document());
                    documents[documents.Count - 1].Add(new Field("Topic", topicName, Field.Store.YES, Field.Index.ANALYZED));

                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        documents[documents.Count - 1].Add(new Field(reader.Name, reader.Value, Field.Store.YES, Field.Index.ANALYZED));
                    }
                }
            };
            
            foreach (var doc in documents) writer.AddDocument(doc);
            writer.Optimize();
            writer.Commit();
            writer.Dispose();
        }

        ///<summary>Used for testing querying while query building is not yet support. Do not ship.</summary>
        public void TempQueryWithString(string queryString)
        {
            var tokens = queryString.Split(' ');
            QueryData[] queryData = new QueryData[1];
            queryData[0] = new QueryData(tokens[0], tokens[1]);
            QueryIndex(queryData);
        }

        public void QueryIndex(QueryData[] queries)
        {
            BooleanQuery boolQuery = new BooleanQuery();

            foreach (var query in queries)
            {
                Term term = new Term(query.m_fieldName, query.m_fieldValue);
                Query query1 = new TermQuery(term);
                boolQuery.Add(query1, Occur.SHOULD);
            }

            TopScoreDocCollector topDocColl = TopScoreDocCollector.Create(10, true);
            m_searcher.Search(boolQuery, topDocColl);
            TopDocs topDocs = topDocColl.TopDocs();

            Console.WriteLine("Number of hits {0}", topDocs.TotalHits);

            foreach (var searchHit in topDocs.ScoreDocs)
            {
                Console.WriteLine(searchHit.Doc + ". " + m_searcher.Doc(searchHit.Doc).GetField("description").StringValue);
            }
        }

    }
}
