#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports EasyCSVNet.easycsvnet.csv.export
Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace testconsoleapp
    Namespace test
        Namespace test001

            <Serializable()>
            Public Class TestColourDTO
                Implements IComparable(Of TestColourDTO),
                           IClonable(Of TestColourDTO),
                           ICSVExportable

#Region "Private attributes"

                Private _name As String = ""
                Private _price As Single? = Nothing
                <NonCSVExported()> Private _comments As String = Nothing

#End Region 'Private attributes

#Region "Public methods"

#Region "Constructors"

                Sub New() 'Default empy-args constructor definition needed by CSV importer
                    Me.New(Nothing)
                End Sub

                Sub New(ByVal _name As String, Optional ByVal _price As Single? = Nothing, Optional _comments As String = Nothing)
                    With Me
                        .name = _name
                        .price = _price
                        .comments = _comments
                    End With
                End Sub

#End Region 'Constructors

#Region "Properties (getters & setters)"

                Public Property name() As String
                    Get
                        Return Me._name
                    End Get
                    Set(ByVal value As String)
                        Me._name = Utils.coalesce(value)
                    End Set
                End Property

                Public Property price() As Single?
                    Get
                        Return Me._price
                    End Get
                    Set(ByVal value As Single?)
                        Me._price = value
                    End Set
                End Property

                Public Property comments() As String
                    Get
                        Return Me._comments
                    End Get
                    Set(ByVal value As String)
                        Me._comments = Utils.coalesce(value)
                    End Set
                End Property

#End Region 'Properties (getters & setters)

#Region "Generic methods (override System.Object's)"

                Public Overrides Function toString() As String
                    Return Utils.toStr(Me)
                End Function

                Public Overrides Function equals(ByVal other As Object) As Boolean
                    Try
                        If TypeOf other Is TestColourDTO Then
                            Dim myObject As TestColourDTO = CType(other, TestColourDTO)
                            With myObject
                                Dim result As Boolean = True
                                If result Then result = Utils.equals(Of String)(Me.name, .name)
                                If result Then result = Utils.equals(Of Single?)(Me.price, .price)
                                If result Then result = Utils.equals(Of String)(Me.comments, .comments)
                                Return result
                            End With
                        Else
                            Return False
                        End If
                    Catch ignoredException As Exception
                        ignoredException = Nothing
                        Return Nothing
                    End Try
                End Function

#End Region 'Generic methods (override System.Object's)

#Region "IComparable interface implementation"

                Public Function compareTo(ByVal other As TestColourDTO) As Integer Implements IComparable(Of TestColourDTO).CompareTo
                    Try
                        If IsNothing(other) Then Return -1
                        With other
                            Dim result As Integer = 0
                            If result = 0 Then result = Utils.compare(Of String)(Me.name, .name)
                            If result = 0 Then result = Utils.compare(Of Single)(Me.price, .price)
                            If result = 0 Then result = Utils.compare(Of String)(Me.comments, .comments)
                            Return result
                        End With
                    Catch ignoredException As Exception
                        ignoredException = Nothing
                        Return Nothing
                    End Try
                End Function

#End Region 'IComparable interface implementation

#Region "IClonable interface implementation"

                Public Function clone() As TestColourDTO Implements IClonable(Of TestColourDTO).clone
                    Return ObjectCloner.clone(Me)
                End Function

#End Region 'IClonable interface implementation

#Region "ICSVExportable interface implementation"

                Public Function exportToCSVLine(Optional ByVal delimiterStr As String = SimpleCSVFileExporter(Of TestColourDTO).DEFAULT_DELIMITER_STRING) As String Implements ICSVExportable.exportToCSVLine
                    Return SimpleCSVFileExporter(Of TestColourDTO).exportToCSVLine(Me, delimiterStr)
                End Function

#End Region 'ICSVExportable interface implementation

#End Region 'Mètodes públics

            End Class

        End Namespace
    End Namespace
End Namespace