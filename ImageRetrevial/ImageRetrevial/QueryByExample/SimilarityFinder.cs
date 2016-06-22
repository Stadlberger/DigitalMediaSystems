﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Accord.Imaging;
using Accord.MachineLearning;
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
            //savedImages = parseCSV(@"D:\div-2014\devset\descvis\descvis\img\acropolis_athens HOG.csv");
        }

        private void parseCSV(string path)
        {

            try
            {
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
                        if(!savedImages.ContainsKey(row[0]))
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
            /*Parallel.ForEach(savedImages, kvp =>
            {
                double dist = savedImages[index].Euclidean(kvp.Value);
                lock (images) images.Add(new Tuple<string, double>(kvp.Key, dist));
            });*/
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
            /*Parallel.ForEach(savedImages, kvp =>
            {
                double dist = savedImages[index].Euclidean(kvp.Value);
                lock (images) images.Add(new Tuple<string, double>(kvp.Key, dist));
            });*/
            images.Sort((tuple, tuple1) => tuple.Item2 > tuple1.Item2 ? 1 : -1);

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
            //Perform Search
            //Get Results ... return List<ISearchResult>
            //Display them
            

            return SearchResults;
        }

        //public List<Tuple<string, double>> getSimilarImagesByFile(string path, int count)
        //{
        //    Bitmap refImg = (Bitmap)Bitmap.FromFile(path);
        //    HistogramsOfOrientedGradients hog = new HistogramsOfOrientedGradients();

        //    List<double> tempDoubles = new List<double>();

        //    foreach (var darray in hog.ProcessImage(refImg))
        //    {
        //        foreach (var doub in darray)
        //        {
        //            tempDoubles.Add(doub);
        //        }
        //    }

        //    double[] currImage = tempDoubles.ToArray();



        //    DateTime start = DateTime.Now;
        //    List<Tuple<string, double>> images = new List<Tuple<string, double>>();
        //    foreach (var kvp in savedImages)
        //    {
        //        double dist = currImage.Euclidean(kvp.Value);
        //        images.Add(new Tuple<string, double>(kvp.Key, dist));
        //    }
        //    images.Sort((tuple, tuple1) => tuple.Item2 > tuple1.Item2 ? 1 : -1);
        //    Console.Write("This took " + DateTime.Now.Subtract(start).TotalMilliseconds + " milliseconds");
        //    return images.GetRange(0, count);
        //}
    }
}
