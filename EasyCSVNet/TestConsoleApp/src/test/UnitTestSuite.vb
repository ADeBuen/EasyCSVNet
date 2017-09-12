#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports EasyCSVNet.easycsvnet.csv.import
Imports EasyCSVNet.easycsvnet.csv.export
Imports EasyCSVNet.easycsvnet.util

Imports TestConsoleApp.testconsoleapp.test.test001
Imports TestConsoleApp.testconsoleapp.test.test002

#End Region 'Imports

Namespace testconsoleapp
    Namespace test

        Public NotInheritable Class UnitTestSuite

#Region "Public Enums"

            Public Enum ExitCode As Integer
                EXIT_OK = 0
                EXIT_ERROR = 1
            End Enum

#End Region 'Public Enums

#Region "Protected constants"

            Protected Const CSV_OCM_PATH As String = "conf/csvOCM"

#End Region 'Protected constants

#Region "Public methods"

            Public Shared Function runAllTests() As ExitCode

                Dim _exitCode As ExitCode = ExitCode.EXIT_OK
                Try
                    Console.WriteLine("[+] STARTing Unit Tests..." & vbCrLf)

                    runTest001("Test #001 :: Importing CSV colour file to TestColourDTO object list & exporting to CSV")

                    runTest002("Test #002 :: Importing CSV patient file to TestPatientDTO object list & exporting to CSV")

                    Console.WriteLine(vbCrLf & "[+] ... ENDing Unit Tests.")
                Catch myException As Exception
                    _exitCode = ExitCode.EXIT_ERROR
                    handleExceptionCustomCallback(myException)
                Finally
                    Console.WriteLine(vbCrLf & "Press a key to exit...")
                    Console.ReadKey(True)
                End Try

                Return _exitCode

            End Function

#End Region 'Public methods

#Region "Protected methods"

            Protected Shared Sub handleExceptionCustomCallback(ByVal exception As Exception)
                Dim logMessage As String = Nothing
                Dim errDescription As System.Text.StringBuilder = New System.Text.StringBuilder("")
                Dim innerException As Exception = exception
                With errDescription
                    While Not IsNothing(innerException)
                        Dim errMessage As String = innerException.Message
                        If Utils.isNullOrEmptyString(errMessage) Then
                            errMessage = "-Unknown error-"
                        Else
                            errMessage = String.Format("'{0}'", errMessage)
                        End If
                        .Append(vbCrLf).Append(errMessage)
                        innerException = innerException.InnerException
                    End While
                    If Not Utils.isNullOrEmptyString(exception.HelpLink) Then .Append(vbCrLf).Append("     [link: ").Append(exception.HelpLink).Append("]")
                    If Not Utils.isNullOrEmptyString(exception.Source) Then .Append(vbCrLf).Append("     [source: ").Append(exception.Source).Append("]")
                    If Not IsNothing(exception.TargetSite) Then .Append(vbCrLf).Append("     [target: ").Append(exception.TargetSite.ToString()).Append("]")
                    If Not Utils.isNullOrEmptyString(exception.StackTrace) Then .Append(vbCrLf).Append(exception.StackTrace)
                    logMessage = .ToString()
                End With
                Console.WriteLine(String.Format("ERROR: {0}", Utils.coalesce(logMessage)))
            End Sub

#End Region 'Protected methods

#Region "Private Methods"

#Region "Constructors"

            Private Sub New()
                'Static class
            End Sub

#End Region 'Constructors

