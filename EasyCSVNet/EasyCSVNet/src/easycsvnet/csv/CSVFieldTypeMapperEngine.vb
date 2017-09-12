#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.Globalization

Imports EasyCSVNet.easycsvnet.csv.import
Imports EasyCSVNet.easycsvnet.csv.export
Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace easycsvnet
    Namespace csv

        Public Class CSVFieldTypeMapperEngine : Implements IStringToTypeMapper, ITypeToStringMapper

#Region "Public attributes"

            Public Shared CULTURE_INFO As CultureInfo = New CultureInfo("en-US", False)

#End Region 'Public attributes

#Region "Private attributes"

            Private _typeMapperTable As IDictionary(Of Type, IStringToObjectMapper) = Nothing

#End Region 'Private attributes

#Region "Public inner classes"

            Public MustInherit Class GenericDefaultCSVFieldTypeMapper : Implements IStringToObjectMapper

                Protected MustOverride Function mapFromStringToType(strValue As String) As Object Implements IStringToObjectMapper.mapFromStringToType

                Protected MustOverride Function mapFromTypeToString(objValue As Object) As String Implements IStringToObjectMapper.mapFromTypeToString

                Protected Overridable Function beforeMappingStringCallbackHook(ByVal strValue As String) As String
                    Return strValue
                End Function

                Protected Overridable Function afterMappingStringCallbackHook(ByVal strValue As String) As String
                    Return strValue
                End Function

                Protected Overridable Function beforeMappingObjectCallbackHook(ByVal objValue As Object) As Object
                    Return objValue
                End Function

                Protected Overridable Function afterMappingObjectCallbackHook(ByVal objValue As Object) As Object
                    Return objValue
                End Function

            End Class

