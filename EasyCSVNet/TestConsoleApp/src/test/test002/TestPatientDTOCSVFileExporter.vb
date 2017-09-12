#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports EasyCSVNet.easycsvnet.csv.export

#End Region 'Imports

Namespace testconsoleapp
    Namespace test
        Namespace test002

            Public Class TestPatientDTOCSVFileExporter
                Inherits SimpleCSVFileExporter(Of TestPatientDTO)

#Region "Public methods"

#Region "Constructors"

                Public Sub New(ByVal filePath As String, Optional ByVal delimiterStr As String = DEFAULT_DELIMITER_STRING)
                    MyBase.New(filePath, delimiterStr)
                End Sub

                Public Sub New(ByVal fileWriter As System.IO.StreamWriter, Optional ByVal delimiterStr As String = DEFAULT_DELIMITER_STRING)
                    MyBase.New(fileWriter, delimiterStr)
                End Sub

#End Region 'Constructors

#Region "Overriden methods from SimpleCSVFileExporter"

                ' Custom restriction in default CSV file creation, at the beginning of the file
                Public Overrides Function fileBeforeExportVisitor_hookCallBack(ByRef dtoList As IEnumerable(Of TestPatientDTO), fileWriter As IO.StreamWriter) As Boolean
                    ' Insert a first non-data comment line
                    fileWriter.WriteLine("# Test 2 - Example : CSV file with a list of patients exported")
                    Return True
                End Function

#End Region 'Overriden methods from SimpleCSVFileExporter

#End Region 'Public methods

            End Class

        End Namespace
    End Namespace
End Namespace
