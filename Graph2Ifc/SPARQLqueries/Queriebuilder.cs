using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using System.Net.Http;
using System.Threading.Tasks;

namespace Graph2Ifc.SPARQLqueries
{
    class Queriebuilder
    {
        public static Uri endpoint { get; set; }
      


        public static IGraph temp = new VDS.RDF.Graph();

        private static readonly HttpClient client = new HttpClient();


        //public static SparqlRemoteEndpoint SRE = new SparqlRemoteEndpoint(new Uri("http://localhost:7200/repositories/BG_4"));

        public static List<ITriplePattern> patternlist = new List<ITriplePattern>();
        
        public static VariablePattern element = new VariablePattern("element");

        public static NamespaceMapper NM = new NamespaceMapper();



        //SparqlQueryParser parser = new SparqlQueryParser();

        private static async Task<string> GetNamespacesAsync(string prefix)
        {
            var reqnamespace = await client.GetStringAsync(endpoint + "/namespaces/" + prefix);
            return reqnamespace;
        }

        public static void SetNamespaces()
        {
            
            // Konstant:
            NM.AddNamespace("expr", new Uri("https://w3id.org/express#"));
            NM.AddNamespace("list", new Uri("https://w3id.org/list#"));
            NM.AddNamespace("onto", new Uri("http://www.ontotext.com/"));
            NM.AddNamespace("dce", new Uri("http://purl.org/dc/elements/1.1/"));
            NM.AddNamespace("vann", new Uri("http://purl.org/vocab/vann/"));
            NM.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            NM.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            NM.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));

            var ifcnamespacetask = GetNamespacesAsync("ifc");
            string ifcnamespace = ifcnamespacetask.Result;

            NM.AddNamespace("ifc", new Uri(ifcnamespace));

            // IFC4_ADD1:
            //NM.AddNamespace("ifc", new Uri("https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD1/OWL#"));
            // IFC2x3:
            //NM.AddNamespace("ifc", new Uri("https://standards.buildingsmart.org/IFC/DEV/IFC2x3/TC1/OWL#"));

            var instnamespacetask = GetNamespacesAsync("inst");
            string instnamespace = instnamespacetask.Result;

            NM.AddNamespace("inst", new Uri(instnamespace));

            // resource A20_FZK-Haus:
            //NM.AddNamespace("inst", new Uri("http://linkedbuildingdata.net/ifc/resources20220103_124941/"));
            // resource BG_4:
            //NM.AddNamespace("inst", new Uri("http://linkedbuildingdata.net/ifc/resources20220404_111257/"));

