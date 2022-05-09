Informationen zur ausführung

{Pfad zur Exe} {Repository URL} {Pfad zur Outputdatei ".../output.ifc"}
Beispiel: "A:\...\Graph2Ifc_cmd\bin\Debug\netcoreapp3.1\Graph2Ifc_cmd.exe" "http://localhost:7200/repositories/verwinkeltesHaus_clean" "A:/verwinkeltesHaus_ausgabe.ifc"

Folgende SPARQL Construct befehle für die Abkürzungen müssen 
manuell ausgeführt werden (Die Befehle sind im SPARQL Ordner):
	ifcext:IfcEntites 
		IFC4 -  Ifc4Entities.rq 
		oder IFC2X3 - Ifc2x3Entities.rq 
	Koordinaten für IfcCartesianPoint - IfcCartesianPoint.rq:	
		ifcext:X_IfcCartesianPoint
		ifcext:Y_IfcCartesianPoint
		ifcext:Z_IfcCartesianPoint
	für IfcDirection -  IfcDirection.rq:
		ifcext:X_IfcDirection
		ifcext:Y_IfcDirection
		ifcext:Z_IfcDirection
