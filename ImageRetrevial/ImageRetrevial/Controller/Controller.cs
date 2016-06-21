using System;
using System.Collections.Generic;
using System.IO;

namespace ImageRetrevial
{  
    class DataController
    {
        IndexController m_indexController;
        
        public DataController()
        {
            m_indexController = new IndexController();
        }
        
        public ICollection<ISearchResult> RunQuery(QueryData[] querys)
        {
            return m_indexController.QueryIndex(querys);
        }      
    }
}
