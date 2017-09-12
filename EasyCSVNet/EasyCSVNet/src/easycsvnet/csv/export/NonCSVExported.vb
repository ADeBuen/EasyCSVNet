#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace csv
        Namespace export

            <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True)>
            Public Class NonCSVExported : Inherits Attribute
                'Attribute class
            End Class

        End Namespace
    End Namespace
End Namespace
