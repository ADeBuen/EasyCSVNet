#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.Xml
Imports System.Xml.Schema
Imports System.Xml.XPath
Imports System.IO

Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace easycsvnet
    Namespace csv
        Namespace import

            Public MustInherit Class GenericXMLParser(Of T)

#Region "Protected attributes"

                Protected _xmlReader As XmlReader = Nothing
                Protected _xmlReaderSettings As XmlReaderSettings = Nothing
                Protected _xPathNavigator As XPathNavigator = Nothing
                Protected _xmlNamespaceManager As XmlNamespaceManager = Nothing
                Protected _metaVarsDictionary As IDictionary(Of String, String) = Nothing

#End Region 'Protected attributes

#Region "Public methods"

                Public Property metaVarsDictionary As IDictionary(Of String, String)
                    Get
                        Return Me._metaVarsDictionary
                    End Get
                    Set(ByVal metaVarsDictionary As IDictionary(Of String, String))
                        Me._metaVarsDictionary = metaVarsDictionary
                    End Set
                End Property

                Public Sub importXMLTemplate(ByVal xmlFilePath As String, Optional ByVal metaVarsDictionary As IDictionary(Of String, String) = Nothing, Optional ByVal xsdSchemaFilePath As String = Nothing, Optional ByVal xsdSchemaTargetNamespace As String = Nothing)
                    If Utils.isNullOrEmptyString(xmlFilePath) Then Throw New ArgumentException("Null or empty XML file name passed")
                    If Not File.Exists(xmlFilePath) Then Throw New ArgumentException(String.Format("XML file '{0}' not found", xmlFilePath))
                    Me._xmlReaderSettings = New XmlReaderSettings()
                    If Not Utils.isNullOrEmptyString(xsdSchemaFilePath) Then
                        If Not File.Exists(xmlFilePath) Then Throw New ArgumentException(String.Format("XSD file '{0}' not found", xmlFilePath))
                        Dim schemaXMLReader As XmlReader = XmlReader.Create(xsdSchemaFilePath)
                        Me._xmlReaderSettings.Schemas.Add(xsdSchemaTargetNamespace, schemaXMLReader)
                        Me._xmlReaderSettings.ValidationType = ValidationType.Schema
                        AddHandler Me._xmlReaderSettings.ValidationEventHandler, New ValidationEventHandler(AddressOf xmlReaderSettingsValidationEventHandler)
                    End If
                    Try
                        Me._xmlReader = XmlReader.Create(xmlFilePath, Me._xmlReaderSettings)
                        Me._xPathNavigator = New XPathDocument(xmlFilePath).CreateNavigator()
                        Me._metaVarsDictionary = metaVarsDictionary
                        parseXMLDefinition()
                    Finally
                        If Not IsNothing(Me._xmlReader) Then Me._xmlReader.Close()
                    End Try
                End Sub

                Public Sub addMetaVar(ByVal key As String, ByVal value As String)
                    If Utils.isNullOrEmptyString(key) Then Throw New ArgumentException("Null or empty metavariable name")
                    If IsNothing(Me._metaVarsDictionary) Then Me._metaVarsDictionary = New Dictionary(Of String, String)()
                    If Not Me._metaVarsDictionary.ContainsKey(key) Then Me._metaVarsDictionary.Add(key, Utils.coalesce(value))
                End Sub

#End Region 'Public methods

#Region "Protected methods"

                Protected Overridable Sub xmlReaderSettingsValidationEventHandler(ByVal sender As Object, ByVal eventArgs As ValidationEventArgs)
                    Const RAISE_EXCEPTION_ON_WARNINGS As Boolean = False
                    If eventArgs.Severity = XmlSeverityType.Error Then
                        Throw New XmlException(String.Format("ERROR while parsing/binding XSD schema :: {0} [severity: {1}]", Utils.coalesce(eventArgs.Message), eventArgs.Severity), eventArgs.Exception)
                    ElseIf RAISE_EXCEPTION_ON_WARNINGS AndAlso eventArgs.Severity = XmlSeverityType.Warning Then
                        Throw New XmlException(String.Format("WARNING while parsing/binding XSD schema :: {0} [severity: {1}]", Utils.coalesce(eventArgs.Message), eventArgs.Severity), eventArgs.Exception)
                    End If
                End Sub

                Protected Function getNillableXMLElementValue(Of T2)(ByVal xPathQuery As String, Optional ByVal mandatory As Boolean = True, Optional ByVal xmlElementName As String = Nothing) As T2
                    Dim nillableValue As String = getXMLElementValue(Of String)(xPathQuery, mandatory, xmlElementName)
                    If Utils.isNullOrEmptyString(nillableValue) Then
                        Return Nothing
                    Else
                        Return getXMLElementValue(Of T2)(xPathQuery, mandatory, xmlElementName)
                    End If
                End Function

                Protected Function getXMLElementValue(Of T2)(ByVal xPathQuery As String, Optional ByVal mandatory As Boolean = True, Optional ByVal xmlElementName As String = Nothing, Optional ByVal format As String = Nothing) As T2
                    Dim elementList As IList(Of T2) = getXMLElementValueList(Of T2)(xPathQuery, mandatory, xmlElementName, format)
                    If (IsNothing(elementList) OrElse elementList.Count < 1) Then
                        Return CType(Utils.getDefaultValue(Of T2)(), T2)
                    Else
                        Dim iter As IEnumerator(Of T2) = elementList.GetEnumerator
                        iter.MoveNext()
                        Return iter.Current
                    End If
                End Function

                Protected Function getXMLElementValueList(Of T2)(ByVal xPathQuery As String, Optional ByVal mandatory As Boolean = True, Optional ByVal xmlElementName As String = Nothing, Optional ByVal format As String = Nothing) As IList(Of T2)
                    Dim elementList As IList(Of T2) = New List(Of T2)
                    Dim iter As XPathNodeIterator = Me._xPathNavigator.Select(xPathQuery)
                    While (iter.MoveNext)
                        If IsNothing(iter.Current.Value) Then
                            elementList.Add(CType(Utils.getDefaultValue(Of T2)(), T2))
                        Else
                            Dim value As String = substituteMetaVars(iter.Current.Value)
                            elementList.Add(CType(Utils.parseToObject(Of T2)(value, format), T2))
                        End If
                    End While
                    If (mandatory AndAlso elementList.Count < 1) Then
                        If Utils.isNullOrEmptyString(xmlElementName) Then
                            Throw New XmlException(String.Format("XML parser error :: cannot evaluate XPath query '{0}'", xPathQuery))
                        Else
                            Throw New XmlException(String.Format("XML parser error :: cannot find '{0}' element", xmlElementName))
                        End If
                    End If
                    Return elementList
                End Function

                Protected Function substituteMetaVars(ByVal text As String) As String
                    Dim result As String = text
                    If Not (IsNothing(Me._metaVarsDictionary) OrElse Me._metaVarsDictionary.Count < 1) Then
                        Dim iter As IEnumerator(Of KeyValuePair(Of String, String)) = Me._metaVarsDictionary.GetEnumerator
                        While iter.MoveNext
                            Dim key As String = iter.Current.Key
                            If Not Utils.isNullOrEmptyString(key) Then
                                Dim keyLabel As String = "{" & key & "}"
                                Dim value As String = CStr(IIf(Utils.isNullOrEmptyString(iter.Current.Value), "", iter.Current.Value))
                                result = result.Replace(keyLabel, value)
                            End If
                        End While
                    End If
                    Return result
                End Function

                Protected MustOverride Sub parseXMLDefinition()

#End Region 'Protected methods

            End Class

        End Namespace
    End Namespace
End Namespace