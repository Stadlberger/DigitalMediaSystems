using System;
using System.Collections.Generic;
using System.IO;

namespace guimoc
{
    class ControllerMoc
    {
        Dictionary<String, ISearchResult> m_data;
        string m_assetPath;

        public ControllerMoc(string assetPath)
        {
            m_assetPath = assetPath;
            m_data = new Dictionary<string, ISearchResult>();

            BuildData();
        }

        void BuildData()
        {
            try
            {   
                foreach (var url in Directory.GetFiles(m_assetPath))
                {
                    ISearchResult data = new SearchResult(url, LoremIpsum.Generate(15, 35, 1, 2, 1));
                    m_data.Add(data.FileName, data);               
                }
            }
            catch (DirectoryNotFoundException excep)
            {
                Console.WriteLine(excep.Message);
            }
        }

        public ICollection<ISearchResult> GetResults()
        {
            return new List<ISearchResult>(m_data.Values);
        }
    }
}
