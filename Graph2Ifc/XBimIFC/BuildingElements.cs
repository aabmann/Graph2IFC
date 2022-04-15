using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Globalization;
using VDS.RDF.Query;
using Xbim.Ifc;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Common.Step21;
using Xbim.IO;
using Xbim.Common;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;
using System.Linq;
using Xbim.Ifc4.RepresentationResource;
using System.Reflection;
using Xbim.Ifc4.PresentationDefinitionResource;
using Graph2Ifc.SPARQLqueries;
using Xbim.Ifc4.QuantityResource;
using Xbim.Ifc4.TopologyResource;
using VDS.RDF;

namespace Graph2Ifc.XBimIFC
{
    class BuildingElements
    {
        public const string fileName = "../../../ausgabe_verwinkeltesHaus.ifc";
        public static IFormatProvider provider = CultureInfo.InvariantCulture;

        public static Dictionary<string, IfcBuildingElement> buildingelementslist = new Dictionary<string, IfcBuildingElement>();
        public static Dictionary<string, dynamic> persistentitylist = new Dictionary<string, dynamic>();

        public static (IfcStore, Dictionary<string, dynamic>) GenerateIfcProject(IfcStore model, Dictionary<string, Dictionary<string, List<dynamic>>> results)
        {
            using (model)
            {
                //var ownerhistory = model.OwnerHistoryAddObject;
                using (var txn = model.BeginTransaction("IfcProject"))
                {

                    // get Assembly from Ifc4
                    Assembly assembly = Assembly.Load("Xbim.Ifc4");

                    IEnumerable<Type> allnonabstracttypes = assembly.GetTypes().Where(i => i.IsAbstract == false & i.IsSubclassOf(typeof(PersistEntity)));

                    IEnumerable<string> alltypesname = allnonabstracttypes.Select(a => a.Name);

                    IEnumerable<Type> valuesubtypes = assembly.GetTypes().Where(t => typeof(IIfcValue).IsAssignableFrom(t) && !t.IsInterface);

                    IEnumerable<string> valuesubtypesnames = valuesubtypes.Select(a => a.Name);


                    //IEnumerable<Type> test = assembly.GetTypes().Where(i => i.BaseType == typeof(PersistEntity));

                    foreach (var instanz in results.Where(
                        i =>
                        alltypesname.Contains(
                            i.Key.Substring(
                                i.Key.LastIndexOf("/") + 1,
                                i.Key.LastIndexOf("_") - i.Key.LastIndexOf("/") - 1))
                        ))
                    {
                        string element = instanz.Key;

                        // get string from Instance to identify the IfcType from e.g:  http://linkedbuildingdata.net/ifc/resources20220103_124941/IfcDoor_17468 --> IfcDoor
                        string ifcinstanztype = element.Substring(
                            element.LastIndexOf("/") + 1,
                            element.LastIndexOf("_") - element.LastIndexOf("/") - 1);

                        Type singletype = allnonabstracttypes.First(t => t.Name == ifcinstanztype);

                        dynamic ifcentity = model.Instances.New(singletype);

                        persistentitylist.Add(element, ifcentity);
                    }

                    foreach (var instanz in results.Where(
                        i =>
                        alltypesname.Contains(
                            i.Key.Substring(
                                i.Key.LastIndexOf("/") + 1,
                                i.Key.LastIndexOf("_") - i.Key.LastIndexOf("/") - 1))
                        )
                    )
                    {
                        string element = instanz.Key; // z.B. inst:IfcProject_66

                        // get string from Instance to identify the IfcType from e.g:  http://linkedbuildingdata.net/ifc/resources20220103_124941/IfcDoor_17468 --> IfcDoor
                        string ifcinstanztype = element.Substring(
                            element.LastIndexOf("/") + 1,
                            element.LastIndexOf("_") - element.LastIndexOf("/") - 1);

                        // get the Type from the subtypes of IfcRoot which name is identical to the Type-Substring of the Instance
                        Type singletype = allnonabstracttypes.First(t => t.Name == ifcinstanztype);

                        //(Type, KeyValuePair<string, Dictionary<string, List<string>>>) ifcetemp = (singletype, instanz);

                        // access the relevent instance via the entitylabel (compare between persistentitylist and Model.Instances(.OfType))
                        dynamic ifcentity = model.Instances.OfType(singletype.Name, true).Where(i => i.EntityLabel == persistentitylist[element].EntityLabel).First();

                        Dictionary<string, List<dynamic>> attributeandvalues = instanz.Value;

                        foreach (string attribute in attributeandvalues.Keys.Where(
                            k =>
                            singletype.GetProperties().Select(p => p.Name.ToLower()).Contains(k.ToLower())
                            //&& attributeandvalues[k].All(i => i.GetType() == typeof(LiteralNode))
                            ))
                        {
                            List<dynamic> values = attributeandvalues[attribute];

                            // allUri is used to set the connection to the other IfcEntitys (#12, #64, etc)
                            // if the persistentitylist doesnt have an instance of the value its not possible to setup an "connection"
                            // 
                            bool allUri = values.All(k => k.GetType() == typeof(UriNode));
                            if (allUri)
                            {
                                if (!persistentitylist.Keys.Intersect(values.Select(v => v.ToString())).Any())
                                {
                                    continue; // the IfcEntity of the value doesnt exist --> check next attribute
                                }
                            }
                            string testing = singletype.Name;
                            //var Nodetype = values.Select(k => k.GetType());
                            var prop = singletype.GetProperties().Where(p => p.Name.ToLower() == attribute.ToLower()).First();

                            Type rtype = prop.PropertyType.IsGenericType ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;

                            // allLiteral is used to set the simple values like IfcLabel, IfcGloballyUniqueId
                            bool allLiteral = values.All(k => k.GetType() == typeof(LiteralNode));
                            if (allLiteral)
                            {
                                if (rtype.IsEnum)
                                {
                                    prop.SetValue(ifcentity, Enum.Parse(rtype, attributeandvalues[attribute].FirstOrDefault().Value));
                                }
                                else if (rtype.IsSubclassOf(typeof(ValueType)) && rtype != typeof(double) /*temporaer zum testen:*/&& rtype.Name != "IfcCompoundPlaneAngleMeasure")
                                {
                                    var n = Activator.CreateInstance(rtype, attributeandvalues[attribute].FirstOrDefault().Value);

                                    prop.SetValue(ifcentity, n);
                                }
                                else if (rtype == typeof(double))
                                {
                                    prop.SetValue(ifcentity, Double.Parse(attributeandvalues[attribute].FirstOrDefault().Value, provider));
                                }
                                else if (rtype == typeof(IfcValue))
                                {
                                    //string valuetypestring = key.Substring(key.IndexOf("|") + 2);
                                    Uri valuetypeuri = attributeandvalues[attribute].FirstOrDefault().DataType;
                                    string valuetypestring = valuetypeuri.Fragment.TrimStart('#');
                                    Type valuetype = valuesubtypes.Where(t => t.Name == valuetypestring).FirstOrDefault();
                                    // ttl Konvertierungsfehler
                                    if (valuetypestring == typeof(IfcGloballyUniqueId).Name) { valuetype = typeof(IfcLabel); }
                                    else if (valuetypestring == "INTEGER") { valuetype = typeof(IfcInteger); }

                                    object value = Activator.CreateInstance(valuetype, attributeandvalues[attribute].FirstOrDefault().Value);
                                    prop.SetValue(ifcentity, value);

                                }

                            }

                            if (allUri)
                            {
                                if (rtype == typeof(IfcProperty) | rtype == typeof(IfcPhysicalQuantity))
                                {
                                    prop.GetValue(ifcentity).Add(persistentitylist[values.FirstOrDefault().ToString()]);
                                }
                                else if (typeof(IPersistEntity).IsAssignableFrom(prop.PropertyType))
                                {
                                    prop.SetValue(ifcentity, persistentitylist[values.FirstOrDefault().ToString()]);
                                }
                                else if (typeof(IItemSet).IsAssignableFrom(prop.PropertyType))
                                {
                                    foreach (string value in values.Select(v => v.ToString()))
                                    {
                                        prop.GetValue(ifcentity).Add(persistentitylist[value]);
                                    }

                                }

                                if (prop.CustomAttributes.Select(w => w.AttributeType).Contains(typeof(InverseProperty)))
                                {

                                    // get property name of relevent IfcRelationship class. e.g. RelatedObject
                                    string invpropname = prop.CustomAttributes.Where(w => w.AttributeType == typeof(InverseProperty)).FirstOrDefault().ConstructorArguments.FirstOrDefault().Value.ToString();
                                    // get prop from relevant IfcRelationship class
                                    PropertyInfo invprop = rtype.GetProperties().Where(r => r.Name == invpropname.ToString() && r.DeclaringType == rtype).FirstOrDefault();
                                    
                                    dynamic singleentity = null;

                                    dynamic ifcrelentity = model.Instances.New(rtype);

                                    if (invprop.GetSetMethod() != null)
                                    {
                                        invprop.SetValue(ifcrelentity, persistentitylist[element]);
                                    }
                                    else if (invprop.PropertyType.Name == "IItemSet`1")
                                    {
                                        invprop.GetValue(ifcrelentity).Add(persistentitylist[element]);
                                    }

                                    // get PropertyInfo of Property for value
                                    PropertyInfo valueprop = rtype.GetProperties().Where(f => f.Name != invpropname && f.DeclaringType == rtype).FirstOrDefault();


                                    if (/*single value in Ifc: */valueprop.GetSetMethod() != null && /* Single Value in the attributeandvalues */values.Count == 1)
                                    {
                                        if (persistentitylist.TryGetValue(values.FirstOrDefault().ToString(), out singleentity))
                                        {
                                            valueprop.SetValue(ifcrelentity, persistentitylist[values.FirstOrDefault().ToString()]); 
                                        }

                                        addentity(ifcrelentity.GetType().Name + "_" + ifcrelentity.EntityLabel.ToString(), ifcrelentity);
                                    }
                                    else if (/* Value List in Ifc: */valueprop.PropertyType.Name == "IItemSet`1")
                                    {
                                        foreach (string value in values.Select(v => v.ToString()))
                                        {
                                            if(persistentitylist.TryGetValue(value, out singleentity))
                                            {
                                                valueprop.GetValue(ifcrelentity).Add(singleentity);
                                            }
                                        }

                                        //values.ForEach(
                                        //    v =>

                                        //    valueprop.GetValue(ifcrelentity).Add(persistentitylist[v.ToString()])
                                        //    );

                                        addentity(ifcrelentity.GetType().Name + "_" + ifcrelentity.EntityLabel.ToString(), ifcrelentity);
                                    }
                                    else if (/*single value in Ifc: */valueprop.GetSetMethod() != null && /* but multiple input attributeandvalues */values.Count > 1)
                                    {
                                        valueprop.SetValue(ifcrelentity, persistentitylist[values.First().ToString()]);

                                        addentity(ifcrelentity.GetType().Name + "_" + ifcrelentity.EntityLabel.ToString(), ifcrelentity);

                                        foreach (INode singlevalue in values.Skip(1))
                                        {
                                            ifcrelentity = model.Instances.New(rtype);

                                            if (invprop.GetSetMethod() != null)
                                            {
                                                invprop.SetValue(ifcrelentity, persistentitylist[element]);
                                            }
                                            else if (invprop.PropertyType.Name == "IItemSet`1")
                                            {
                                                invprop.GetValue(ifcrelentity).Add(persistentitylist[element]);
                                            }

                                            valueprop.SetValue(ifcrelentity, persistentitylist[singlevalue.ToString()]);

                                            addentity(ifcrelentity.GetType().Name + "_" + ifcrelentity.EntityLabel.ToString(), ifcrelentity);
                                        }
                                    }

                                }


                            }
                            //Console.Write("XXX");

                            //Type rtype = prop.PropertyType.IsGenericType ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;

                            //if (rtype.IsEnum)
                            //{
                            //    prop.SetValue(ifcentity, Enum.Parse(rtype, attributeandvalues[key].FirstOrDefault().Value));
                            //}
                            //else if (rtype.IsSubclassOf(typeof(ValueType)) && rtype.Name != "IfcCompoundPlaneAngleMeasure")
                            //{
                            //    var n = Activator.CreateInstance(rtype, attributeandvalues[key].FirstOrDefault().Value);
                            //    prop.SetValue(ifcentity, n);
                            //}

                        }

                    }

                    txn.Commit();
                }
                model.SaveAs(fileName);
                return (model, persistentitylist);
            }
        }

        public static void addentity(string ifcinstanz, dynamic ifcentity)
        {
            try
            {
                if (persistentitylist.ContainsKey(ifcinstanz))
                {
                    persistentitylist[ifcinstanz] = ifcentity;
                }
                else
                {
                    persistentitylist.Add(ifcinstanz, ifcentity);
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("An element with Key = " + ifcinstanz + " already exists");
            }
        }


    }
}