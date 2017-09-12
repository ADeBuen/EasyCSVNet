#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace util

        Public Interface IClonable(Of T)

#Region "Public abstract methods"

            Function clone() As T

#End Region 'Public abstract methods

        End Interface

    End Namespace
End Namespace