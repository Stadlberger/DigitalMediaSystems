using System.IO;

namespace ImageRetrevial
{
    public interface ISearchResult
    {
        string ImageName { get; set; }
        string RelativeURI { get; set; }
        string Title { get; set; }
        string Description { get; set; }
    }
    class SearchResult : ISearchResult
    {
        string m_fileName;
        string m_URI;
        string m_descripton;
        string m_title;

        public string ImageName
        {
            get { return m_fileName; }
            set { }
        }

        public string RelativeURI
        {
            get { return m_URI; }
            set { }
        }

        public string Title
        {
            get { return m_title; }
            set { }
        }

        public string Description
        {
            get { return m_descripton; }
            set { }
        }

        public SearchResult(string url, string title, string fileName, string description = "empty")
        {
            m_URI = url;
            m_title = title;
            m_fileName = fileName;
            m_descripton = description;
        }
    }
}
