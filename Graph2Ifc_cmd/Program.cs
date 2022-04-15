using System;
using Graph2Ifc;

namespace Graph2Ifc_cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri repository = new Uri(args[0]);

            string output = args[1];

            Graph2Ifc.Graph2Ifc.Graph2IfcMain(repository, output);
            // repository URl 
            // ausgabe
            
        }
    }
}
