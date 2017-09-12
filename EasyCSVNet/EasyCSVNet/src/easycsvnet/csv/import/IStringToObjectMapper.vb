#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace csv
        Namespace import

            Public Interface IStringToObjectMapper

#Region "Public abstract methods"

                Function mapFromStringToType(ByVal strValue As String) As Object
                Function mapFromTypeToString(ByVal objValue As Object) As String

#End Region 'Public abstract methods

            End Interface

        End Namespace
    End Namespace
End Namespace