            // ifcext:
            NM.AddNamespace("ifcext", new Uri("http://www.semanticweb.org/aaron/ontologies/2022/1/ifcextension#"));
        }

        public static SparqlQuery queriemaingraph(string ifcclass)
        {
            temp.NamespaceMap.Import(NM);

            VariablePattern element = new VariablePattern("element");
            VariablePattern attribute = new VariablePattern("attribute");
            VariablePattern value = new VariablePattern("value");
            VariablePattern start = new VariablePattern("start");


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
            VariablePattern c1 = new VariablePattern("c1");


            SparqlVariable[] subSVlist = new SparqlVariable[]{
                new SparqlVariable(element.VariableName),
                new SparqlVariable(attribute.VariableName),
                new SparqlVariable(b1.VariableName)
                };

            UriNode type = (UriNode)temp.CreateUriNode("rdf:type");
            UriNode subClassOf = (UriNode)temp.CreateUriNode("rdfs:subClassOf");
            UriNode ifcclassnode = (UriNode)temp.CreateUriNode("ifc:" + ifcclass);
            UriNode hasnext = (UriNode)temp.CreateUriNode("list:hasNext");
            UriNode hascontents = (UriNode)temp.CreateUriNode("list:hasContents");
            UriNode owllist = (UriNode)temp.CreateUriNode("list:OWLList");

            SparqlQuery sq =
                QueryBuilder
                .Select(subSVlist)
                .Distinct()
                .Where(
                    new PropertyPathPattern(
                        // Subject:
                        element,
                        // Predicate: rdf:type / rdfs:subClassOf*
                        new SequencePath(
                            new Property(type),
                            new ZeroOrMore(
                                new Property(subClassOf))
                            ),
                        // Object:
                        new NodeMatchPattern(ifcclassnode)
                        ),
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
                .Bind(b => b.If(b.IsLiteral(b.Variable(a3.VariableName))).Then(b.StrDt(b.Str(b.Variable(a3.VariableName)),b.Variable(a4.VariableName))).Else(b.Variable(a1.VariableName)))
                .As(b1.VariableName)
                .BuildQuery();

            SparqlQuery listquerie =
                QueryBuilder
                .Select(SVlist)
                .Where()
                .Optional(
                    (optionalBuilder) =>
                    {
                        optionalBuilder.Where(
                            new PropertyPathPattern(
                                // Subject:
                                b1,
                                // Predicate: rdf:type / rdfs:subClassOf*
                                new SequencePath(
                                    new Property(type),
                                    new ZeroOrMore(
                                        new Property(subClassOf))
                                    ),
                                // Object:
                                new NodeMatchPattern(owllist)
                                ),
                            new PropertyPathPattern(
                                b1,
                                new SequencePath(
                                    new ZeroOrMore(
                                        new Property(hasnext)),
                                    new Property(hascontents)),
                                c1
                                ));
                    }
                    )
                .Where(
                    new SubQueryPattern(sq)
                    )
                .Bind(b => b.If(b.Bound(b.Variable(c1.VariableName))).Then(b.Variable(c1.VariableName)).Else(b.Variable(b1.VariableName)))
                .As(value.VariableName)
                .BuildQuery();

            // set FROM <http://www.ontotext.com/explicit>
            //NotInFunction notInFunction = new NotInFunction(type, new IEnumerable<VariableTerm>(new VariableTerm(attribute.VariableName), new VariableTerm(a2.VariableName)));
            listquerie.AddDefaultGraph(new Uri("http://www.ontotext.com/explicit"));


            string querie =
            " SELECT ?element ?attribute ?value FROM onto:explicit" +
            " WHERE {" +
            "   {" +
            "       SELECT DISTINCT ?element ?attribute ?c1" +
            "       WHERE { " +
            "           ?element rdf:type / rdfs:subClassOf* ifc:"+ifcclass+" ."+
            "           ?element ?attribute ?a1 ." +
            "           ?a1 ?a2 ?a3 ." +
            "           ?a1 rdf:type ?a4 ." +
            //"           #?attribute rdfs:range ?e1 ." +
            //"           #?a4 rdfs:subClassOf* ?e1 ." +
            "           BIND(IF(ISLITERAL(?a3), STRDT(STR(?a3), ?a4), ?a1) AS ?c1)" +
            "           FILTER(rdf:type NOT IN(?attribute, ?a2))" +
            "       }" +
            "   }" +
            "   OPTIONAL {" +
            "   ?c1 rdf:type / rdfs:subClassOf* list:OWLList." +
            "   FILTER NOT EXISTS {?c1 rdf:type / rdfs:subClassOf* ifc:IfcValue}" +
            "   ?c1 list:hasNext* / list:hasContents ?d1 ." +
            "   }" +
            "   BIND(IF(BOUND(?d1), ?d1, ?c1) AS ?value)" +
            "}";
            SparqlParameterizedString SPS = new SparqlParameterizedString
            {
                Namespaces = NM,
                CommandText = querie,
            };
            //SparqlOptimiser.QueryOptimiser = optimiser;

            Options.QueryOptimisation = false;

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery SQ = parser.ParseFromString(SPS);

            
            ISparqlAlgebra lsdifb = listquerie.ToAlgebra();
            Console.Write(SQ.ToString());

            return SQ;    
        }

        public static SparqlQuery queriesubgraph(string ifcclass)
        {

            temp.NamespaceMap.Import(NM);

            VariablePattern element = new VariablePattern("element");
            VariablePattern attribute = new VariablePattern("attribute");
            VariablePattern value = new VariablePattern("value");
            VariablePattern start = new VariablePattern("start");


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
            VariablePattern c1 = new VariablePattern("c1");


            SparqlVariable[] subSVlist = new SparqlVariable[]{
                new SparqlVariable(element.VariableName),
                new SparqlVariable(attribute.VariableName),
                new SparqlVariable(b1.VariableName)
                };

            UriNode type = (UriNode)temp.CreateUriNode("rdf:type");
            UriNode subClassOf = (UriNode)temp.CreateUriNode("rdfs:subClassOf");
            UriNode ifcclassnode = (UriNode)temp.CreateUriNode("ifc:" + ifcclass);
            UriNode hasnext = (UriNode)temp.CreateUriNode("list:hasNext");
            UriNode hascontents = (UriNode)temp.CreateUriNode("list:hasContents");
            UriNode owllist = (UriNode)temp.CreateUriNode("list:OWLList");

            SparqlQuery sq =
                QueryBuilder
                .Select(subSVlist)
                .Distinct()
                .Where(
                    new PropertyPathPattern(
                        // Subject:
                        element,
                        // Predicate: rdf:type / rdfs:subClassOf*
                        new SequencePath(
                            new Property(type),
                            new ZeroOrMore(
                                new Property(subClassOf))
                            ),
                        // Object:
                        new NodeMatchPattern(ifcclassnode)
                        ),
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
                .As(b1.VariableName)
                .BuildQuery();

            SparqlQuery listquerie =
                QueryBuilder
                .Select(SVlist)
                .Where()
                .Optional(
                    (optionalBuilder) =>
                    {
                        optionalBuilder.Where(
                            new PropertyPathPattern(
                                // Subject:
                                b1,
                                // Predicate: rdf:type / rdfs:subClassOf*
                                new SequencePath(
                                    new Property(type),
                                    new ZeroOrMore(
                                        new Property(subClassOf))
                                    ),
                                // Object:
                                new NodeMatchPattern(owllist)
                                ),
                            new PropertyPathPattern(
                                b1,
                                new SequencePath(
                                    new ZeroOrMore(
                                        new Property(hasnext)),
                                    new Property(hascontents)),
                                c1
                                ));
                    }
                    )
                .Where(
                    new SubQueryPattern(sq)
                    )
                .Bind(b => b.If(b.Bound(b.Variable(c1.VariableName))).Then(b.Variable(c1.VariableName)).Else(b.Variable(b1.VariableName)))
                .As(value.VariableName)
                .BuildQuery();

            // set FROM <http://www.ontotext.com/explicit>
            //NotInFunction notInFunction = new NotInFunction(type, new IEnumerable<VariableTerm>(new VariableTerm(attribute.VariableName), new VariableTerm(a2.VariableName)));
            listquerie.AddDefaultGraph(new Uri("http://www.ontotext.com/explicit"));


            string querie =
            " SELECT ?element ?attribute ?value FROM onto:explicit" +
            " WHERE {" +
            "   {" +
            "       SELECT DISTINCT ?element ?attribute ?c1" +
            "       WHERE { " +
            "           {SELECT ?element WHERE {" +
            "               GRAPH <http://example.org/bsp#>" +
            "               {" +
            "                   ?element ?b1 ?b2 " +
            "               }}}" +
            //"           ?element rdf:type / rdfs:subClassOf* ifc:"+ifcclass+" ."+
            "           ?element ?attribute ?a1 ." +
            "           ?a1 ?a2 ?a3 ." +
            "           ?a1 rdf:type ?a4 ." +
            //"           #?attribute rdfs:range ?e1 ." +
            //"           #?a4 rdfs:subClassOf* ?e1 ." +
            "           BIND(IF(ISLITERAL(?a3), STRDT(STR(?a3), ?a4), ?a1) AS ?c1)" +
            "           FILTER(rdf:type NOT IN(?attribute, ?a2))" +
            "       }" +
            "   }" +
            "   OPTIONAL {" +
            "   ?c1 rdf:type / rdfs:subClassOf* list:OWLList." +
            "   FILTER NOT EXISTS {?c1 rdf:type / rdfs:subClassOf* ifc:IfcValue}" +
            "   ?c1 list:hasNext* / list:hasContents ?d1 ." +
            "   }" +
            "   BIND(IF(BOUND(?d1), ?d1, ?c1) AS ?value)" +
            "}";
            SparqlParameterizedString SPS = new SparqlParameterizedString
            {
                Namespaces = NM,
                CommandText = querie,
            };
            //SparqlOptimiser.QueryOptimiser = optimiser;

            Options.QueryOptimisation = false;

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery SQ = parser.ParseFromString(SPS);


            ISparqlAlgebra lsdifb = listquerie.ToAlgebra();
            Console.Write(SQ.ToString());

            return SQ;
        }

        public static SparqlQuery GetIfcVersion()
        {
            temp.NamespaceMap.Import(NM);

            VariablePattern IfcVersion = new VariablePattern("IfcVersion");

            UriNode type = (UriNode)temp.CreateUriNode("rdf:type");
            UriNode identifier = (UriNode)temp.CreateUriNode("dce:identifier");
            UriNode ontology = (UriNode)temp.CreateUriNode("owl:Ontology");
            UriNode preferrednamespaceprefix = (UriNode)temp.CreateUriNode("vann:preferredNamespacePrefix");

            SparqlQuery IfcVersionQuery = QueryBuilder
                .Select(new SparqlVariable[] { new SparqlVariable(IfcVersion.VariableName)})
                .Where(
                    (triplePatternBuilder) =>
                    {
                        triplePatternBuilder
                            .Subject("x")
                            .PredicateUri(type)
                            .Object(ontology);
                        triplePatternBuilder
                            .Subject("x")
                            .PredicateUri(preferrednamespaceprefix)
                            .ObjectLiteral("ifc");
                        triplePatternBuilder
                            .Subject("x")
                            .PredicateUri(identifier)
                            .Object(IfcVersion);
                    })
                .BuildQuery();

            IfcVersionQuery.NamespaceMap.Import(NM);

            Console.WriteLine(IfcVersionQuery.ToString());

            return IfcVersionQuery;
        }

        public static SparqlResultSet abfrage(SparqlQuery SQ)
        {
            SparqlRemoteEndpoint SRE = new SparqlRemoteEndpoint(endpoint);
            
            SRE.ResultsAcceptHeader = "application/sparql-results+json";
            SparqlResultSet SRS = SRE.QueryWithResultSet(SQ.ToString());
            return SRS;

        }
        
    }


}
