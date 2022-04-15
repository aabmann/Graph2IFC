using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Parsing;
using System.Data;
using System.Linq;
using VDS.RDF.Query.Algebra;

namespace Graph2Ifc
{
    // --------------------------------OLD--------------------------------
    class SPARQLQueries
    {
        public static SparqlRemoteEndpoint EndpointConnection()
        {
            SparqlRemoteEndpoint SRE = new SparqlRemoteEndpoint(new Uri("http://localhost:7200/repositories/AC20-FZK-Haus"));
            return SRE;
        }

        public static NamespaceMapper GetNamespaces()
        {
            NamespaceMapper NM = new NamespaceMapper();

            NM.AddNamespace("ifc", new Uri("https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD1/OWL#"));
            NM.AddNamespace("expr", new Uri("https://w3id.org/express#"));
            NM.AddNamespace("inst", new Uri("http://linkedbuildingdata.net/ifc/resources20220103_124941/"));

            // Konstant:
            NM.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            NM.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            NM.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));

            return NM;
        }

        public static SparqlResultSet GetIfcOwnerHistory(SparqlRemoteEndpoint SRE)
        {

            SparqlParameterizedString IfcOwnerHistoryQueryString = new SparqlParameterizedString() { Namespaces = GetNamespaces()};

            IfcOwnerHistoryQueryString.CommandText =
            "SELECT" 
            + " ?Instanz01 ?Attributtyp01 ?AttributInstanz01 ?AttributInstanzTyp01"
            + " ?Attributtyp02 ?AttributInstanz02 ?AttributInstanzTyp02"
            + " ?Attributtyp03 ?AttributInstanz03 ?AttributInstanzTyp03"
            + " ?Attributtyp04 ?AttributInstanz04 ?AttributInstanzTyp04"
            + " ?Value"
            + " FROM <http://www.ontotext.com/explicit> "
            + " WHERE {"
                + " ?Instanz01 rdf:type ifc:IfcWindow."
                + " ?Instanz01 ?Attributtyp01 ?AttributInstanz01."
                + " ?Attributtyp01 rdf:type owl:ObjectProperty."
                + " ?AttributInstanz01 rdf:type ?AttributInstanzTyp01 ."
                + " filter not exists { ?subtype01 ^a ?AttributInstanz01 ; rdfs:subClassOf ?AttributInstanzTyp01 .}"
                + " filter not exists { ?Instanz01 ifc:ownerHistory_IfcRoot ?AttributInstanz01}"
                + " OPTIONAL { ?AttributInstanz01 expr:hasInteger | expr:hasString | rdfs:label ?Value}"
                + " FILTER EXISTS {?AttributInstanzTyp01 rdf:type owl:Class .} "

                + " OPTIONAL {"
                    + " ?AttributInstanz01 ?Attributtyp02 ?AttributInstanz02."
                    + " ?Attributtyp02 rdf:type owl:ObjectProperty."
                    + " FILTER(?AttributInstanz01 != ?AttributInstanz02)"
                    + " ?AttributInstanz02 rdf:type ?AttributInstanzTyp02 ."
                    + " filter not exists { ?subtype02 ^a ?AttributInstanz02 ; rdfs:subClassOf ?AttributInstanzTyp02 .}"
                    + " OPTIONAL { ?AttributInstanz02 expr:hasInteger | expr:hasString | rdfs:label ?Value}"

                    + " OPTIONAL {"
                        + " ?AttributInstanz02 ?Attributtyp03 ?AttributInstanz03."
                        + " ?Attributtyp03 rdf:type owl:ObjectProperty."
                        + " FILTER(?AttributInstanz01 != ?AttributInstanz03 && ?AttributInstanz02 != ?AttributInstanz03)"
                        + " ?AttributInstanz03 rdf:type ?AttributInstanzTyp03 ."
                        + " filter not exists { ?subtype03 ^a ?AttributInstanz03 ; rdfs:subClassOf ?AttributInstanzTyp03 .}"
                        + " OPTIONAL { ?AttributInstanz03 expr:hasInteger | expr:hasString | rdfs:label ?Value}"

                        + " OPTIONAL {"
                            + " ?AttributInstanz03 ?Attributtyp04 ?AttributInstanz04."
                            + " ?Attributtyp04 rdf:type owl:ObjectProperty."
                            + " FILTER(?AttributInstanz01 != ?AttributInstanz04 && ?AttributInstanz02 != ?AttributInstanz04 && ?AttributInstanz03 != ?AttributInstanz04)"
                            + " ?AttributInstanz04 rdf:type?AttributInstanzTyp04 ."
                            + " filter not exists { ?subtype04 ^a ?AttributInstanz04 ; rdfs:subClassOf ?AttributInstanzTyp04 .}"
                            + " OPTIONAL { ?AttributInstanz04 expr:hasInteger | expr:hasString | rdfs:label ?Value}"
                        + "}"
                    + "}"
                + "}"
            + "}";

            //var prefixes = new NamespaceMapper(true);
            //prefixes.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            //prefixes.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            //prefixes.AddNamespace("dce", new Uri("http://purl.org/dc/elements/1.1/"));

            //var IfcVersion = new SparqlVariable("IfcVersion");
            //var IfcVersionQuery = QueryBuilder
            //    .Select(new SparqlVariable[] { IfcVersion, new SparqlVariable("b"), new SparqlVariable("c") })
            //    .Where(
            //        (triplePatternBuilder) =>
            //        {
            //            triplePatternBuilder
            //                .Subject("x")
            //                .PredicateUri("rdf:type")
            //                .Object(new Uri("http://www.w3.org/2002/07/owl#Ontology"));
            //            triplePatternBuilder
            //                .Subject("x")
            //                .PredicateUri("dce:identifier")
            //                .Object(IfcVersion);
            //        });

            //IfcVersionQuery.Prefixes = prefixes;

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery IfcOwnerHistoryQuery = parser.ParseFromString(IfcOwnerHistoryQueryString);

            // Log-Output
            Console.WriteLine(IfcOwnerHistoryQuery.ToString());

            SparqlResultSet SRS01 = SRE.QueryWithResultSet(IfcOwnerHistoryQuery.ToString());

           // Dictionary<string, string> keyvalue = new Dictionary<string, string>();

            // schreibe resultSet in Instanz01: {(Key, Value),(Key, Value),(Key, Value)} um
            Dictionary<string, Dictionary<string, string>> Results = new Dictionary<string, Dictionary<string, string>>();
            foreach (SparqlResult SR in SRS01)
            {
                string id = SR.HasBoundValue("Instanz01") ? SR["Instanz01"].ToString() : null;
                if (!Results.ContainsKey(id))
                {
                    Results.Add(id, new Dictionary<string, string>());
                }
                int SRindex = SR.Count - 4 ;
                Results[id].Add(SR[SRindex].ToString(), SR["Value"].ToString());

            }

            foreach(var element in Results)
            {
                int i = 0;
                while ( i < element.Value.Count())
                {
                    Console.WriteLine(element.ToString() + element.Value.ElementAt(i));
                    i++;
                }
            }


            List<SparqlResult> SRSList01 = SRS01.Results;

            //foreach (SparqlResult SingleResult in SRSList01)
            //{
                // Log-Output
            //    Console.WriteLine("\nData:\n" + SingleResult["Value"] + "\n" + SingleResult["Instanz01"] + "\n");
            //}

            return SRS01;

        }

        public static SparqlQuery GetIfcElement()
        {

            IGraph temp = new VDS.RDF.Graph();
            temp.NamespaceMap.Import(GetNamespaces());

            VariablePattern element = new VariablePattern("element");


            // building PropertyPath: ifc:globalId_IfcRoot / expr:hasString
            SequencePath first = new SequencePath
                (
                new Property
                (
                    temp.CreateUriNode("ifc:globalId_IfcRoot")
                    ), 
                new Property
                (
                    temp.CreateUriNode("expr:hasString")
                    )
                );


            // building PropertyPath: ifc:name_IfcRoot / expr:hasString
            SequencePath second = new SequencePath
                (
                new Property
                (
                    temp.CreateUriNode("ifc:name_IfcRoot")
                    ),
                new Property
                (
                    temp.CreateUriNode("expr:hasString")
                    )
                );


            // building PropertyPath: ifc:description_IfcRoot / expr:hasString
            SequencePath third = new SequencePath
                (
                new Property
                (
                    temp.CreateUriNode("ifc:description_IfcRoot")
                    ),
                new Property
                (
                    temp.CreateUriNode("expr:hasString")
                    )
                );


            VariablePattern v01 = new VariablePattern("globalId_IfcRoot")/*, globalId = "globalId_IfcRoot", hasString = "hasString"*/;
            VariablePattern v02 = new VariablePattern("name_IfcRoot");
            VariablePattern v03 = new VariablePattern("description_IfcRoot");
            VariablePattern v04 = new VariablePattern("ownerHistory_IfcRoot");

            // -------------- Building Patterns -------------------

            // Triplepattern with { ?element rdf:type ifc:IfcWindow }
            TriplePattern TPstart = new TriplePattern(element, new NodeMatchPattern(temp.CreateUriNode("rdf:type")), new NodeMatchPattern(temp.CreateUriNode("ifc:IfcWindow")));

            // PropertyPath with { ?element ifc:globalId_IfcRoot / expr:hasString ?ifcgloballyuniqueid }
            PropertyPathPattern TP1 = new PropertyPathPattern(element, first, v01);
            
            List <ITriplePattern>  test = new List<ITriplePattern>() 
            { 
                TPstart, 
                TP1 
            };

            ISparqlAlgebra select01 = new Bgp(test);

            // ----------------- Optional Patterns ---------------

            // PropertyPath with { ?element ifc:name_IfcRoot / expr:hasString ?name }
            PropertyPathPattern TP2 = new PropertyPathPattern(element, second, v02);

            LeftJoin selectOptional02 = new LeftJoin(select01, new Bgp(TP2));

            // PropertyPath
            PropertyPathPattern TP3 = new PropertyPathPattern(element, third, v03);

            LeftJoin selectOptional03 = new LeftJoin(selectOptional02, new Bgp(TP3));



            SparqlParameterizedString sp = new SparqlParameterizedString() 
            {
                CommandText = $"SELECT {element} {v01} {v02} {v03} {v04} WHERE " + selectOptional03.ToQuery(),
                Namespaces = GetNamespaces()
            };

            SparqlQueryParser parser = new SparqlQueryParser();

            Console.WriteLine(sp.ToString());
                        
            SparqlQuery IfcElementQueryString = parser.ParseFromString(sp);

            return IfcElementQueryString;
        }

        public static void GetIfcVersion(SparqlRemoteEndpoint SRE)
        {
            var prefixes = new NamespaceMapper(true);
            prefixes.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            prefixes.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            prefixes.AddNamespace("dce", new Uri("http://purl.org/dc/elements/1.1/"));

            var IfcVersion = new SparqlVariable("IfcVersion");
            var IfcVersionQuery = QueryBuilder
                .Select(new SparqlVariable[] { IfcVersion, new SparqlVariable("b"), new SparqlVariable("c") })
                .Where(
                    (triplePatternBuilder) =>
                    {
                        triplePatternBuilder
                            .Subject("x")
                            .PredicateUri("rdf:type")
                            .Object(new Uri("http://www.w3.org/2002/07/owl#Ontology"));
                        triplePatternBuilder
                            .Subject("x")
                            .PredicateUri("dce:identifier")
                            .Object(IfcVersion);
                    });

            IfcVersionQuery.Prefixes = prefixes;

            // Log-Output
            Console.WriteLine(IfcVersionQuery.BuildQuery().ToString());

            SparqlResultSet SRS01 = SRE.QueryWithResultSet(IfcVersionQuery.BuildQuery().ToString());


            List<SparqlResult> SRSList01 = SRS01.Results;

            foreach (SparqlResult SingleResult in SRSList01)
            {
                // Log-Output
                Console.WriteLine("\nIfcVersion: " + SingleResult[0] + "\n");
            }

        }

        public static void GetIfcRoot(SparqlRemoteEndpoint SRE)
        {

            var prefixes = new NamespaceMapper(true);
            prefixes.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            prefixes.AddNamespace("ifc", new Uri("https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD1/OWL#"));
            prefixes.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));

            var Instanz = new SparqlVariable("Instanz");
            var IfcRootQuery = QueryBuilder
                .Select(new SparqlVariable[] { /*Instanz */new SparqlVariable("a"), new SparqlVariable("b"), new SparqlVariable("c") })
                .Where(
                    (triplePatternBuilder) =>
                    {
                        triplePatternBuilder
                            .Subject(Instanz)
                            .PredicateUri("rdf:type")
                            .Object(new Uri("https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD1/OWL#IfcRoot"));
                        triplePatternBuilder
                            .Subject(Instanz)
                            .Predicate("b")
                            .Object("c");
                    //triplePatternBuilder
                    //    .Subject("b")
                    //    .PredicateUri("rdf:type")
                    //    .Object(new Uri("http://www.w3.org/2002/07/owl#ObjectProperty"));
                })
                .Filter((builder) => !builder.IsBlank("c"))
                .Filter((builder) => !builder.IsBlank("a"));

            IfcRootQuery.Prefixes = prefixes;

            // Log-Output
            Console.WriteLine(IfcRootQuery.BuildQuery().ToString());


            SparqlResultSet SRS01 = SRE.QueryWithResultSet(IfcRootQuery.BuildQuery().ToString());

            //SparqlResultSet SRS01 = SRE.QueryWithResultSet("PREFIX ifc: <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD1/OWL#>" +
            //    "SELECT * WHERE {?instanz a ifc:IfcRoot.}");

            List<SparqlResult> SRSList01 = SRS01.Results;

            foreach (SparqlResult SingleResult in SRSList01)
            {
                // Log-Output
                Console.WriteLine(SingleResult/*"\nI: " + SingleResult[0] + "\na: " + SingleResult[1] + "\nb: " + SingleResult[2]*/);
            }
        }
    }
}
