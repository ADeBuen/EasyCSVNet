#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.Reflection
Imports EasyCSVNet.easycsvnet.csv.import
Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace easycsvnet
    Namespace csv
        Namespace export

            Public Class SimpleCSVFileExporter(Of T As ICSVExportable) : Implements IDisposable

#Region "Public constants"

                Public Const DEFAULT_DELIMITER_STRING As String = ","

#End Region 'Public constants

#Region "Protected inner classes"

                Protected Class FieldInfoByNameComparer : Implements IComparer(Of FieldInfo)

                    Public Function Compare(x As FieldInfo, y As FieldInfo) As Integer Implements IComparer(Of FieldInfo).Compare
                        If IsNothing(x) Then
                            If IsNothing(y) Then Return 0 Else Return 1
                        ElseIf IsNothing(y) Then
                            Return -1
                        Else
                            Return String.Compare(x.Name, y.Name)
                        End If
                    End Function

                End Class

#End Region 'Protected inner classes

#Region "Protected attributes"

                Protected _delimiterStr As String = DEFAULT_DELIMITER_STRING
                Protected _filePath As String = Nothing

#End Region 'Protected attributes

#Region "Private attributes"

                Private _fileWriter As System.IO.StreamWriter = Nothing

#End Region 'Private attributes

#Region "Public methods"

#Region "Constructors"

                Public Sub New(ByVal filePath As String, Optional ByVal delimiterStr As String = DEFAULT_DELIMITER_STRING)
                    If Utils.isNullOrEmptyString(filePath) Then Throw New ArgumentException("Null or empty file name passed")
                    With Me
                        ._filePath = filePath
                        ._delimiterStr = Utils.coalesce(delimiterStr)
                    End With
                End Sub

                Public Sub New(ByVal fileWriter As System.IO.StreamWriter, Optional ByVal delimiterStr As String = DEFAULT_DELIMITER_STRING)
                    If IsNothing(fileWriter) Then Throw New ArgumentException("Null or empty fileWriter stream passed")
                    With Me
                        ._fileWriter = fileWriter
                        ._filePath = "<unknown>"
                        ._delimiterStr = Utils.coalesce(delimiterStr)
                    End With
                End Sub

