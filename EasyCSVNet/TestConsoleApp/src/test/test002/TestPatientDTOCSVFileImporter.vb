#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports EasyCSVNet.easycsvnet.csv
Imports EasyCSVNet.easycsvnet.csv.import
Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace testconsoleapp
    Namespace test
        Namespace test002

            Public Class TestPatientDTOCSVFileImporter
                Inherits SimpleCSVFileImporter(Of TestPatientDTO)

#Region "Public attributes"

                Public Shared DEFAULT_XML_DEFINITION_FILE_PATH As String = "conf/csvOCM/csvOCM_TestPatientDTO_def.xml"

#End Region 'Public attributes

#Region "Public methods"

#Region "Constructors"

                Public Sub New(ByVal csvFilePath As String, Optional ByVal xmlFilePath As String = Nothing, Optional ByVal xsdSchemaFileRelativePath As String = DEFAULT_XSD_FILE_RELATIVE_PATH, Optional ByVal xsdSchemaTargetNamespace As String = DEFAULT_XSD_TARGET_NAMESPACE)

                    ' Provide a default value for XML definition file path
                    MyBase.New(csvFilePath, Utils.coalesce(xmlFilePath, DEFAULT_XML_DEFINITION_FILE_PATH), xsdSchemaFileRelativePath, xsdSchemaTargetNamespace)

                    ' Override some default CSV field to type mapping actions by setting a customized CSVFieldTypeMapperEngine instance:
                    ' Boolean and Boolean? types -> Should map 'Yes' literal to True
                    Dim csvFieldTypeMapperEngine As New CSVFieldTypeMapperEngine()
                    Dim csvFieldTypeMapper_Boolean As CSVFieldTypeMapperEngine.GenericDefaultCSVFieldTypeMapper = New CSVFieldTypeMapperEngine.DefaultCSVFieldTypeMapper_Boolean(New List(Of String)({"Yes"}))
                    Dim csvFieldTypeMapper_NullableBoolean As CSVFieldTypeMapperEngine.GenericDefaultCSVFieldTypeMapper = New CSVFieldTypeMapperEngine.DefaultCSVFieldTypeMapper_NullableWrapper(Of Boolean)(csvFieldTypeMapper_Boolean)
                    ' DateTime and DateTime? types -> Should take 'yyyy-MM-dd' format pattern for dates
                    Dim csvFieldTypeMapper_DateTime As CSVFieldTypeMapperEngine.GenericDefaultCSVFieldTypeMapper = New CSVFieldTypeMapperEngine.DefaultCSVFieldTypeMapper_DateTime("yyyy-MM-dd")
                    Dim csvFieldTypeMapper_NullableDateTime As CSVFieldTypeMapperEngine.GenericDefaultCSVFieldTypeMapper = New CSVFieldTypeMapperEngine.DefaultCSVFieldTypeMapper_NullableWrapper(Of DateTime)(csvFieldTypeMapper_DateTime)
                    With Me.csvFieldTypeMapperEngine
                        .registerTypeMapper(Of Boolean)(csvFieldTypeMapper_Boolean)
                        .registerTypeMapper(Of Boolean?)(csvFieldTypeMapper_NullableBoolean)
                        .registerTypeMapper(Of DateTime)(csvFieldTypeMapper_DateTime)
                        .registerTypeMapper(Of DateTime?)(csvFieldTypeMapper_NullableDateTime)
                    End With

                End Sub

#End Region 'Constructors

#Region "Overriden methods from SimpleCSVFileImporter"

                ' Custom restriction in default CSV line acceptance behavior
                Public Overrides Function lineBeforeParseVisitor_hookCallBack(ByVal numLine As ULong, ByRef line As String, ByVal numField As UShort, ByVal expectedNumFields As UShort, ByRef fieldArray As String()) As Boolean
                    Const COMMENT_LINE_START_CHARACTER As Char = "#"c
                    ' Skip comment lines starting with '#'
                    Return Not line.StartsWith(COMMENT_LINE_START_CHARACTER)
                End Function

                ' Custom restriction in default CSV field parsing
                Public Overrides Function fieldAfterParseVisitor_hookCallBack(ByVal numLine As ULong, ByVal numFields As UShort, ByVal fieldArray As String(), ByVal csvField As SimpleCSVField, ByVal fieldStrValue As String, ByRef obj As TestPatientDTO) As Boolean
                    Select Case csvField.mappedAttributeName
                        Case "_birthDate"
                            ' Calculate present age just after having parsed field 'birthDate'
                            obj.calculatePresentAgeInYears()
                    End Select
                    Return True ' No lines skipped
                End Function

                ' Custom restriction in default CSV line acceptance behavior
                Public Overrides Function lineAfterParseVisitor_hookCallBack(ByVal numLine As ULong, ByVal numField As UShort, ByVal fieldArray As String(), ByRef obj As TestPatientDTO) As Boolean
                    With obj
                        ' Skip lines with TestPatientDTO.acceptFlag <> True
                        Return (Not IsNothing(.acceptFlag)) AndAlso .acceptFlag.HasValue AndAlso .acceptFlag.Value
                    End With
                End Function

#End Region 'Overriden methods from SimpleCSVFileImporter

#End Region 'Public methods

            End Class

        End Namespace
    End Namespace
End Namespace