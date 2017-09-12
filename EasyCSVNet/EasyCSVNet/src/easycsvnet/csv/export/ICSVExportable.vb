#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace csv
        Namespace export

            Public Interface ICSVExportable

#Region "Public abstract methods"

                Function exportToCSVLine(Optional ByVal delimiterStr As String = SimpleCSVFileExporter(Of ICSVExportable).DEFAULT_DELIMITER_STRING) As String

#End Region 'Public abstract methods

            End Interface

        End Namespace
    End Namespace
End Namespace