#End Region 'Constructors

                '@Overridable
                '@Return Boolean {True = Accept line | False = Skip line}
                Public Overridable Function lineBeforeExportVisitor_hookCallBack(ByVal numLine As ULong, ByRef dto As T, ByVal fileWriter As IO.StreamWriter) As Boolean
                    Return True
                End Function

                '@Overridable
                '@Return Boolean {True = Accept line | False = Skip line}
                Public Overridable Function lineAfterExportVisitor_hookCallBack(ByVal numLine As ULong, ByRef dto As T, ByRef csvLine As String, ByVal fileWriter As IO.StreamWriter) As Boolean
                    Return True
                End Function

                '@Overridable
                '@Return Boolean {True = Accept field | False = Skip field}
                Public Overridable Function fileBeforeExportVisitor_hookCallBack(ByRef dtoList As IEnumerable(Of T), ByVal fileWriter As IO.StreamWriter) As Boolean
                    Return True
                End Function

                '@Overridable
                '@Return Boolean {True = Accept field | False = Skip field}
                Public Overridable Function fileAfterExportVisitor_hookCallBack(ByRef dtoList As IEnumerable(Of T), ByVal fileWriter As IO.StreamWriter, ByVal numLines As ULong) As Boolean
                    Return True
                End Function

                Public Function exportDTOList(ByVal dtoList As IEnumerable(Of T)) As ULong
                    Try
                        If IsNothing(Me._fileWriter) Then
                            If Not System.IO.File.Exists(_filePath) Then
                                Me._fileWriter = New System.IO.StreamWriter(System.IO.File.Create(Me._filePath))
                            Else
                                Me._fileWriter = New System.IO.StreamWriter(Me._filePath)
                            End If
                        End If
                        If IsNothing(Me._fileWriter) Then Throw New System.IO.IOException(String.Format("Error creating or accessing file '{0}' - null or empty file writer stream", Utils.coalesce(Me._filePath)))
                        Me.fileBeforeExportVisitor_hookCallBack(dtoList, Me._fileWriter)
                        Dim numItemsExported As ULong = 0UL
                        If Not IsNothing(dtoList) Then
                            Dim iter As IEnumerator(Of T) = dtoList.GetEnumerator()
                            While (iter.MoveNext)
                                Dim dto As T = iter.Current
                                If Me.lineBeforeExportVisitor_hookCallBack(numItemsExported + 1UL, dto, Me._fileWriter) Then
                                    If Not IsNothing(dto) Then
                                        Dim csvLine As String = dto.exportToCSVLine(Me._delimiterStr)
                                        If Me.lineAfterExportVisitor_hookCallBack(numItemsExported + 1UL, dto, csvLine, Me._fileWriter) Then
                                            Me._fileWriter.WriteLine(csvLine)
                                            numItemsExported += 1UL
                                        End If
                                    End If
                                End If
                            End While
                            Me.fileAfterExportVisitor_hookCallBack(dtoList, Me._fileWriter, numItemsExported)
                            Me._fileWriter.Flush()
                        End If
                        Return numItemsExported
                    Catch myException As Exception
                        Throw New System.IO.IOException(String.Format("Error creating or accessing file '{0}'", Utils.coalesce(Me._filePath)), myException)
                    Finally
                        If Not IsNothing(Me._fileWriter) Then
                            Me._fileWriter.Close()
                            Me._fileWriter = Nothing
                        End If
                    End Try
                End Function

                Public Shared Function exportToCSVLine(ByVal obj As Object, Optional ByVal csvFieldDelimiter As String = DEFAULT_DELIMITER_STRING, Optional fieldTypeMapperEngine As ITypeToStringMapper = Nothing, Optional strictTypeMapping As Boolean = True) As String
                    Dim csvLineItems As IList(Of String) = New List(Of String)()
                    With csvLineItems
                        If Not IsNothing(obj) Then
                            Dim type As System.Type = obj.GetType()
                            If type.IsClass Then
                                Dim fieldArray As FieldInfo() = type.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
                                Array.Sort(fieldArray, New FieldInfoByNameComparer())
                                Dim excludedFieldsArray As String() = {"_size", "_version", "_syncRoot", "_items"}
                                For Each field As FieldInfo In fieldArray
                                    Dim fieldValue As Object = field.GetValue(obj)
                                    Dim fieldAttributesTypeList As ISet(Of Type) = New HashSet(Of Type)()
                                    For Each attr As Attribute In field.GetCustomAttributes()
                                        fieldAttributesTypeList.Add(attr.GetType())
                                    Next
                                    If Not fieldAttributesTypeList.Contains(GetType(NonCSVExported)) Then
                                        If Array.TrueForAll(Of String)(excludedFieldsArray, Function(element) CBool(IIf(element.Equals(field.Name), False, True))) Then
                                            Try
                                                If IsNothing(fieldTypeMapperEngine) Then fieldTypeMapperEngine = New CSVFieldTypeMapperEngine()
                                                .Add(fieldTypeMapperEngine.mapToString(fieldValue))
                                            Catch mappingException As Exception
                                                If strictTypeMapping Then Throw mappingException
                                                .Add(Utils.toStr(fieldValue))
                                            End Try
                                        End If
                                    End If
                                Next
                            Else
                                .Add(Utils.toStr(obj))
                            End If
                        End If
                        Return String.Join(csvFieldDelimiter, csvLineItems.ToArray())
                    End With
                End Function

#End Region 'Public methods

#Region "Protected methods"

                Protected Sub init(ByRef file As System.IO.File, Optional ByVal delimiterStr As String = DEFAULT_DELIMITER_STRING)
                    Me._delimiterStr = Utils.coalesce(delimiterStr)
                End Sub

#End Region 'Protected methods

#Region "IDisposable interface implementation"

                Private disposedValue As Boolean ' To detect redundant calls

                ' IDisposable
#Disable Warning IDE1006
                Protected Overridable Sub Dispose(disposing As Boolean)
                    If Not disposedValue Then
                        If disposing Then
                            If Not IsNothing(Me._fileWriter) Then
                                With Me._fileWriter
                                    Try
                                        .Flush()
                                    Catch ignoredException As Exception
                                        ignoredException = Nothing
                                    Finally
                                        Try
                                            .Close()
                                        Catch ignoredException As Exception
                                            ignoredException = Nothing
                                            .Dispose()
                                        Finally
                                        End Try
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