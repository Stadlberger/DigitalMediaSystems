using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guimoc
{
    class Program
    {
        static void Main(string[] args)
        {
            ControllerMoc moc = new ControllerMoc("../../images/");
            
            foreach (var file in moc.GetResults())
            {
                Console.WriteLine(file.ImageName);
                Console.WriteLine(file.FileName);
                Console.WriteLine(file.Description);
            }
        }
    }
}
