using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accord.Math;
using ImageRetrevial;

namespace QueryImage
{
    class SimilarityFinder
    {
        private Dictionary<string, double[]> savedImages;
        private DataController dataController;

        public SimilarityFinder(string assetPath, string features, DataController dataController)
        {
            this.dataController = dataController;
            savedImages = new Dictionary<string, double[]>();

            foreach (var url in Directory.GetFiles(assetPath))
            {
                if (url.EndsWith(features + ".csv"))
                    parseCSV(url);
            }
        }

        private void parseCSV(string path)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                using (StreamReader readFile = new StreamReader(path))
                {
                    string line;
                    string[] row;

                    while ((line = readFile.ReadLine()) != null)
                    {
                        List<double> values = new List<double>();
                        row = line.Split(',');
                        for (int i = 1; i < row.Length; i++)
                            values.Add(double.Parse(row[i]));
                        if (!savedImages.ContainsKey(row[0]))
                            savedImages.Add(row[0],values.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public List<Tuple<string,double>> getSimilarImagesVerbose(string index, int count)
        {
            DateTime start = DateTime.Now;
            List <Tuple<string,double>> images = new List<Tuple<string, double>>();

            foreach (var kvp in savedImages)
            {
                double dist = savedImages[index].Euclidean(kvp.Value);
                images.Add(new Tuple<string, double>(kvp.Key, dist));   
            }

            images.Sort((tuple, tuple1) => tuple.Item2>tuple1.Item2?1:-1);
            Console.Write("This took " + DateTime.Now.Subtract(start).TotalMilliseconds + " milliseconds");
            return images.GetRange(0, count);
        }
        
        public List<ISearchResult> getSimilarImages(string index, int count)
        {
            List<Tuple<string, double>> images = new List<Tuple<string, double>>();

            foreach (var kvp in savedImages)
            {
                double dist = savedImages[index].Euclidean(kvp.Value);
                images.Add(new Tuple<string, double>(kvp.Key, dist));
            }

            images.Sort((tuple, tuple1) => tuple.Item2 > tuple1.Item2 ? 1 : -1);
            images = images.GetRange(0, count);

            List<ISearchResult> SearchResults = new List<ISearchResult>();
            QueryData[] qData = new QueryData[1];

            for (int i = 0; i < images.Count; i++)
            {
                QueryData data = new QueryData();
                data.m_fieldName = "id";
                data.m_fieldValue = images[i].Item1;

                qData[0] = data;

                SearchResults.Add(dataController.RunQuery(qData).ToList()[0]);
            }
            return SearchResults;
        }
    }
}
