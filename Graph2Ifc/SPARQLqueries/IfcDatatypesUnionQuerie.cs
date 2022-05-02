using System;
using System.Collections.Generic;
using System.Text;

namespace Graph2Ifc.SPARQLqueries
{
    class IfcDatatypesUnionQuerie
    {

        public static string gesamtabfrage(string ifcclass)
        {
            string querie = "SELECT ?element ?attribute ?value" +
            " WHERE {" +
            "    VALUES (?ifcentity) {(" + ifcclass + ")}" +
            "	?element rdf:type / rdfs:subClassOf* ?ifcentity  ." +
            "	?element ?attribute ?datatype ." +
            "    {" +
            "        ?datatype rdf:type owl:NamedIndividual ." +
            "        ?datatype rdfs:label ?value . "+
            "    }" +
            "    UNION" +
            "    {" +
            "        ?datatype expr:hasInteger | expr:hasDouble | expr:hasString | expr:hasHexBinary | expr:hasBoolean | expr:hasLogical / rdfs:label  ?rawvalue ." +
            "        ?datatype rdf:type ?type ." +
            "        BIND (STRDT(STR(?rawvalue), ?type) AS ?value)" +
            "    }" +
            "    UNION" +
            "    {" +
            "        {SELECT ?element ?attribute (CONCAT(\"(\",?rawvalue,\")\") AS ?value) WHERE {{{" +
            //List from Lists e.g. IfcCartesianPointList
            //LIST [n:m] OF LIST [o:p] OF type;
            "            SELECT ?element ?attribute (GROUP_CONCAT(?values; separator=\"),(\") AS ?rawvalue) " +
            "            WHERE {{" +
            "                    SELECT ?element ?attribute ?content (GROUP_CONCAT(?valuess; separator=\", \") AS ?values) WHERE {" +
            "                        ?element rdf:type / rdfs:subClassOf* ?ifcentity  ." +
            "                        ?element ?attribute ?datatype ." +
            "                        ?datatype rdf:type / rdfs:subClassOf*  list:OWLList ." +
            "                        ?datatype list:hasNext* / list:hasContents ?content ." +
            "                        ?content list:hasNext* / list:hasContents / (expr:hasInteger | expr:hasDouble | expr:hasString | expr:hasHexBinary | expr:hasBoolean) ?valuess ." +
            "            } GROUP BY ?element ?attribute ?content}}GROUP BY ?element ?attribute" +
            "            }}" +
            "        UNION" +
            "        {" +
            //LIST [n:m] OF type
            "            SELECT ?element ?attribute  (GROUP_CONCAT(?values; separator=\", \") AS ?rawvalue) WHERE {" +
            "            ?element rdf:type / rdfs:subClassOf* ?ifcentity  ." +
            "            ?element ?attribute ?datatype ." +
            "                        ?datatype rdf:type / rdfs:subClassOf*  list:OWLList ." +
            "                        ?datatype list:hasNext* / list:hasContents / (expr:hasInteger | expr:hasDouble | expr:hasString | expr:hasHexBinary | expr:hasBoolean) ?values ." +
            "            } GROUP BY ?element ?attribute" +
            "        }" +
            "        UNION " +
            "        {" +
            //LIST [n:m] OF entities
            " SELECT ?element ?attribute  (GROUP_CONCAT(?values; separator=\", \") AS ?rawvalue) WHERE {" +
            "                        ?element rdf:type / rdfs:subClassOf* ?ifcentity  ." +
            "                        ?element ?attribute ?datatype ." +
            "                        ?datatype rdf:type / rdfs:subClassOf*  list:OWLList ." +
            "                        ?datatype list:hasNext* / list:hasContents ?values ." +
            "                        ?values rdf:type / rdfs:subClassOf* ifcext:IfcEntities  ." +
            "            } GROUP BY ?element ?attribute" +
            "        }}}" +
            "    }" +
            "    UNION" +
            "    {" +
            "        ?datatype rdf:type / rdfs:subClassOf* ifcext:IfcEntities ." +
            "        BIND (?datatype AS ?value)" +
            "    }}";

            return querie;
        }
 
    }
}
