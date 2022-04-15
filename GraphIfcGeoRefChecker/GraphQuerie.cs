using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace GraphIfcGeoRefChecker
{
    public class GraphQuerie
    {
        public static IGraph temp = new VDS.RDF.Graph();

        public static VariablePattern element = new VariablePattern("element");
        public static VariablePattern attribute = new VariablePattern("attribute");
        public static VariablePattern value = new VariablePattern("value");

        public static IGraph SetNamespaces()
        {
            NamespaceMapper NM = new NamespaceMapper();

            NM.AddNamespace("inst", new Uri("http://linkedbuildingdata.net/ifc/resources20220103_124941/"));
            // Konstant:
            NM.AddNamespace("ifc", new Uri("https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD1/OWL#"));
            NM.AddNamespace("ifcext", new Uri("http://www.semanticweb.org/aaron/ontologies/2022/1/ifcextension#"));
            NM.AddNamespace("expr", new Uri("https://w3id.org/express#"));
            NM.AddNamespace("list", new Uri("https://w3id.org/list#"));
            NM.AddNamespace("onto", new Uri("http://www.ontotext.com/"));
            NM.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            NM.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            NM.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));

            temp.NamespaceMap.Import(NM);
            return temp;
        }

        public static SparqlQuery queriebuilder(SparqlQuery subsparqlQuery)
        {
            temp = SetNamespaces();

            SparqlVariable[] SVlist = new SparqlVariable[]{
                new SparqlVariable(element.VariableName),
                new SparqlVariable(attribute.VariableName),
                new SparqlVariable(value.VariableName)
                };

            VariablePattern a1 = new VariablePattern("a1");
            VariablePattern a2 = new VariablePattern("a2");
            VariablePattern a3 = new VariablePattern("a3");
            VariablePattern a4 = new VariablePattern("a4");
            VariablePattern a5 = new VariablePattern("a5");
            VariablePattern b1 = new VariablePattern("b1");

            UriNode type = (UriNode)temp.CreateUriNode("rdf:type");
            UriNode subClassOf = (UriNode)temp.CreateUriNode("rdfs:subClassOf");

            SparqlQuery sq =
                QueryBuilder
                .Select(SVlist)
                .Distinct()
                .Where(
                    new SubQueryPattern(subsparqlQuery),
                    new TriplePattern(
                        element,
                        attribute,
                        a1
                        ),
                    new TriplePattern(
                        a1,
                        a2,
                        a3
                        )
                    )
                .Filter(new NotInFunction(new ConstantTerm(type), new List<VariableTerm>() { new VariableTerm(attribute.VariableName), new VariableTerm(a2.VariableName) }))
                .Where(new TriplePattern(a1, new NodeMatchPattern(type), a4))

                .Bind(b => b.If(b.IsLiteral(b.Variable(a3.VariableName))).Then(b.StrDt(b.Str(b.Variable(a3.VariableName)), b.Variable(a4.VariableName))).Else(b.Variable(a1.VariableName)))
                .As(value.VariableName)
                .BuildQuery();


            // set FROM <http://www.ontotext.com/explicit>
            sq.AddDefaultGraph(new Uri("http://www.ontotext.com/explicit"));
            return sq;
        }

        public static SparqlQuery ObjQuery(string ifcclass) // Get IfcProject, IfcSite & IfcBuilding
        {
            temp = SetNamespaces();

            UriNode type = (UriNode)temp.CreateUriNode("rdf:type");
            UriNode ifcclassnode = (UriNode)temp.CreateUriNode("ifc:" + ifcclass);

            SparqlQuery subquery =
                QueryBuilder
                .Select(new SparqlVariable(element.VariableName))
                .Distinct()
                .Where(
                    new TriplePattern(
                        // Subject:
                        element,
                        // Predicate: rdf:type
                        new NodeMatchPattern(type),
                        // Object:
                        new NodeMatchPattern(ifcclassnode)
                        )
                    ).BuildQuery();

            SparqlQuery sq = queriebuilder(subquery);

            return sq;
        }

        public static SparqlQuery GeometricRepresentationQuery(string instproject) // Get IfcProject, IfcSite & IfcBuilding
        {
            temp = SetNamespaces();

            UriNode type = (UriNode)temp.CreateUriNode("rdf:type");
            UriNode ifcclassnode = (UriNode)temp.CreateUriNode("inst:" + instproject);

            SparqlQuery subquery =
                QueryBuilder
                .Select(new SparqlVariable(element.VariableName))
                .Distinct()
                .Where(
                    new TriplePattern(
                        // Subject:
                        element,
                        // Predicate: rdf:type
                        new NodeMatchPattern(type),
                        // Object:
                        new NodeMatchPattern(ifcclassnode)
                        )//,
                    //new PropertyPathPattern(
                     //   element,
                      //  new InversePath(), //?element ^ifc:representationContexts_IfcContext / rdf:type ifc:IfcProject  . oder
                      //                     //?element ^ifc:representationContexts_IfcContext inst:IfcProject_66  .
                      //  new NodeMatchPattern((UriNode)temp.CreateUriNode("ifc:IfcProject"))
                      //  )
                    ).BuildQuery();

            SparqlQuery sq = queriebuilder(subquery);

            return sq;
        }
    }
}
