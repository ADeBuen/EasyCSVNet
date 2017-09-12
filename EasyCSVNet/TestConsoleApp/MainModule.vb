#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports TestConsoleApp.testconsoleapp.test

#End Region 'Imports

Module MainModule

    'Main method
    Public Sub main()

        Environment.ExitCode = UnitTestSuite.runAllTests()

    End Sub

End Module