#Region "DefaultCSVFieldTypeMappers"

            Public Class DefaultCSVFieldTypeMapper_NullableWrapper(Of T As Structure) : Inherits GenericDefaultCSVFieldTypeMapper

                Private _baseMapper As IStringToObjectMapper = Nothing

                Public Sub New(ByVal baseMapper As IStringToObjectMapper)
                    MyBase.New()
                    Me._baseMapper = baseMapper
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CType(Me._baseMapper.mapFromStringToType(Me.beforeMappingStringCallbackHook(strValue)), Nullable(Of T)))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    If IsNothing(objValue) Then
                        Return Me.afterMappingStringCallbackHook(Me._baseMapper.mapFromTypeToString(Nothing))
                    Else
                        Return Me.afterMappingStringCallbackHook(Me._baseMapper.mapFromTypeToString(CType(Me.beforeMappingObjectCallbackHook(objValue), Nullable(Of T)).Value))
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_DateTime : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_DATETIMEFORMAT_PATTERN As String = "dd/MM/yyyy HH:mm:ss" 'Default value

                Private _dateTimeFormatPattern As String = DEFAULT_DATETIMEFORMAT_PATTERN

                Protected Property dateTimeFormatPattern As String
                    Get
                        Return Me._dateTimeFormatPattern
                    End Get
                    Set(dateTimeFormatPattern As String)
                        Me._dateTimeFormatPattern = dateTimeFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _dateTimeFormatPattern As String = DEFAULT_DATETIMEFORMAT_PATTERN)
                    Me.dateTimeFormatPattern = _dateTimeFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return CObj(Me.afterMappingObjectCallbackHook(Date.ParseExact(Utils.coalesce(Me.beforeMappingStringCallbackHook(strValue)), Me.dateTimeFormatPattern(), Nothing)))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is DateTime) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        With CType(objValue, DateTime)
                            Return Me.afterMappingStringCallbackHook(String.Format("{0}/{1}/{2} {3}:{4}:{5}", .Day, .Month, .Year + 1900, .Hour, .Minute, .Second))
                        End With
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableDateTime : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of DateTime)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_DateTime())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_Boolean : Inherits GenericDefaultCSVFieldTypeMapper

                Private _booleanTruePatternList As IList(Of String) = New List(Of String)({"^" & Boolean.TrueString & "$"}) 'Default

                Protected Property booleanTruePatternList As IList(Of String)
                    Get
                        Return Me._booleanTruePatternList
                    End Get
                    Set(booleanTruePatternList As IList(Of String))
                        If IsNothing(booleanTruePatternList) Then
                            Me._booleanTruePatternList = New List(Of String)()
                        Else
                            Me._booleanTruePatternList = booleanTruePatternList
                        End If
                    End Set
                End Property

                Public Sub New(Optional ByVal _booleanTruePatternList As IList(Of String) = Nothing)
                    If Not IsNothing(_booleanTruePatternList) Then Me.booleanTruePatternList = _booleanTruePatternList
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    strValue = Me.beforeMappingStringCallbackHook(strValue)
                    For Each booleanTruePattern As String In booleanTruePatternList
                        Dim booleanTrueRegExp As Text.RegularExpressions.Regex = New Text.RegularExpressions.Regex(booleanTruePattern)
                        If Text.RegularExpressions.Regex.IsMatch(strValue, booleanTruePattern, Text.RegularExpressions.RegexOptions.IgnoreCase) Then Return CObj(Me.beforeMappingObjectCallbackHook(True))
                    Next
                    Return CObj(Me.beforeMappingObjectCallbackHook(False))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Boolean) OrElse Not CType(objValue, Boolean) Then
                        Return Me.afterMappingStringCallbackHook(Boolean.FalseString)
                    Else
                        Return Me.afterMappingStringCallbackHook(Boolean.TrueString)
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableBoolean : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Boolean)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Boolean())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_Byte : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_BYTEFORMAT_PATTERN As String = "F" 'Default value

                Private _byteFormatPattern As String = DEFAULT_BYTEFORMAT_PATTERN

                Protected Property byteFormatPattern As String
                    Get
                        Return Me._byteFormatPattern
                    End Get
                    Set(byteFormatPattern As String)
                        Me._byteFormatPattern = byteFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _byteFormatPattern As String = DEFAULT_BYTEFORMAT_PATTERN)
                    Me.byteFormatPattern = _byteFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToByte(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Byte) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CByte(objValue).ToString(Me._byteFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableByte : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Byte)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Byte())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_Decimal : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_DECIMALFORMAT_PATTERN As String = "F" 'Default value

                Private _decimalFormatPattern As String = DEFAULT_DECIMALFORMAT_PATTERN

                Protected Property decimalFormatPattern As String
                    Get
                        Return Me._decimalFormatPattern
                    End Get
                    Set(decimalFormatPattern As String)
                        Me._decimalFormatPattern = decimalFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _decimalFormatPattern As String = DEFAULT_DECIMALFORMAT_PATTERN)
                    Me.decimalFormatPattern = _decimalFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToDecimal(Utils.coalesce(Me.beforeMappingStringCallbackHook(strValue)).Replace(".", ","))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Decimal) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CDec(objValue).ToString(Me._decimalFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableDecimal : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Decimal)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Decimal())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_Double : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_DOUBLEFORMAT_PATTERN As String = "F" 'Default value

                Private _doubleFormatPattern As String = DEFAULT_DOUBLEFORMAT_PATTERN

                Protected Property doubleFormatPattern As String
                    Get
                        Return Me._doubleFormatPattern
                    End Get
                    Set(doubleFormatPattern As String)
                        Me._doubleFormatPattern = doubleFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _doubleFormatPattern As String = DEFAULT_DOUBLEFORMAT_PATTERN)
                    Me.doubleFormatPattern = _doubleFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToDouble(Utils.coalesce(Me.beforeMappingStringCallbackHook(strValue)).Replace(".", ","))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Double) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CDbl(objValue).ToString(Me._doubleFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableDouble : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Double)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Double())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_Single : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_SINGLEFORMAT_PATTERN As String = "F" 'Default value

                Private _singleFormatPattern As String = DEFAULT_SINGLEFORMAT_PATTERN

                Protected Property singleFormatPattern As String
                    Get
                        Return Me._singleFormatPattern
                    End Get
                    Set(singleFormatPattern As String)
                        Me._singleFormatPattern = singleFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _singleFormatPattern As String = DEFAULT_SINGLEFORMAT_PATTERN)
                    Me.singleFormatPattern = _singleFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToSingle(Utils.coalesce(Me.beforeMappingStringCallbackHook(strValue)).Replace(".", ","))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Single) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CSng(objValue).ToString(Me._singleFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableSingle : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Single)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Single())
                End Sub

            End Class

            ' System.Int16
            Public Class DefaultCSVFieldTypeMapper_Short : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_SHORTFORMAT_PATTERN As String = "F" 'Default value

                Private _shortFormatPattern As String = DEFAULT_SHORTFORMAT_PATTERN

                Protected Property shortFormatPattern As String
                    Get
                        Return Me._shortFormatPattern
                    End Get
                    Set(shortFormatPattern As String)
                        Me._shortFormatPattern = shortFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _shortFormatPattern As String = DEFAULT_SHORTFORMAT_PATTERN)
                    Me.shortFormatPattern = _shortFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToInt16(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Short) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CShort(objValue).ToString(Me._shortFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            ' System.Int16
            Public Class DefaultCSVFieldTypeMapper_NullableShort : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Short)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Short())
                End Sub

            End Class

            ' System.Int32
            Public Class DefaultCSVFieldTypeMapper_Integer : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_INTEGERFORMAT_PATTERN As String = "F" 'Default value

                Private _integerFormatPattern As String = DEFAULT_INTEGERFORMAT_PATTERN

                Protected Property integerFormatPattern As String
                    Get
                        Return Me._integerFormatPattern
                    End Get
                    Set(integerFormatPattern As String)
                        Me._integerFormatPattern = integerFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _integerFormatPattern As String = DEFAULT_INTEGERFORMAT_PATTERN)
                    Me.integerFormatPattern = _integerFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToInt32(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Integer) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CInt(objValue).ToString(Me._integerFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            ' System.Int32
            Public Class DefaultCSVFieldTypeMapper_NullableInteger : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Integer)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Integer())
                End Sub

            End Class

            ' System.Int64
            Public Class DefaultCSVFieldTypeMapper_Long : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_LONGFORMAT_PATTERN As String = "F" 'Default value

                Private _longFormatPattern As String = DEFAULT_LONGFORMAT_PATTERN

                Protected Property longFormatPattern As String
                    Get
                        Return Me._longFormatPattern
                    End Get
                    Set(longFormatPattern As String)
                        Me._longFormatPattern = longFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _longFormatPattern As String = DEFAULT_LONGFORMAT_PATTERN)
                    Me.longFormatPattern = _longFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToInt64(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Long) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CLng(objValue).ToString(Me._longFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            ' System.Int64
            Public Class DefaultCSVFieldTypeMapper_NullableLong : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Long)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Long())
                End Sub

            End Class

            ' System.UInt16
            Public Class DefaultCSVFieldTypeMapper_UShort : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_USHORTFORMAT_PATTERN As String = "F" 'Default value

                Private _ushortFormatPattern As String = DEFAULT_USHORTFORMAT_PATTERN

                Protected Property ushortFormatPattern As String
                    Get
                        Return Me._ushortFormatPattern
                    End Get
                    Set(ushortFormatPattern As String)
                        Me._ushortFormatPattern = ushortFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _ushortFormatPattern As String = DEFAULT_USHORTFORMAT_PATTERN)
                    Me.ushortFormatPattern = _ushortFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToUInt16(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is UShort) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CUShort(objValue).ToString(Me._ushortFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            ' System.UInt16
            Public Class DefaultCSVFieldTypeMapper_NullableUShort : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of UShort)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_UShort())
                End Sub

            End Class

            ' System.UInt32
            Public Class DefaultCSVFieldTypeMapper_UInteger : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_UINTEGERFORMAT_PATTERN As String = "F" 'Default value

                Private _uintegerFormatPattern As String = DEFAULT_UINTEGERFORMAT_PATTERN

                Protected Property uintegerFormatPattern As String
                    Get
                        Return Me._uintegerFormatPattern
                    End Get
                    Set(uintegerFormatPattern As String)
                        Me._uintegerFormatPattern = uintegerFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _uintegerFormatPattern As String = DEFAULT_UINTEGERFORMAT_PATTERN)
                    Me.uintegerFormatPattern = _uintegerFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToUInt32(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is UInteger) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CUInt(objValue).ToString(Me._uintegerFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            ' System.UInt32
            Public Class DefaultCSVFieldTypeMapper_NullableUInteger : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of UInteger)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_UInteger())
                End Sub

            End Class

            ' System.UInt64
            Public Class DefaultCSVFieldTypeMapper_ULong : Inherits GenericDefaultCSVFieldTypeMapper

                Public Const DEFAULT_ULONGFORMAT_PATTERN As String = "F" 'Default value

                Private _ulongFormatPattern As String = DEFAULT_ULONGFORMAT_PATTERN

                Protected Property ulongFormatPattern As String
                    Get
                        Return Me._ulongFormatPattern
                    End Get
                    Set(ulongFormatPattern As String)
                        Me._ulongFormatPattern = ulongFormatPattern
                    End Set
                End Property

                Public Sub New(Optional ByVal _ulongFormatPattern As String = DEFAULT_ULONGFORMAT_PATTERN)
                    Me.ulongFormatPattern = _ulongFormatPattern
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToUInt64(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is ULong) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CLng(objValue).ToString(Me._ulongFormatPattern, CULTURE_INFO))
                    End If
                End Function

            End Class

            ' System.UInt64
            Public Class DefaultCSVFieldTypeMapper_NullableULong : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of ULong)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_ULong())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_Char : Inherits GenericDefaultCSVFieldTypeMapper

                Public Sub New()
                    'Do nothing
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToChar(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is Char) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CChar(objValue).ToString())
                    End If
                End Function

            End Class

            Public Class DefaultCSVFieldTypeMapper_NullableChar : Inherits DefaultCSVFieldTypeMapper_NullableWrapper(Of Char)

                Public Sub New()
                    MyBase.New(New DefaultCSVFieldTypeMapper_Char())
                End Sub

            End Class

            Public Class DefaultCSVFieldTypeMapper_String : Inherits GenericDefaultCSVFieldTypeMapper

                Public Sub New()
                    'Do nothing
                End Sub

                Protected Overrides Function mapFromStringToType(strValue As String) As Object
                    Return Me.afterMappingObjectCallbackHook(CObj(Convert.ToString(Me.beforeMappingStringCallbackHook(strValue))))
                End Function

                Protected Overrides Function mapFromTypeToString(objValue As Object) As String
                    objValue = Me.beforeMappingObjectCallbackHook(objValue)
                    If IsNothing(objValue) OrElse Not (TypeOf objValue Is String) Then
                        Return Me.afterMappingStringCallbackHook(String.Empty)
                    Else
                        Return Me.afterMappingStringCallbackHook(CStr(objValue).ToString())
                    End If
                End Function

            End Class