#Region "Unit tests"

            'Test #001 :: Importing CSV colour file to TestColourDTO object list
            Private Shared Sub runTest001(ByVal testName As String)

                'Test #001 starts
                Console.WriteLine(String.Format(vbCrLf & "=== {0} ===" & vbCrLf, Utils.coalesce(testName)))

                'Set input CSV file path
                Dim csvInputFilePath As String = "ext/test/test001/colours.csv"
                Console.WriteLine(String.Format("Importing CSV file '{0}'...", csvInputFilePath))

                'Set XML definition file path
                Dim definitionXMLFilePath As String = Utils.formPath(CSV_OCM_PATH, "csvOCM_TestColourDTO_def.xml")

                'Instantiate CSV importer
                Dim csvFileImporter As SimpleCSVFileImporter(Of TestColourDTO) = New SimpleCSVFileImporter(Of TestColourDTO)(csvInputFilePath, definitionXMLFilePath)

                'Use CSV importer to parse CSV input file and get DTO object list
                Console.WriteLine(String.Format("Parsing CSV file '{0}'...", csvInputFilePath))
                Dim colourDTOList As IList(Of TestColourDTO) = csvFileImporter.parseCSVData()

                'Output DTO object list obtained
                Console.WriteLine("Showing colour DTO list imported:" & vbCrLf)
                Dim numColoursImported As ULong = 0UL
                For Each colourDTO As TestColourDTO In colourDTOList
                    numColoursImported += 1UL
                    Console.WriteLine(">> Colour #{0} : {1}", numColoursImported, colourDTO)
                Next

                'Set output CSV file path
                Dim csvOutputFilePath As String = "ext/test/test001/colours_output.csv"

                'Export DTO object list to output CSV file
                Console.WriteLine(String.Format(vbCrLf & "Exporting CSV file '{0}'...", csvOutputFilePath))
                Dim csvFileExporter As SimpleCSVFileExporter(Of TestColourDTO) = New SimpleCSVFileExporter(Of TestColourDTO)(csvOutputFilePath)
                Dim numColoursExported As ULong = csvFileExporter.exportDTOList(colourDTOList)
                Console.WriteLine(String.Format("{0} colour(s) exported.", numColoursExported))

            End Sub

            'Test #002 :: Importing CSV patient file To TestPatientDTO Object list
            Private Shared Sub runTest002(ByVal testName As String)

                'Test #002 starts
                Console.WriteLine(String.Format(vbCrLf & "=== {0} ===" & vbCrLf, Utils.coalesce(testName)))

                'Set input CSV file path
                Dim csvInputFilePath As String = "ext/test/test002/patients.csv"
                Console.WriteLine(String.Format("Importing CSV file '{0}'...", csvInputFilePath))

                'Set XML definition file path
                Dim definitionXMLFilePath As String = Utils.formPath(CSV_OCM_PATH, "csvOCM_TestPatientDTO_def.xml")

                'Instantiate CSV importer
                Dim csvFileImporter As TestPatientDTOCSVFileImporter = New TestPatientDTOCSVFileImporter(csvInputFilePath)

                'Use CSV importer to parse CSV input file and get DTO object list
                Console.WriteLine(String.Format("Parsing CSV file '{0}'...", csvInputFilePath))
                Dim patientDTOList As IList(Of TestPatientDTO) = csvFileImporter.parseCSVData()

                'Output DTO object list obtained
                Console.WriteLine("Showing patient DTO list imported:" & vbCrLf)
                Dim numPatientsImported As ULong = 0UL
                For Each patientDTO As TestPatientDTO In patientDTOList
                    numPatientsImported += 1UL
                    Console.WriteLine(">> Patient #{0} : {1}", numPatientsImported, patientDTO)
                Next

                'Set output CSV file path
                Dim csvOutputFilePath As String = "ext/test/test002/patients_output.csv"

                'Export DTO object list to output CSV file
                Console.WriteLine(String.Format(vbCrLf & "Exporting CSV file '{0}'...", csvOutputFilePath))
                Dim csvFileExporter As TestPatientDTOCSVFileExporter = New TestPatientDTOCSVFileExporter(csvOutputFilePath)
                Dim numPatientsExported As ULong = csvFileExporter.exportDTOList(patientDTOList)
                Console.WriteLine(String.Format("{0} patient(s) exported.", numPatientsExported))

            End Sub

#End Region 'Unit tests

#End Region 'Private methods

        End Class

    End Namespace
End Namespace
