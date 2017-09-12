#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions

#End Region 'Imports

Namespace easycsvnet
    Namespace util

        Public NotInheritable Class Utils

#Region "Declarations"

            Private Class NativeMethods
                Public Declare Function AllocConsole Lib "kernel32" () As Int32
                Public Declare Function FreeConsole Lib "kernel32" () As Int32
            End Class

#End Region 'Declarations

#Region "Private methods"

#Region "Constructors"

            Private Sub New()
                'Static class
            End Sub

#End Region 'Constructors

#End Region 'Private methods

#Region "Public methods"

            Public Shared Function isNullOrEmptyString(ByVal str As String) As Boolean
                If ((IsNothing(str)) OrElse ("".Equals(str)) OrElse (str.Length = 0)) Then
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Shared Function isNullOrEmptyDate(ByVal dt As Date) As Boolean
                If ((IsNothing(dt)) OrElse (New Date() = dt) OrElse (New Date(0L) = dt)) Then
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Shared Function extractDateFromDateTime(ByVal dt As DateTime, Optional ByVal dateFormatPattern As String = "dd-MM-yyyy") As String
                If IsNothing(dt) Then
                    Throw New System.ArgumentNullException("Null DateTime object")
                ElseIf (Utils.isNullOrEmptyString(dateFormatPattern)) Then
                    Throw New System.ArgumentNullException("Wrong DateTime format")
                End If
                Try
                    Dim result As String = dt.ToString(dateFormatPattern)
                    Return result
                Catch myException As Exception
                    Throw New FormatException("Error trying to format date extracted from DateTime object", myException)
                End Try
            End Function

            Public Shared Function extractTimeFromDateTime(ByVal dt As DateTime, Optional ByVal timeFormatPattern As String = "HH:mm:ss") As String
                If IsNothing(dt) Then
                    Throw New System.ArgumentNullException("Null DateTime object")
                ElseIf (Utils.isNullOrEmptyString(timeFormatPattern)) Then
                    Throw New System.ArgumentNullException("Wrong time format")
                End If
                Try
                    Dim result As String = dt.ToString(timeFormatPattern)
                    Return result
                Catch myException As Exception
                    Throw New FormatException("Error trying to format time extracted from DateTime object", myException)
                End Try
            End Function

            Public Shared Function coalesce(ByVal str As String, Optional ByVal coalesceValue As String = "") As String
                If isNullOrEmptyString(str) Then
                    Return coalesceValue
                Else
                    Return str
                End If
            End Function

            Public Overloads Shared Function toStr(ByVal obj As Object) As String
                Dim result As StringBuilder = New StringBuilder("")
                With result
                    If Not IsNothing(obj) Then
                        Dim type As System.Type = obj.GetType()
                        If type.IsValueType OrElse type Is GetType(String) OrElse TypeOf obj Is Nullable Then
                            .Append(obj.ToString())
                        ElseIf type.IsArray Then
                            .Append("[")
                            Dim array As Array = TryCast(obj, System.Array)
                            For i As Integer = 0 To array.Length - 1
                                .Append(CStr(IIf("[".Equals(result), "", ", "))).Append(Utils.toStr(array.GetValue(i)))
                            Next i
                            .Append("]")
                        ElseIf type.IsAssignableFrom(GetType(IEnumerable)) Then
                            .Append("[")
                            Dim list As IEnumerable(Of Object) = TryCast(obj, IEnumerable(Of Object))
                            For Each element As Object In list
                                .Append(CStr(IIf("[".Equals(result), "", ", "))).Append(Utils.toStr(element))
                            Next
                            .Append("]")
                        ElseIf TypeOf obj Is IDictionary(Of String, Object) Then
                            .Append("[")
                            Dim myDictionary As IDictionary(Of String, Object) = CType(obj, IDictionary(Of String, Object))
                            Dim numParam As UInteger = 0UI
                            For Each paramKey As String In myDictionary.Keys
                                If (numParam > 0UI) Then .Append(", ")
                                .Append(paramKey).Append(" = '").Append(Utils.toStr(myDictionary.Item(paramKey))).Append("'")
                                numParam += 1UI
                            Next
                            .Append("]")
                        ElseIf TypeOf obj Is IDictionary(Of Object, Object) Then
                            .Append("[")
                            Dim myDictionary As IDictionary(Of Object, Object) = CType(obj, IDictionary(Of Object, Object))
                            Dim numParam As UInteger = 0UI
                            For Each paramKey As Object In myDictionary.Keys
                                If (numParam > 0UI) Then .Append(", ")
                                .Append(Utils.toStr(paramKey)).Append(" = '").Append(Utils.toStr(myDictionary.Item(paramKey))).Append("'")
                                numParam += 1UI
                            Next
                            .Append("]")
                        ElseIf type.IsClass Then
                            .Append("{")
                            Dim fieldArray As FieldInfo() = type.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
                            'Dim memberArray() As MemberInfo = type.GetMembers(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
                            Dim excludedFieldsArray As String() = {"_size", "_version", "_syncRoot"}
                            For Each field As FieldInfo In fieldArray
                                Dim fieldName As String = field.Name
                                Dim fieldValue As Object = field.GetValue(obj)
                                If Array.TrueForAll(Of String)(excludedFieldsArray, Function(element) CBool(IIf(element.Equals(fieldName), False, True))) Then
                                    .Append(CStr(IIf("_items".Equals(fieldName), Utils.toStr(fieldValue), CStr(IIf("{".Equals(.ToString()), "", ", "))))).Append(fieldName).Append(" = '").Append(Utils.toStr(fieldValue)).Append("'")
                                End If
                            Next
                            .Append("}")
                        Else
                            Try
                                .Append(Utils.coalesce(obj.ToString()))
                            Catch ignoredException As Exception
                                Try
                                    Try
                                        .Append(Utils.coalesce(TryCast(obj, String)))
                                    Catch myException As Exception
                                        .Append(Utils.coalesce(CStr(obj)))
                                    End Try
                                Catch myException As Exception
                                    Throw New ArgumentException("Unknown type object", myException)
                                End Try
                            End Try
                        End If
                    Else
                        .Append("<null>")
                    End If
                    Return .ToString()
                End With
            End Function

            Public Overloads Shared Function toStr(ByVal booleanObject As Boolean) As String
                If ((Not IsNothing(booleanObject)) AndAlso (booleanObject)) Then
                    Return "1"
                Else
                    Return "0"
                End If
            End Function

            Public Overloads Shared Function toStr(ByVal nullableBooleanObject As Boolean?) As String
                If IsNothing(nullableBooleanObject) OrElse Not nullableBooleanObject.HasValue Then
                    Return ""
                Else
                    Return Utils.toStr(nullableBooleanObject.Value)
                End If
            End Function

            Public Overloads Shared Function toStr(ByVal dateObject As DateTime, Optional ByVal dateTimeFormatPattern As String = "yyyy-MM-dd HH:mm:ss") As String
                If IsNothing(dateObject) Then
                    dateObject = CType(getDefaultValue(Of DateTime)(), DateTime)
                ElseIf (Utils.isNullOrEmptyString(dateTimeFormatPattern)) Then
                    Throw New System.ArgumentNullException("Wrong datetime format")
                End If
                Try
                    Dim result As String = dateObject.ToString(dateTimeFormatPattern)
                    Return result
                Catch myException As Exception
                    Throw New FormatException("Error trying to format date and time from a DateTime object", myException)
                End Try
            End Function

            Public Overloads Shared Function toStr(ByVal nullableDatetimeObject As DateTime?, Optional ByVal dateTimeFormatPattern As String = "yyyy-MM-dd HH:mm:ss") As String
                If IsNothing(nullableDatetimeObject) OrElse Not nullableDatetimeObject.HasValue Then
                    Return ""
                Else
                    Return Utils.toStr(nullableDatetimeObject.Value, dateTimeFormatPattern)
                End If
            End Function

            Public Shared Function getDefaultValue(ByVal typeFullName As String) As Object
                If Utils.isNullOrEmptyString(typeFullName) Then
                    Return Nothing
                End If
                Select Case typeFullName
                    Case "System.Boolean"
                        Dim defaultBoolean As Boolean = False
                        Return defaultBoolean
                    Case "System.Byte"
                        Dim defaultByte As Byte = 0
                        Return defaultByte
                    Case "System.Char"
                        Dim defaultChar As Char = Chr(0)
                        Return defaultChar
                    Case "System.Date"
                        Dim defaultDate As Date = Nothing
                        Return defaultDate
                    Case "System.DateTime"
                        Dim defaultDateTime As DateTime = Nothing
                        Return defaultDateTime
                    Case "System.Decimal"
                        Dim defaultDecimal As Decimal = 0D
                        Return defaultDecimal
                    Case "System.Double"
                        Dim defaultDouble As Double = 0D
                        Return defaultDouble
                    Case "System.Single"
                        Dim defaultSingle As Single = 0D
                        Return defaultSingle
                    Case "System.Int16" 'Short
                        Dim defaultShort As Short = 0S
                        Return defaultShort
                    Case "System.Int32" 'Integer
                        Dim defaultInteger As Integer = 0
                        Return defaultInteger
                    Case "System.Int64" 'Long
                        Dim defaultLong As Long = 0L
                        Return defaultLong
                    Case "System.Object"
                        Dim defaultObject As Object = Nothing
                        Return defaultObject
                    Case "System.String"
                        Dim defaultString As String = ""
                        Return defaultString
                    Case "System.UInt16" 'UShort
                        Dim defaultUShort As UShort = 0S
                        Return defaultUShort
                    Case "System.UInt32" 'UInteger
                        Dim defaultUInteger As UInteger = 0
                        Return defaultUInteger
                    Case "System.UInt64" 'ULong
                        Dim defaultULong As ULong = 0L
                        Return defaultULong
                    Case Else
                        Return Nothing
                End Select
            End Function

            Public Shared Function getDefaultValue(Of Type)() As Object
                If GetType(Type) Is Nothing Then Return Nothing
                Dim typeFullName As String = GetType(Type).FullName
                Return Utils.getDefaultValue(typeFullName)
            End Function

            Public Shared Function parseToObject(ByVal value As String, ByVal typeFullName As String) As Object
                If Utils.isNullOrEmptyString(typeFullName) Then Return Nothing
                Select Case typeFullName
                    Case "System.Boolean"
                        Return Utils.parseToObject(Of Boolean)(value)
                    Case "System.Byte"
                        Return Utils.parseToObject(Of Byte)(value)
                    Case "System.Char"
                        Return Utils.parseToObject(Of Char)(value)
                    Case "System.Date"
                        Return Utils.parseToObject(Of Date)(value)
                    Case "System.DateTime"
                        Return Utils.parseToObject(Of DateTime)(value)
                    Case "System.Decimal"
                        Return Utils.parseToObject(Of Decimal)(value)
                    Case "System.Double"
                        Return Utils.parseToObject(Of Double)(value)
                    Case "System.Single"
                        Return Utils.parseToObject(Of Single)(value)
                    Case "System.Int16" 'Short
                        Return Utils.parseToObject(Of Short)(value)
                    Case "System.Int32" 'Integer
                        Return Utils.parseToObject(Of Integer)(value)
                    Case "System.Int64" 'Long
                        Return Utils.parseToObject(Of Long)(value)
                    Case "System.Object"
                        Return Nothing
                    Case "System.String"
                        Return Utils.parseToObject(Of String)(value)
                    Case "System.UInt16" 'UShort
                        Return Utils.parseToObject(Of UShort)(value)
                    Case "System.UInt32" 'UInteger
                        Return Utils.parseToObject(Of UInteger)(value)
                    Case "System.UInt64" 'ULong
                        Return Utils.parseToObject(Of ULong)(value)
                    Case Else
                        Return Nothing
                End Select
            End Function

            Public Shared Function parseToObject(ByVal value As String, ByVal myType As Type) As Object
                myType.GetType()
                If IsNothing(myType) OrElse IsNothing(myType.GetType()) Then Return Nothing
                Dim typeFullName As String = myType.GetType().FullName
                Return Utils.getDefaultValue(typeFullName)
            End Function

            Public Shared Function parseToObject(Of T)(ByVal value As String, Optional ByVal format As String = Nothing) As Object
                If GetType(T) Is Nothing Then Return Nothing
                Dim typeFullName As String = GetType(T).FullName
                If Utils.isNullOrEmptyString(typeFullName) Then Return Nothing
                Select Case typeFullName
                    Case "System.Boolean"
                        Dim booleanValue As Boolean
                        Boolean.TryParse(value, booleanValue)
                        Return booleanValue
                    Case "System.Byte"
                        Dim byteValue As Byte
                        Byte.TryParse(value, byteValue)
                        Return byteValue
                    Case "System.Char"
                        Dim charValue As Char
                        Char.TryParse(value, charValue)
                        Return charValue
                    Case "System.Date"
                        Dim dateValue As Date
                        Dim provider As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
                        Try
                            dateValue = Date.ParseExact(value, format, provider)
                        Catch myDateTimeException As FormatException
                            Date.TryParse(value, dateValue)
                        End Try
                        Return dateValue
                    Case "System.DateTime"
                        Dim dateTimeValue As DateTime
                        Dim provider As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
                        Try
                            Dim _format As String = Utils.coalesce(format)
                            dateTimeValue = DateTime.ParseExact(value, _format, provider)
                        Catch myDateTimeException As FormatException
                            Date.TryParse(value, dateTimeValue)
                        End Try
                        Return dateTimeValue
                    Case "System.Decimal"
                        Dim decimalValue As Decimal
                        Decimal.TryParse(value, decimalValue)
                        Return decimalValue
                    Case "System.Double"
                        Dim doubleValue As Double
                        Double.TryParse(value, doubleValue)
                        Return doubleValue
                    Case "System.Single"
                        Dim singleValue As Single
                        Single.TryParse(value, singleValue)
                        Return singleValue
                    Case "System.Int16" 'Short
                        Dim shortValue As Short
                        Short.TryParse(value, shortValue)
                        Return shortValue
                    Case "System.Int32" 'Integer
                        Dim integerValue As Integer
                        Integer.TryParse(value, integerValue)
                        Return integerValue
                    Case "System.Int64" 'Long
                        Dim longValue As Long
                        Long.TryParse(value, longValue)
                        Return longValue
                    Case "System.Object"
                        Return Nothing
                    Case "System.String"
                        Dim stringValue As String
                        If Utils.isNullOrEmptyString(value) Then
                            stringValue = ""
                        Else
                            stringValue = value
                        End If
                        Return stringValue
                    Case "System.UInt16" 'UShort
                        Dim uShortValue As UShort
                        UShort.TryParse(value, uShortValue)
                        Return uShortValue
                    Case "System.UInt32" 'UInteger
                        Dim uIntegerValue As UInteger
                        UInteger.TryParse(value, uIntegerValue)
                        Return uIntegerValue
                    Case "System.UInt64" 'ULong
                        Dim uLongValue As ULong
                        ULong.TryParse(value, uLongValue)
                        Return uLongValue
                    Case Else
                        Return Nothing
                End Select
            End Function

            Public Shared Function formPath(ByVal ParamArray pathList() As String) As String
                Const PATH_SEPARATOR As String = "/"
                Dim sb As StringBuilder = New StringBuilder
                For index As Integer = 0 To pathList.Length - 1
                    If (index > 0) AndAlso Not pathList(index - 1).EndsWith(PATH_SEPARATOR) Then sb.Append(PATH_SEPARATOR)
                    sb.Append(pathList(index))
                Next index
                Return sb.ToString()
            End Function

            Public Shared Function strILike(ByVal str1 As String, ByVal str2 As String) As Boolean
                Return (Utils.coalesce(str1).Trim().ToUpper() Like Utils.coalesce(str2).Trim().ToUpper())
            End Function

            Public Shared Function strICompare(ByVal str1 As String, ByVal str2 As String) As Integer
                Dim s1 As String = Utils.coalesce(str1).Trim().ToUpper()
                Dim s2 As String = Utils.coalesce(str2).Trim().ToUpper()
                Return String.Compare(s1, s2)
            End Function

            Public Shared Function normalizeString(ByVal str As String) As String
                str = Utils.coalesce(str)
                With str
                    str = Regex.Replace(str, "[‡·‚‰]", "a")
                    str = Regex.Replace(str, "[ËÈÍÎ]", "e")
                    str = Regex.Replace(str, "[ÏÌÓÔ]", "i")
                    str = Regex.Replace(str, "[ÚÛÙˆ]", "o")
                    str = Regex.Replace(str, "[˘˙˚¸]", "u")
                    str = Regex.Replace(str, "[¿¡¬ƒ]", "A")
                    str = Regex.Replace(str, "[»… À]", "E")
                    str = Regex.Replace(str, "[ÃÕŒœ]", "I")
                    str = Regex.Replace(str, "[“”‘÷]", "O")
                    str = Regex.Replace(str, "[Ÿ⁄€‹]", "U")
                    str = Regex.Replace(str, "∑", ".")
                End With
                Return str
            End Function

            Public Shared Function dateTimeToStrAsDate(ByVal dt As DateTime, Optional ByVal dateFormat As String = "dd/MM/yyyy") As String
                If IsNothing(dt) OrElse dt = DateTime.MinValue OrElse Year(dt) = 1 Then
                    Return ""
                Else
                    Try
                        Return Utils.toStr(dt, dateFormat)
                    Catch myException As Exception
                        Return ""
                    End Try
                End If
            End Function

            Public Shared Function getTimeDescription(ByVal hours As UShort, ByVal minutes As UShort, ByVal seconds As UShort) As String
                Dim s As UShort = seconds Mod 60US
                Dim m As UShort = (minutes + (seconds \ 60US)) Mod 60US
                Dim h As UShort = (hours + (minutes + (seconds \ 60US)) \ 60US)
                Dim hStr As String = Utils.coalesce(Utils.toStr(h), "0")
                Dim mStr As String = Utils.coalesce(m.ToString("D2"), "00")
                Dim sStr As String = Utils.coalesce(s.ToString("D2"), "00")
                Return String.Format("{0}h{1}m{2}s", hStr, mStr, sStr)
            End Function

            Public Shared Function getFileBaseName(ByVal filePath As String, Optional ByVal withExtension As Boolean = True) As String
                Dim fileBaseName As String = Regex.Replace(Utils.coalesce(filePath).Replace("\"c, "/"c), "^.*?([^/]+)$", "$1")
                If Not (IsNothing(withExtension) OrElse withExtension) Then fileBaseName = Regex.Replace(fileBaseName, "\.[^\.]+$", "")
                Return fileBaseName
            End Function

            Public Shared Function getFileExtension(ByVal filePath As String) As String
                Dim fileExtension As String = ""
                filePath = Utils.coalesce(filePath).Trim()
                If filePath.Contains(".") AndAlso Not filePath.EndsWith(".") Then
                    fileExtension = Regex.Replace(filePath, "^.*\.([^\.]+)$", "$1")
                End If
                Return fileExtension
            End Function

            Public Shared Function indentXML(ByVal xml As String, Optional ByVal offset As Integer = 0) As String
                Const TAB As String = vbTab
                Const CRLF As String = vbCrLf
                Dim xmlIndentedEntryList As IList(Of String) = New List(Of String)()
                If Not Utils.isNullOrEmptyString(xml) Then
                    Dim xmlTagEntriesArray As String() = xml.Split("<"c)
                    Dim numTabs As Integer = offset
                    For Each xmlTagEntry As String In xmlTagEntriesArray
                        If Not Utils.isNullOrEmptyString(xmlTagEntry) Then
                            xmlTagEntry = "<" & xmlTagEntry
                            Dim _offset As Integer = 0
                            If xmlTagEntry.StartsWith("</") Then
                                numTabs -= 1
                                _offset = -1
                            End If
                            For _index As Integer = 0 To numTabs - 1
                                xmlTagEntry = TAB & xmlTagEntry
                            Next
                            If Not xmlTagEntry.EndsWith("/>") Then
                                numTabs += 1
                            End If
                            numTabs += _offset
                            xmlIndentedEntryList.Add(xmlTagEntry)
                        End If
                    Next
                End If
                Dim result As String = String.Join(CRLF, xmlIndentedEntryList)
                result = Regex.Replace(result, "([^>\t\n\r])[\t\n\r]+", "$1")
                Return result
            End Function

            Public Shared Function resumeString(ByVal str As String, ByVal maxLength As UInteger, Optional ByVal addResumeTerminator As Boolean = True) As String
                Const RESUME_TERMINATOR As String = "..."
                Dim result As String = Utils.coalesce(str)
                If result.Length > maxLength Then
                    If addResumeTerminator Then
                        If RESUME_TERMINATOR.Length < maxLength Then
                            maxLength -= CUInt(RESUME_TERMINATOR.Length)
                            result = result.Substring(0, CInt(maxLength)) & RESUME_TERMINATOR
                        Else
                            result = RESUME_TERMINATOR.Substring(0, CInt(maxLength))
                        End If
                    Else
                        result = result.Substring(0, CInt(maxLength))
                    End If
                End If
                Return result
            End Function

            Public Overloads Shared Function equals(Of T)(ByVal arg1 As T, ByVal arg2 As T) As Boolean
                If IsNothing(arg1) Then
                    If IsNothing(arg2) Then Return True Else Return False
                Else
                    Return arg1.Equals(arg2)
                End If
            End Function

            Public Overloads Shared Function compare(Of T As IComparable)(ByVal arg1 As T, ByVal arg2 As T) As Integer
                If IsNothing(arg1) Then
                    If IsNothing(arg2) Then Return 0 Else Return 1
                Else
                    Return arg1.CompareTo(arg2)
                End If
            End Function

            Public Overloads Shared Function compare(Of T As {Structure, IComparable})(ByVal arg1 As T?, ByVal arg2 As T?) As Integer
                If IsNothing(arg1) Then
                    If IsNothing(arg2) Then Return 0 Else Return 1
                ElseIf IsNothing(arg2) Then
                    Return -1
                Else
                    Return Utils.compare(Of T)(arg1.Value, arg2.Value)
                End If
            End Function

#End Region 'Public methods

        End Class

    End Namespace
End Namespace