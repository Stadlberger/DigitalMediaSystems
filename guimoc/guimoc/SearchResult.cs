using System.IO;

namespace guimoc
{
    public interface ISearchResult
    {
        string ImageName { get; set; }
        string RelativeURL { get; set; }
        string FileName { get; set; }
        string Description { get; set; }
    }
    class SearchResult : ISearchResult
    {
        string m_imageName;
        string m_URL;
        string m_descripton;
        string m_fileName;

        public string ImageName
        {
            get { return m_imageName; }
            set { }
        }

        public string RelativeURL
        {
            get { return m_URL; }
            set { }
        }

        public string FileName
        {
            get { return m_fileName; }
            set { }
        }

        public string Description
        {
            get { return m_descripton; }
            set { }
        }

        public SearchResult(string url, string description = "empty")
        {
            m_URL = url;
            m_imageName = Path.GetFileNameWithoutExtension(m_URL);
            m_descripton = description;
            m_fileName = Path.GetFileName(m_URL);
        }
    }
}
