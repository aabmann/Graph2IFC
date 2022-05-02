using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using System.Data;
using System.Linq;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Common.Step21;
using Xbim.Ifc4.SharedBldgElements;
using VDS.RDF.Query.Inference;
using Xbim.Ifc4.ProductExtension;
using Graph2Ifc.SPARQLqueries;
using Graph2Ifc.XBimIFC;
using Xbim.Common.XbimExtensions;
using VDS.RDF.Nodes;
using System.Reflection;
using Xbim.Common;
using System.IO;
using System.Threading.Tasks;

namespace Graph2Ifc
{
    public class Graph2Ifc
    {

        //private static readonly HttpClient client = new HttpClient();
        public static async Task exportenities()
        {
            Assembly assembly = Assembly.Load("Xbim.Ifc4");
            IEnumerable<string> subtypesPersistEntity = assembly.GetTypes().Where(t => t.BaseType == typeof(PersistEntity)).Select(t => t.Name);
            int x = subtypesPersistEntity.Count();
            IEnumerable<Type> selecttypes = assembly.GetTypes().Where(t => typeof(IExpressSelectType).IsAssignableFrom(t) & t.IsAbstract == true & !t.IsSubclassOf(typeof(PersistEntity)));
            await File.WriteAllLinesAsync("../../Ifc4Entities.txt", subtypesPersistEntity);
        }

