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
        Namespace test002

            <Serializable()>
            Public Class TestPatientDTO
                Implements IComparable(Of TestPatientDTO),
                           IClonable(Of TestPatientDTO),
                           ICSVExportable

#Region "Private attributes"

                <NonSerialized()> Private _id As ULong? = Nothing
                Private _name As String = ""
                Private _birthDate As Date? = Nothing
                <NonSerialized()> Private _presentAgeInYears As UShort? = Nothing
                Private _acceptFlag As Boolean? = Nothing

#End Region 'Private attributes

#Region "Public methods"

#Region "Constructors"

                Sub New() 'Default empy-args constructor definition needed by CSV importer
                    Me.New(Nothing)
                End Sub

                Sub New(ByVal _id As ULong?, Optional ByVal _name As String = "", Optional ByVal _birthDate As Date? = Nothing, Optional ByVal _acceptFlag As Boolean? = Nothing)
                    With Me
                        .id = _id
                        .name = _name
                        .birthDate = _birthDate
                        .calculatePresentAgeInYears()
                        .acceptFlag = _acceptFlag
                    End With
                End Sub

#End Region 'Constructors

#Region "Properties (getters & setters)"

                Public Property id() As ULong?
                    Get
                        Return Me._id
                    End Get
                    Set(ByVal value As ULong?)
                        Me._id = value
                    End Set
                End Property

                Public Property name() As String
                    Get
                        Return Me._name
                    End Get
                    Set(ByVal value As String)
                        Me._name = Utils.coalesce(value)
                    End Set
                End Property

                Public Property birthDate() As Date?
                    Get
                        Return Me._birthDate
                    End Get
                    Set(ByVal value As Date?)
                        Me._birthDate = value
                    End Set
                End Property

                Public ReadOnly Property presentAgeInYears() As UShort?
                    Get
                        Return Me._presentAgeInYears
                    End Get
                End Property

                Public Property acceptFlag() As Boolean?
                    Get
                        Return Me._acceptFlag
                    End Get
                    Set(ByVal value As Boolean?)
                        Me._acceptFlag = value
                    End Set
                End Property

#End Region 'Properties (getters & setters)

#Region "Generic methods (override System.Object's)"

                Public Overrides Function toString() As String
                    Return Utils.toStr(Me)
                End Function

                Public Overrides Function equals(ByVal other As Object) As Boolean
                    Try
                        If TypeOf other Is TestPatientDTO Then
                            Dim myObject As TestPatientDTO = CType(other, TestPatientDTO)
                            With myObject
                                Dim result As Boolean = True
                                If result Then result = Utils.equals(Of String)(Me.name, .name)
                                If result Then result = Utils.equals(Of Date?)(Me.birthDate, .birthDate)
                                If result Then result = Utils.equals(Of Boolean?)(Me.acceptFlag, .acceptFlag)
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

                Public Function compareTo(ByVal other As TestPatientDTO) As Integer Implements IComparable(Of TestPatientDTO).CompareTo
                    Try
                        If IsNothing(other) Then Return -1
                        With other
                            Dim result As Integer = 0
                            If result = 0 Then result = Utils.compare(Of String)(Me.name, .name)
                            If result = 0 Then result = Utils.compare(Of Date)(Me.birthDate, .birthDate)
                            If result = 0 Then result = Utils.compare(Of Boolean)(Me.acceptFlag, .acceptFlag)
                            Return result
                        End With
                    Catch ignoredException As Exception
                        ignoredException = Nothing
                        Return Nothing
                    End Try
                End Function

#End Region 'IComparable interface implementation

#Region "IClonable interface implementation"

                Public Function clone() As TestPatientDTO Implements IClonable(Of TestPatientDTO).clone
                    Return ObjectCloner.clone(Me)
                End Function

#End Region 'IClonable interface implementation

#Region "ICSVExportable interface implementation"

                Public Function exportToCSVLine(Optional ByVal delimiterStr As String = SimpleCSVFileExporter(Of TestPatientDTO).DEFAULT_DELIMITER_STRING) As String Implements ICSVExportable.exportToCSVLine
                    Const EXPORT_DATE_FORMAT_PATTERN As String = "dd/MM/yyyy"
                    With Me
                        Return String.Join(delimiterStr, {Utils.toStr(.id), Utils.toStr(.name), Utils.toStr(.birthDate, EXPORT_DATE_FORMAT_PATTERN), Utils.toStr(.acceptFlag)})
                    End With
                End Function

#End Region 'ICSVExportable interface implementation

                Public Sub calculatePresentAgeInYears()
                    If IsNothing(Me.birthDate) OrElse Not Me.birthDate.HasValue Then
                        Me._presentAgeInYears = Nothing
                    Else
                        Me._presentAgeInYears = Convert.ToUInt16(DateDiff(DateInterval.Year, Me.birthDate.Value, Now))
                    End If
                End Sub

#End Region 'Mètodes públics

            End Class

        End Namespace
    End Namespace
End Namespace