using System.IO;

namespace ImageRetrevial
{
    class Config
    {
        static Config m_instance;

        static string m_pathToConfig = "config.txt";

        public string m_pathToXML;
        public string m_pathToData;
        public string m_pathToImages;
        public string m_pathToCSV;

        Config()
        {
            using (StreamReader reader = new StreamReader(m_pathToConfig))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var tokens = line.Split('=');

                    switch (tokens[0])
                    {
                        case "pathToXML":
                            m_pathToXML = tokens[1];
                            break;

                        case "pathToData":
                            m_pathToData = tokens[1];
                            break;

                        case "pathToImages":
                            m_pathToImages = tokens[1];
                            break;

                        case "pathToCSV":
                            m_pathToCSV = tokens[1];
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public static Config Get()
        {
            if (m_instance == null) m_instance = new Config();
            return m_instance;
        }
    }
}
