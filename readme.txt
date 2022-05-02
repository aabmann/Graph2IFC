Informationen zur ausführung

{Pfad zur Exe} {Repository URL} {Pfad zur Outputdatei ".../output.ifc"}
Beispiel: "A:\...\bin\Debug\netcoreapp3.1\Graph2Ifc_cmd.exe" "http://localhost:7200/repositories/verwinkeltesHaus_clean" "A:/verwinkeltesHaus_ausgabe.ifc"

Folgende SPARQL Construct befehle fuer Abkuerzungen 
muessen manuell eingegeben werden:
	ifcext:IfcEntites (IFC4 und IFC2X3)
	Koordinaten für IfcCartesianPoint:
		ifcext:X_IfcCartesianPoint
		ifcext:Y_IfcCartesianPoint
		ifcext:Z_IfcCartesianPoint
	für IfcDirection:
		ifcext:X_IfcDirection
		ifcext:Y_IfcDirection
		ifcext:Z_IfcDirection