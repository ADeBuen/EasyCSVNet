#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace csv
        Namespace import

            Public Interface IStringToTypeMapper

#Region "Public abstract methods"

                Overloads Function mapTo(Of T)(ByVal strValue As String) As T
                Overloads Function mapTo(ByVal _type As Type, ByVal strValue As String) As Object
                Overloads Function mapTo(ByVal typeFullName As String, ByVal strValue As String) As Object

#End Region 'Public abstract methods

            End Interface

        End Namespace
    End Namespace
End Namespace