#End Region 'DefaultCSVFieldTypeMappers

#End Region 'Public inner classes

#Region "Public methods"

#Region "Constructors"

            Public Sub New()
                With Me
                    ._typeMapperTable = New Dictionary(Of Type, IStringToObjectMapper)()
                    .registerDefaultTypeMappers()
                End With
            End Sub

#End Region 'Constructors

#Region "Properties (getters & setters)"

            Public ReadOnly Property registeredTypesCollection As ICollection(Of Type)
                Get
                    Return Me._typeMapperTable.Keys
                End Get
            End Property

#End Region 'Properties (getters & setters)

#Region "IStringToTypeMapper interface implementation"

            Public Overloads Function mapTo(Of T)(ByVal strValue As String) As T Implements IStringToTypeMapper.mapTo
                Return CType(Me.mapTo(GetType(T), strValue), T)
            End Function

            Public Overloads Function mapTo(ByVal _type As Type, ByVal strValue As String) As Object Implements IStringToTypeMapper.mapTo
                If Me.registeredTypesCollection.Contains(_type) Then
                    Return CTypeDynamic(Me._typeMapperTable.Item(_type).mapFromStringToType(strValue), _type)
                Else
                    Throw New ArgumentException(String.Format("No suitable CSVFieldTypeMapperIfc instance found for unregistered type '{0}'", _type.FullName))
                End If
            End Function

            Public Overloads Function mapTo(ByVal typeFullName As String, ByVal strValue As String) As Object Implements IStringToTypeMapper.mapTo
                For Each registeredType As Type In Me.registeredTypesCollection
                    If registeredType.FullName = typeFullName Then Return Me.mapTo(registeredType, strValue)
                Next
                Throw New ArgumentException(String.Format("No suitable CSVFieldTypeMapperIfc instance found for unregistered type '{0}'", typeFullName))
            End Function

