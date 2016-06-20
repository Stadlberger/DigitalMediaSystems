using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;

using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;

using Accord;
using Accord.Imaging;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics.Kernels;
using AForge;
//using Accord.Math.Distances;

/*\
|*| This is the same program found on the DMS wiki
|*| site, but with explanatory comments added.
|*| No spelling guarantees provided.
\*/

namespace ImageRetrieval
{
    class Program
    {
        static DirectoryInfo INDEX_DIR = new DirectoryInfo("index");

        static void Main(string[] args)
        {
            Index();
            Query();
            ImageFeatures();
        }

        static void Index()
        {
            // Used to write out an index for later usage.
            IndexWriter writer = new IndexWriter(FSDirectory.Open(INDEX_DIR), new StandardAnalyzer(Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED);

            try
            {
                // A series of fields, used for indexing and searching.
                Document doc1 = new Document();

                // https://lucenenet.apache.org/docs/3.0.3/db/d65/class_lucene_1_1_net_1_1_documents_1_1_document.html#details
                // https://lucene.apache.org/core/3_0_3/api/core/org/apache/lucene/document/Field.Index.html
                // A field contains a name and a string value. 
                // If a field is stored, it will be returned with the document when
                // a search lands a hit on it.
                // Not indexing a field leads to it not being searchable.
                doc1.Add(new Field("name", "doc 1", Field.Store.YES, Field.Index.NO));

                // This field is indexed, meaning it is searchable using a query, as well as analyzed,
                // meaning it is run through an analyzer ( which leads to it being split into a searchable, tokenized form ).
                // Alternatively, a field may be indexed but not analyzed, leading to it being stored as a single token.
                // This may be useful for single value fields like names or ids.
                doc1.Add(new Field("content", "abc xyz", Field.Store.YES, Field.Index.ANALYZED));

                // Finally, we add the document to index writer.
                writer.AddDocument(doc1);

                Document doc2 = new Document();
                doc2.Add(new Field("name", "doc 2", Field.Store.YES, Field.Index.NO));
                doc2.Add(new Field("content", "abc defg defg defg", Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc2);

                Document doc3 = new Document();
                doc3.Add(new Field("name", "doc 3", Field.Store.YES, Field.Index.NO));
                doc3.Add(new Field("content", "qwerty defg defg", Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc3);

                Console.Out.WriteLine("Optimizing...");

                // Optimizes the created index for fast search. Optimally call when the index is 
                // created fully.
                writer.Optimize();

                // Takes all changes to the index and merges them. After this call all changes
                // are permanently written into the index.
                writer.Commit();
            }
            catch (IOException excep)
            {
                Console.WriteLine(excep.Message);
            }

            writer.Dispose();
        }

        static void Query()
        {
            try
            {
                // Counterpart to the IndexWriter used above.
                IndexReader reader = IndexReader.Open(FSDirectory.Open(INDEX_DIR), true);
                Console.Out.WriteLine("Number of indexed docs: " + reader.NumDocs());

                IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(INDEX_DIR));

                // Construct a search query. String 1 in term is the field name, String 2 the content to match against.
                // The name of the field "content" here and how we filled a "content" field using the IndexWrite above.
                // Apparently  BooleanQuery  can be used to combine search queries.
                Term searchTerm = new Term("content", "defg");
                TermQuery query = new TermQuery(searchTerm);

                // Used to collect the highest scoring hits when searching.
                TopScoreDocCollector topDocColl = TopScoreDocCollector.Create(10, true);
                searcher.Search(query, topDocColl);

                // Collection of documents matched using the search query.
                TopDocs topDocs = topDocColl.TopDocs();
                Console.WriteLine("Number of hits: " + topDocs.TotalHits);

                // Traverse through the search hits, print their index in the query and the document's name.
                foreach (var searchHit in topDocs.ScoreDocs)
                {
                    Console.WriteLine(searchHit.Doc + ". " + searcher.Doc(searchHit.Doc).GetField("name").StringValue);
                }

            }
            catch (IOException excep)
            {
                Console.Out.WriteLine(excep.Message);
            }
        }

        static void ImageFeatures()
        {
            Dictionary<string, Bitmap> testImages = new Dictionary<string, Bitmap>();

            testImages.Add("img_acropolis", (Bitmap)Bitmap.FromFile("test_imgs/acropolis_athens.jpg"));
            testImages.Add("img_cathedral", (Bitmap)Bitmap.FromFile("test_imgs/amiens_cathedral.jpg"));
            testImages.Add("img_bigben", (Bitmap)Bitmap.FromFile("test_imgs/big_ben.jpg"));

            int numberOfWords = 6; // number of cluster centers: typically >>100

            // Create a Binary-Split clustering algorithm
            BinarySplit binarySplit = new BinarySplit(numberOfWords);

            IBagOfWords<Bitmap> bow;
            // Create bag-of-words ( BoW ) with the given algorithm
            BagOfVisualWords surfBow = new BagOfVisualWords(binarySplit);

            // Compute the BoW codebook using training images only
            Bitmap[] bmps = new Bitmap[testImages.Count];
            testImages.Values.CopyTo(bmps, 0);
            surfBow.Compute(bmps);
            bow = surfBow;

            // this model needs to be saved once it is calculated: only compute it once to calculate features 
            // from the collection as well as for new queries.
            // THE SAME TRAINED MODEL MUST BE USED TO GET THE SAME FEATURES!!!


            Dictionary<string, double[]> testImageFeatures = new Dictionary<string, double[]>();

            // Extract features for all images
            foreach (string imagename in testImages.Keys)
            {
                double[] featureVector = bow.GetFeatureVector(testImages[imagename]);
                testImageFeatures.Add(imagename, featureVector);
                Console.Out.WriteLine(imagename + " features: " + featureVector.ToString(DefaultArrayFormatProvider.InvariantCulture));
            }

            // Calculate Image Similarities
            string[] imagenames = new string[testImageFeatures.Keys.Count];
            testImageFeatures.Keys.CopyTo(imagenames, 0);

            for (int i = 0; i < imagenames.Length; i++)
            {
                for (int j = i + 1; j < imagenames.Length; j++)
                {
                    double dist = Distance.Cosine(testImageFeatures[imagenames[i]], testImageFeatures[imagenames[j]]);
                    Console.Out.WriteLine(imagenames[i] + " <-> " + imagenames[j] + " distance: " + dist.ToString());
                }
            }
        }
    }
}
