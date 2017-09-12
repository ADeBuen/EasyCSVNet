#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.IO
Imports System.Xml
Imports System.Xml.XPath

Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace easycsvnet
    Namespace csv
        Namespace import

            Public Class SimpleCSVFileImporter(Of T) : Inherits GenericXMLParser(Of T) : Implements IDisposable

#Region "Public constants"

                Public Const INLINE_XSD As Boolean = True
                Public Const CSV_OCM_PATH As String = "conf/csvOCM"

#End Region 'Public constants

#Region "Protected constants"

                Protected Const DEFAULT_XSD_FILE_RELATIVE_PATH As String = "schema/csvOCM_SimpleCSVFileImporter_schema.xsd"
                Protected Const DEFAULT_XSD_TARGET_NAMESPACE As String = "urn:csv"

                Protected Const QUOTATION_CHAR_LIST As String = """,'"

                Protected Const XPATH_AUTHOR_QUERY As String = "//metadata//author[1]"
                Protected Const XPATH_VERSION_QUERY As String = "//metadata//version[1]"
                Protected Const XPATH_REVISION_QUERY As String = "//metadata//revision[1]"
                Protected Const XPATH_MAPPED_OBJECT_FQDN_QUERY As String = "//metadata//mappedobjectfqdn[1]"
                Protected Const XPATH_DESCRIPTION_QUERY As String = "//metadata//description[1]"
                Protected Const XPATH_TITLES_QUERY As String = "//data//titles[1]"
                Protected Const XPATH_DELIMITERS_QUERY As String = "//data//delimiter"
                Protected Const XPATH_STRICT_TYPECAST_QUERY As String = "//data//stricttypecast[1]"
                Protected Const XPATH_ENCODING_QUERY As String = "//data//encoding[1]"
                Protected Const XPATH_FIELD_ID_QUERY As String = "//field[@id]//@id"
                Protected Const XPATH_FIELD_POSITION_QUERY As String = "//field[@id='{0}']//@position"
                Protected Const XPATH_FIELD_NILLABLE_QUERY As String = "//field[@id='{0}']//@nillable"
                Protected Const XPATH_FIELD_QUOTED_QUERY As String = "//field[@id='{0}']//@quoted"
                Protected Const XPATH_FIELD_DESCRIPTION_QUERY As String = "//field[@id='{0}']//@description"
                Protected Const XPATH_FIELD_MAPPED_ATTRIBUTENAME_QUERY As String = "//map[@field='{0}']//@attribute"
                Protected Const XPATH_FIELD_MAPPED_DESCRIPTION_QUERY As String = "//map[@field='{0}']//@description"

#End Region 'Protected constants

#Region "Protected attributes"

                Protected Shared DEFAULT_CSV_ENCODING As System.Text.Encoding = System.Text.Encoding.Default

                Protected _csvFileReader As System.IO.StreamReader = Nothing
                Protected _csvFieldMapperEngine As CSVFieldTypeMapperEngine = Nothing
                Protected _xsdSchemaTempFilePath As String = Nothing

#End Region 'Protected attributes

#Region "Private attributes"

                Private _csv_Author As String = Nothing
                Private _csv_Version As String = Nothing
                Private _csv_Revision As Date = Nothing
                Private _csv_MappedObjectFQDN As String = Nothing
                Private _csv_Description As String = Nothing
                Private _csv_Titles As Boolean = Nothing
                Private _csv_Delimiters As IList(Of Char) = Nothing
                Private _csv_StrictTypeCast As Boolean = Nothing
                Private _csv_Encoding As System.Text.Encoding = Nothing
                Private _csv_Fields As IList(Of SimpleCSVField)

#End Region 'Private attributes

#Region "Public methods"

#Region "Constructors"

                Public Sub New(ByVal csvFilePath As String, ByVal xmlFilePath As String, Optional ByVal xsdSchemaFileRelativePath As String = DEFAULT_XSD_FILE_RELATIVE_PATH, Optional ByVal xsdSchemaTargetNamespace As String = DEFAULT_XSD_TARGET_NAMESPACE)
                    If Utils.isNullOrEmptyString(csvFilePath) Then
                        Throw New ArgumentException("Null or empty CSV file name passed")
                    ElseIf Not (File.Exists(csvFilePath)) Then
                        Throw New ArgumentException(String.Format("CSV file '{0}' not found", csvFilePath))
                    Else
                        Me._csvFieldMapperEngine = New CSVFieldTypeMapperEngine()
                        Dim xsdSchemaFilePath As String = Nothing
                        If INLINE_XSD Then
                            Me._xsdSchemaTempFilePath = Path.GetTempFileName()
                            File.WriteAllText(Me._xsdSchemaTempFilePath, CSVOCMSimpleCSVFileImporterXSDSchema.XML)
                            xsdSchemaFilePath = Me._xsdSchemaTempFilePath
                        Else
                            xsdSchemaFilePath = Utils.formPath(CSV_OCM_PATH, xsdSchemaFileRelativePath)
                        End If
                        Me.importXMLTemplate(xmlFilePath, Nothing, xsdSchemaFilePath, xsdSchemaTargetNamespace)
                        Me._csvFileReader = New StreamReader(File.OpenRead(csvFilePath), Me._csv_Encoding)
                    End If
                End Sub

#End Region 'Constructors

#Region "Properties (getters & setters)"

                Public ReadOnly Property csv_Author As String
                    Get
                        Return Me._csv_Author
                    End Get
                End Property

                Public ReadOnly Property csv_Version As String
                    Get
                        Return Me._csv_Version
                    End Get
                End Property

                Public ReadOnly Property csv_Revision As Date
                    Get
                        Return Me._csv_Revision
                    End Get
                End Property

                Public ReadOnly Property csv_MappedObjectFQDN As String
                    Get
                        Return Me._csv_MappedObjectFQDN
                    End Get
                End Property

                Public ReadOnly Property csv_Description As String
                    Get
                        Return Me._csv_Description
                    End Get
                End Property

                Public Property csvFieldTypeMapperEngine As CSVFieldTypeMapperEngine
                    Get
                        Return Me._csvFieldMapperEngine
                    End Get
                    Set(value As CSVFieldTypeMapperEngine)
                        Me._csvFieldMapperEngine = value
                    End Set
                End Property

#End Region 'Properties (getters & setters)

                '@Overridable
                '@Return Boolean {True = Accept line | False = Skip line}
                Public Overridable Function lineBeforeParseVisitor_hookCallBack(ByVal numLine As ULong, ByRef line As String, ByVal numField As UShort, ByVal expectedNumFields As UShort, ByRef fieldArray As String()) As Boolean
                    Return True
                End Function

                '@Overridable
                '@Return Boolean {True = Accept line | False = Skip line}
                Public Overridable Function lineAfterParseVisitor_hookCallBack(ByVal numLine As ULong, ByVal numField As UShort, ByVal fieldArray As String(), ByRef dto As T) As Boolean
                    Return True
                End Function

                '@Overridable
                '@Return Boolean {True = Accept field | False = Skip field}
                Public Overridable Function fieldBeforeParseVisitor_hookCallBack(ByVal numLine As ULong, ByVal numField As UShort, ByVal fieldArray As String(), ByVal csvField As SimpleCSVField, ByRef fieldStrValue As String) As Boolean
                    Return True
                End Function

                '@Overridable
                '@Return Boolean {True = Accept field | False = Skip field}
                Public Overridable Function fieldAfterParseVisitor_hookCallBack(ByVal numLine As ULong, ByVal numField As UShort, ByVal fieldArray As String(), ByVal csvField As SimpleCSVField, ByVal fieldStrValue As String, ByRef dto As T) As Boolean
                    Return True
                End Function

                Public Function parseCSVData() As IList(Of T)
                    Dim numLine As ULong = Nothing
                    Dim numField As UShort = Nothing
                    Dim mappedAttributeName As String = Nothing
                    Dim mappedAttributeType As Type = Nothing
                    Dim fieldStrValue As String = Nothing
                    Try
                        Dim data As List(Of T) = New List(Of T)
                        Me.parseXMLDefinition()
                        numLine = 0UL
                        Do While (Me._csvFileReader.Peek() >= 0)
                            Dim line As String = Me._csvFileReader.ReadLine()
                            If Not ((numLine = 0UL) AndAlso Me._csv_Titles) Then
                                If (IsNothing(Me._csv_Delimiters) OrElse Me._csv_Delimiters.Count < 1) Then Throw New InvalidDataException("Invalid CSV file format :: No delimiter chars specified")
                                Dim iter As IEnumerator(Of Char) = Me._csv_Delimiters.GetEnumerator
                                iter.MoveNext()
                                Dim firstDelimiter As Char = iter.Current
                                While iter.MoveNext
                                    line = line.Replace(iter.Current, firstDelimiter)
                                End While
                                Dim fieldArray As String() = line.Split(firstDelimiter)
                                Dim numFields As UShort = CUShort(fieldArray.Length)
                                Dim acceptLine As Boolean = Me.lineBeforeParseVisitor_hookCallBack(numLine, line, numFields, CUShort(Me._csv_Fields.Count), fieldArray)
                                numFields = CUShort(fieldArray.Length)
                                If (acceptLine) Then
                                    Dim dto As T = Nothing
                                    Dim csvFieldTOParsed As SimpleCSVField = Nothing
                                    Dim objectClassObject As Object = ReflectionUtils.instantiateObject(Of T)()
                                    Dim acceptField As Boolean = True
                                    numField = 0US
                                    For Each csvFieldTO As SimpleCSVField In Me._csv_Fields
                                        If (acceptField AndAlso csvFieldTO.position = numField) Then
                                            If numField >= fieldArray.Count Then Throw New InvalidDataException(String.Format("Invalid CSV file format @line #{0}, field #{1} :: field index exceeds parsed line array upper bound", numLine + 1S, numField + 1S))
                                            fieldStrValue = fieldArray(numField)
                                            acceptField = fieldBeforeParseVisitor_hookCallBack(numLine, numField, fieldArray, csvFieldTO, fieldStrValue)
                                            If (acceptField) Then
                                                csvFieldTOParsed = csvFieldTO
                                                If (csvFieldTO.quoted) Then
                                                    For Each quotationString As String In QUOTATION_CHAR_LIST.Split(CChar(","))
                                                        fieldStrValue = fieldStrValue.Replace(CChar(quotationString), "")
                                                    Next
                                                End If
                                                If (Utils.isNullOrEmptyString(fieldStrValue)) AndAlso Not csvFieldTO.nillable Then Throw New InvalidDataException(String.Format("Invalid CSV file format @line #{0}, field #{1} :: illegal null or empty value for not nillable field", numLine + 1S, numField + 1S))
                                                mappedAttributeName = csvFieldTO.mappedAttributeName
                                                mappedAttributeType = Nothing
                                                ReflectionUtils.initializeObjectProperty(Of T)(objectClassObject, mappedAttributeName, fieldStrValue, Me._csv_StrictTypeCast, Me.csvFieldTypeMapperEngine, mappedAttributeType)
                                                Dim fieldParsedStrValue As String = fieldStrValue
                                                dto = CType(objectClassObject, T)
                                                acceptField = fieldAfterParseVisitor_hookCallBack(numLine, numField, fieldArray, csvFieldTOParsed, fieldParsedStrValue, dto)
                                            End If
                                        End If
                                        numField = CUShort(numField + 1S)
                                    Next
                                    If (acceptField AndAlso Not IsNothing(csvFieldTOParsed)) Then
                                        If (numFields <> Me._csv_Fields.Count) Then
                                            mappedAttributeName = ""
                                            Throw New InvalidDataException(String.Format("Invalid CSV file format @line #{0} :: illegal field number doesn't match DTD XML schema", numLine + 1UL))
                                        End If
                                        acceptLine = Me.lineAfterParseVisitor_hookCallBack(numLine, numFields, fieldArray, dto)
                                        If (acceptLine) Then data.Add(dto)
                                    End If
                                End If
                            End If
                            numLine += 1UL
                        Loop
                        Return data
                    Catch myException As Exception
                        Dim errMessage As String = String.Format("{0} [line number: {1}, field position: {2}, attribute name: '{3}', attribute type: '{4}', field value: '{5}']", Utils.coalesce(myException.Message), numLine + 1UL, numField, mappedAttributeName, mappedAttributeType, Utils.coalesce(fieldStrValue))
                        Throw New Exception(errMessage, myException)
                    Finally
                        Me._csvFileReader.Close()
                    End Try
                End Function

#End Region 'Public methods

#Region "Protected methods"

                Protected Overrides Sub parseXMLDefinition()
                    Const REVISION_DATETIME_FORMAT_PATTERN As String = "yyyy-MM-dd+HH:mm"
                    Try
                        Me._csv_Author = getXMLElementValue(Of String)(XPATH_AUTHOR_QUERY)
                    Catch optionalXmlAttributeNotFoundException As XmlException
                        Me._csv_Author = ""
                    End Try
                    Me._csv_Version = getXMLElementValue(Of String)(XPATH_VERSION_QUERY)
                    Try
                        Me._csv_Revision = getXMLElementValue(Of Date)(XPATH_REVISION_QUERY, True, Nothing, REVISION_DATETIME_FORMAT_PATTERN)
                    Catch optionalXmlAttributeNotFoundException As XmlException
                        Me._csv_Revision = Nothing
                    End Try
                    Me._csv_MappedObjectFQDN = getXMLElementValue(Of String)(XPATH_MAPPED_OBJECT_FQDN_QUERY)
                    Try
                        Me._csv_Description = getXMLElementValue(Of String)(XPATH_DESCRIPTION_QUERY)
                    Catch optionalXmlAttributeNotFoundException As XmlException
                        Me._csv_Description = ""
                    End Try
                    Me._csv_Titles = getXMLElementValue(Of Boolean)(XPATH_TITLES_QUERY)
                    Me._csv_Delimiters = getXMLElementValueList(Of Char)(XPATH_DELIMITERS_QUERY)
                    Me._csv_StrictTypeCast = getXMLElementValue(Of Boolean)(XPATH_STRICT_TYPECAST_QUERY)
                    Try
                        Dim csvEncoding As String = getXMLElementValue(Of String)(XPATH_ENCODING_QUERY)
                        Select Case Utils.coalesce(csvEncoding).Trim()
                            Case "System.Text.Encoding.ASCII"
                                Me._csv_Encoding = System.Text.Encoding.ASCII
                            Case "System.Text.Encoding.BigEndianUnicode"
                                Me._csv_Encoding = System.Text.Encoding.BigEndianUnicode
                            Case "System.Text.Encoding.Default"
                                Me._csv_Encoding = System.Text.Encoding.Default
                            Case "System.Text.Encoding.Unicode"
                                Me._csv_Encoding = System.Text.Encoding.Unicode
                            Case "System.Text.Encoding.UTF7"
                                Me._csv_Encoding = System.Text.Encoding.UTF7
                            Case "System.Text.Encoding.UTF8"
                                Me._csv_Encoding = System.Text.Encoding.UTF8
                            Case "System.Text.Encoding.UTF32"
                                Me._csv_Encoding = System.Text.Encoding.UTF32
                            Case Else
                                Me._csv_Encoding = DEFAULT_CSV_ENCODING
                        End Select
                    Catch optionalXmlAttributeNotFoundException As XmlException
                        Me._csv_Encoding = DEFAULT_CSV_ENCODING
                    End Try
                    Me._csv_Fields = New List(Of SimpleCSVField)
                    Dim fieldIdByPositionDictionary As IDictionary(Of UShort, String) = New SortedDictionary(Of UShort, String)
                    Dim iter As XPathNodeIterator = Me._xPathNavigator.Select(XPATH_FIELD_ID_QUERY)
                    While (iter.MoveNext)
                        Dim fieldId As String = iter.Current.Value
                        Dim position As UShort = getXMLElementValue(Of UShort)(String.Format(XPATH_FIELD_POSITION_QUERY, fieldId))
                        fieldIdByPositionDictionary.Add(position, fieldId)
                    End While
                    For Each keyValuePair As KeyValuePair(Of UShort, String) In fieldIdByPositionDictionary
                        Dim fieldId As String = keyValuePair.Value
                        Dim position As UShort = keyValuePair.Key
                        Dim nillable As Boolean = getXMLElementValue(Of Boolean)(String.Format(XPATH_FIELD_NILLABLE_QUERY, fieldId))
                        Dim quoted As Boolean = getXMLElementValue(Of Boolean)(String.Format(XPATH_FIELD_QUOTED_QUERY, fieldId))
                        Dim description As String = Nothing
                        Try
                            description = getXMLElementValue(Of String)(String.Format(XPATH_FIELD_DESCRIPTION_QUERY, fieldId))
                        Catch optionalXmlAttributeNotFoundException As XmlException
                            description = ""
                        End Try
                        Dim mapped_AttributeName As String = getXMLElementValue(Of String)(String.Format(XPATH_FIELD_MAPPED_ATTRIBUTENAME_QUERY, fieldId), True, "mapped_AttributeName")
                        Dim mapped_Description As String = Nothing
                        Try
                            mapped_Description = getXMLElementValue(Of String)(String.Format(XPATH_FIELD_MAPPED_DESCRIPTION_QUERY, fieldId), True, "mapped_Description")
                        Catch optionalXmlAttributeNotFoundException As XmlException
                            mapped_Description = ""
                        End Try
                        Dim csvFieldTo As SimpleCSVField = New SimpleCSVField() With {
                            .id = fieldId,
                            .position = position,
                            .nillable = nillable,
                            .quoted = quoted,
                            .description = description,
                            .mappedAttributeName = mapped_AttributeName,
                            .mappedDescription = mapped_Description
                        }
                        Me._csv_Fields.Add(csvFieldTo)
                    Next
                End Sub

#End Region 'Protected methods

#Region "IDisposable interface implementation"

                Private disposedValue As Boolean ' To detect redundant calls

                ' IDisposable
#Disable Warning IDE1006
                Protected Overridable Sub Dispose(disposing As Boolean)
                    If Not disposedValue Then
                        If disposing Then
                            If (Not IsNothing(Me._xsdSchemaTempFilePath)) AndAlso IO.File.Exists(Me._xsdSchemaTempFilePath) Then
                                Try
                                    IO.File.Delete(Me._xsdSchemaTempFilePath)
                                Catch ignoredException As Exception
                                    ignoredException = Nothing
                                End Try
                            End If
                            If Not IsNothing(Me._csvFileReader) Then
                                With Me._csvFileReader
                                    Try
                                        .Close()
                                    Catch ignoredException As Exception
                                        ignoredException = Nothing
                                        .Dispose()
                                    End Try
                                End With
                            End If
                        End If
                        End If
                    disposedValue = True
                End Sub

                Protected Overrides Sub Finalize()
                    Dispose(False)
                    MyBase.Finalize()
                End Sub

                Public Sub Dispose() Implements IDisposable.Dispose
                    Dispose(True)
                    GC.SuppressFinalize(Me)
                End Sub

#End Region 'IDisposable interface implementation

            End Class

        End Namespace
    End Namespace
End Namespace