#End Region 'IStringToTypeMapper interface implementation

#Region "ITypeToStringMapper interface implementation"

            Public Function mapToString(ByVal obj As Object) As String Implements ITypeToStringMapper.mapToString
                Dim _type As Type = ReflectionUtils.getTypeOf(obj)
                If Me.registeredTypesCollection.Contains(_type) Then
                    Return Me._typeMapperTable.Item(_type).mapFromTypeToString(obj)
                Else
                    Throw New ArgumentException(String.Format("No suitable CSVFieldTypeMapperIfc instance found for unregistered type '{0}'", _type.FullName))
                End If
            End Function

#End Region 'ITypeToStringMapper interface implementation

            Public Sub registerTypeMapper(Of T)(ByVal typeMapper As IStringToObjectMapper)
                Dim _type As Type = GetType(T)
                If Me._typeMapperTable.ContainsKey(_type) Then Me._typeMapperTable.Remove(_type)
                Me._typeMapperTable.Add(_type, CType(typeMapper, IStringToObjectMapper))
            End Sub

#End Region 'Public methods

#Region "Protected methods"

            Protected Sub registerDefaultTypeMappers()
                With Me._typeMapperTable
                    .Add(GetType(DateTime), CType(New DefaultCSVFieldTypeMapper_DateTime(), IStringToObjectMapper))
                    .Add(GetType(DateTime?), CType(New DefaultCSVFieldTypeMapper_NullableDateTime(), IStringToObjectMapper))
                    .Add(GetType(Boolean), CType(New DefaultCSVFieldTypeMapper_Boolean(), IStringToObjectMapper))
                    .Add(GetType(Boolean?), CType(New DefaultCSVFieldTypeMapper_NullableBoolean(), IStringToObjectMapper))
                    .Add(GetType(Byte), CType(New DefaultCSVFieldTypeMapper_Byte(), IStringToObjectMapper))
                    .Add(GetType(Byte?), CType(New DefaultCSVFieldTypeMapper_NullableByte(), IStringToObjectMapper))
                    .Add(GetType(Decimal), CType(New DefaultCSVFieldTypeMapper_Decimal(), IStringToObjectMapper))
                    .Add(GetType(Decimal?), CType(New DefaultCSVFieldTypeMapper_NullableDecimal(), IStringToObjectMapper))
                    .Add(GetType(Double), CType(New DefaultCSVFieldTypeMapper_Double(), IStringToObjectMapper))
                    .Add(GetType(Double?), CType(New DefaultCSVFieldTypeMapper_NullableDouble(), IStringToObjectMapper))
                    .Add(GetType(Single), CType(New DefaultCSVFieldTypeMapper_Single(), IStringToObjectMapper))
                    .Add(GetType(Single?), CType(New DefaultCSVFieldTypeMapper_NullableSingle(), IStringToObjectMapper))
                    .Add(GetType(Short), CType(New DefaultCSVFieldTypeMapper_Short(), IStringToObjectMapper))
                    .Add(GetType(Short?), CType(New DefaultCSVFieldTypeMapper_NullableShort(), IStringToObjectMapper))
                    .Add(GetType(Integer), CType(New DefaultCSVFieldTypeMapper_Integer(), IStringToObjectMapper))
                    .Add(GetType(Integer?), CType(New DefaultCSVFieldTypeMapper_NullableInteger(), IStringToObjectMapper))
                    .Add(GetType(Long), CType(New DefaultCSVFieldTypeMapper_Long(), IStringToObjectMapper))
                    .Add(GetType(Long?), CType(New DefaultCSVFieldTypeMapper_NullableLong(), IStringToObjectMapper))
                    .Add(GetType(UShort), CType(New DefaultCSVFieldTypeMapper_UShort(), IStringToObjectMapper))
                    .Add(GetType(UShort?), CType(New DefaultCSVFieldTypeMapper_NullableUShort(), IStringToObjectMapper))
                    .Add(GetType(UInteger), CType(New DefaultCSVFieldTypeMapper_UInteger(), IStringToObjectMapper))
                    .Add(GetType(UInteger?), CType(New DefaultCSVFieldTypeMapper_NullableUInteger(), IStringToObjectMapper))
                    .Add(GetType(ULong), CType(New DefaultCSVFieldTypeMapper_ULong(), IStringToObjectMapper))
                    .Add(GetType(ULong?), CType(New DefaultCSVFieldTypeMapper_NullableULong(), IStringToObjectMapper))
                    .Add(GetType(Char), CType(New DefaultCSVFieldTypeMapper_Char(), IStringToObjectMapper))
                    .Add(GetType(Char?), CType(New DefaultCSVFieldTypeMapper_NullableChar(), IStringToObjectMapper))
                    .Add(GetType(String), CType(New DefaultCSVFieldTypeMapper_String(), IStringToObjectMapper))
                End With
            End Sub

#End Region 'Protected methods

        End Class

    End Namespace
End Namespace