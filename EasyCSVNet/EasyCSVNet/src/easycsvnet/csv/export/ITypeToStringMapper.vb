#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace csv
        Namespace export

            Public Interface ITypeToStringMapper

#Region "Public abstract methods"

                Function mapToString(ByVal obj As Object) As String

#End Region 'Public abstract methods

            End Interface

        End Namespace
    End Namespace
End Namespace