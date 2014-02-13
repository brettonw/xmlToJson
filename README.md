xmlToJson
=========

Dist\xmlToJson is a simple command line utility for Windows, written in C#, to convert an XML file to JSON. I use it in a Cygwin shell to bulk process XML files produced by other tools into a format that can be piped into Elastic Search. There is no error handling to speak of.

    Usage: xmlToJson <inputFileName.xml>
    Usage: xmlToJson <inputFileName1.xml>  <inputFileName2.xml>  <inputFileName3.xml> ...
    Usage: xmlToJson *.xml

The output is written to &lt;inputFileName.json&gt; for each input file.

PLEASE NOTE: In converting XML to JSON, the author has to make a decision about how to handle attributes on the XML nodes. We chose to create a child element in the JSON called "_attributes". In cases where the XML element contains attributes, but no other children than the innerText, we chose to create a new child element called "value", which contains the innerText result.