        public static void Graph2IfcMain(Uri repository, string output)
        {
            //exportenities();

            Queriebuilder.endpoint = repository;

            Queriebuilder.SetNamespaces();

            // check IFC Version
            SparqlQuery SQifcversionquerie = Queriebuilder.GetIfcVersion();
            SparqlResultSet SRSifcversionquerie = Queriebuilder.abfrage(SQifcversionquerie);

            string ifcversion = null;

            if(SRSifcversionquerie.Count > 1)
            {
                throw new Exception("recieved to many IfcVersion from database");
            }
            else if (SRSifcversionquerie.Count == 1)
            {
                ifcversion = SRSifcversionquerie[0]["IfcVersion"].ToString();
            }
            else
            {
                throw new Exception("Error");
            }

            if (ifcversion.Contains("IFC4"))
            {
                BuildingElements.fileName = output;

                SparqlQuery SQifcentities = Queriebuilder.queriemaingraph("IfcEntities");
                SparqlResultSet SRSifcentities = Queriebuilder.abfrage(SQifcentities);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsifcentities = sortresults(SRSifcentities);

                /*SparqlQuery SQdefinitionselect = Queriebuilder.queriemaingraph("IfcDefinitionSelect");
                SparqlResultSet SRSdefinitionselect = Queriebuilder.abfrage(SQdefinitionselect);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsdefinitionselect = sortresults(SRSdefinitionselect);

                SparqlQuery SQifcrelationship = Queriebuilder.queriemaingraph("IfcRelationship");
                SparqlResultSet SRSifcrelationship = Queriebuilder.abfrage(SQifcrelationship);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsifcrelationship = sortresults(SRSifcrelationship);

                // IfcPropertyDefinition
                //SparqlQuery SQpropertydefinition = Queriebuilder.queriemaingraph("IfcPropertyDefinition"); // IfcDefinitionSelect
                //SparqlResultSet SRSpropertydefinition = Queriebuilder.abfrage(SQpropertydefinition);
                //Dictionary<string, Dictionary<string, List<dynamic>>> resultspropertydefinition = Graph2Ifc.sortresults(SRSpropertydefinition);

                SparqlQuery SQproperty = Queriebuilder.queriemaingraph("IfcResourceObjectSelect");
                SparqlResultSet SRSproperty = Queriebuilder.abfrage(SQproperty);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsproperty = Graph2Ifc.sortresults(SRSproperty);

                // IfcLayeredItem
                SparqlQuery SQlayereditem = Queriebuilder.queriemaingraph("IfcLayeredItem");
                SparqlResultSet SRSlayereditem = Queriebuilder.abfrage(SQlayereditem);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultslayereditem = Graph2Ifc.sortresults(SRSlayereditem);


                SparqlQuery SQproductrepresentationselect = Queriebuilder.queriemaingraph("IfcProductRepresentationSelect");
                SparqlResultSet SRSproductrepresentationselect = Queriebuilder.abfrage(SQproductrepresentationselect);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsproductrepresentationselect = Graph2Ifc.sortresults(SRSproductrepresentationselect);


                SparqlQuery SQpresentationitem = Queriebuilder.queriemaingraph("IfcPresentationItem");
                SparqlResultSet SRSpresentationitem = Queriebuilder.abfrage(SQpresentationitem);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultspresentationitem = Graph2Ifc.sortresults(SRSpresentationitem);


                SparqlQuery SQrepresentationcontext = Queriebuilder.queriemaingraph("IfcRepresentationContext");
                SparqlResultSet SRSrepresentationcontext = Queriebuilder.abfrage(SQrepresentationcontext);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsrepresentationcontext = Graph2Ifc.sortresults(SRSrepresentationcontext);


                SparqlQuery SQpresentationlayerassignment = Queriebuilder.queriemaingraph("IfcPresentationLayerAssignment");
                SparqlResultSet SRSpresentationlayerassignment = Queriebuilder.abfrage(SQpresentationlayerassignment);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultspresentationlayerassignment = Graph2Ifc.sortresults(SRSpresentationlayerassignment);


                SparqlQuery SQstyleassignmentselect = Queriebuilder.queriemaingraph("IfcStyleAssignmentSelect");
                SparqlResultSet SRSstyleassignmentselect = Queriebuilder.abfrage(SQstyleassignmentselect);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsstyleassignmentselect = Graph2Ifc.sortresults(SRSstyleassignmentselect);


                SparqlQuery SQconnectiongeometry = Queriebuilder.queriemaingraph("IfcConnectionGeometry");
                SparqlResultSet SRSconnectiongeometry = Queriebuilder.abfrage(SQconnectiongeometry);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsconnectiongeometry = Graph2Ifc.sortresults(SRSconnectiongeometry);


                SparqlQuery SQobjectplacement = Queriebuilder.queriemaingraph("IfcObjectPlacement");
                SparqlResultSet SRSobjectplacement = Queriebuilder.abfrage(SQobjectplacement);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsobjectplacement = Graph2Ifc.sortresults(SRSobjectplacement);


                Dictionary<string, Dictionary<string, List<dynamic>>> allresults = resultsdefinitionselect
                    .Concat(resultsifcrelationship)
                    .Concat(resultsproperty)
                    .Concat(resultslayereditem)
                    .Concat(resultsproductrepresentationselect)
                    .Concat(resultspresentationitem)
                    .Concat(resultsrepresentationcontext)
                    .Concat(resultsstyleassignmentselect)
                    .Concat(resultsconnectiongeometry)
                    .Concat(resultsobjectplacement)
                    .ToDictionary(x => x.Key, x => x.Value);

                resultsifcrelationship.Clear();
                resultsproperty.Clear();
                resultslayereditem.Clear();
                resultsproductrepresentationselect.Clear();*/

                var model = IfcStore.Create(/*editor,*/ XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
                BuildingElements.GenerateIfcProject(model, resultsifcentities);
            }

            else if (ifcversion.Contains("IFC2X3"))
            {

                BuildingElements_Ifc2x3.fileName = output;

                SparqlQuery SQroot = Queriebuilder.queriemaingraph("IfcEntities");
                SparqlResultSet SRSroot = Queriebuilder.abfrage(SQroot);
                Dictionary<string, Dictionary<string, List<dynamic>>> resultsroot = sortresults(SRSroot);

                var model = IfcStore.Create(/*editor,*/ XbimSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);
                Dictionary<string, dynamic> ifcentities = new Dictionary<string, dynamic>();
                BuildingElements_Ifc2x3.GenerateIfcProject(model, resultsroot);
            }



        }
        
        // element attribute value
        public static Dictionary<string, Dictionary<string, List<dynamic>>> sortresults(SparqlResultSet SRS) 
        {
            Dictionary<string, Dictionary<string, List<dynamic>>> sortedresults = new Dictionary<string, Dictionary<string, List<dynamic>>>();
            foreach (SparqlResult SR in SRS)
            {
                if(!SR.IsGroundResult)
                {
                    break;
                }

                string id = SR.HasBoundValue("element") ? SR["element"].ToString() : null;

                string attr = null;

                if (SR["attribute"].NodeType == NodeType.Uri)
                {
                    UriNode urinode = (UriNode)SR["attribute"];
                    string fragment = urinode.Uri.Fragment;
                    attr = fragment.Substring(fragment.IndexOf('#') + 1, fragment.Contains('_') ? fragment.LastIndexOf('_') - fragment.IndexOf('#') -1 : fragment.Length - fragment.IndexOf('#') - 1).ToLower();
                }
                else
                {
                    Console.Write("ERROR: non Uri Node in Result as Attribute \n");
                }
                //string attrsubstring = null;
                //if (attr.Contains("#") & attr.Contains("_"))
                //{
                //    attrsubstring = attr.Substring(attr.LastIndexOf("#") + 1, attr.LastIndexOf("_") - attr.LastIndexOf("#") - 1);
                //}
                //else if (attr.Contains("#") & !attr.Contains("_"))
                //{
                //    attrsubstring = attr.Substring(attr.LastIndexOf("#") + 1);
                //}
                //else if (!attr.Contains("#") & !attr.Contains("_"))
                //{
                //    attrsubstring = attr.Substring(attr.LastIndexOf("/") + 1);
                //}

                if (!sortedresults.ContainsKey(id))
                {
                    sortedresults.Add(id, new Dictionary<string, List<dynamic>>());
                }

                if (!sortedresults[id].ContainsKey(attr))
                {
                    //attrsubstring, SR.HasBoundValue("value") ? SR["value"].ToString() : "_" 
                    sortedresults[id].Add(attr, new List<dynamic>() { SR["value"] });

                    /*Type nodetype = SR["value"].GetType();

                    if (SR["value"].NodeType == NodeType.Literal)
                    {
                        LiteralNode literalnode = (LiteralNode)SR["value"];
                        
                    }
                    else if(SR["value"].NodeType == NodeType.Uri)
                    {
                        UriNode urinode = (UriNode)SR["value"];
                    }*/
                }
                else if (sortedresults[id].ContainsKey(attr))
                {
                    bool sametype = sortedresults[id][attr][0].GetType() == SR["value"].GetType();
                    if (sametype)
                    {
                        sortedresults[id][attr].Add(SR["value"]);
                    }
                    else
                    {
                        Console.Write("Error: Different types of values at " + id + attr);
                    }
                    
                }
            }
            return sortedresults;
        }
    }
}