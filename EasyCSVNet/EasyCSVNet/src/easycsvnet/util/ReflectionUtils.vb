#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.Reflection

Imports EasyCSVNet.easycsvnet.csv.import

#End Region 'Imports

Namespace easycsvnet
    Namespace util

        Public Class ReflectionUtils

#Region "Private methods"

#Region "Constructors"

            Private Sub New()
                'Static class
            End Sub

#End Region 'Constructors

#End Region 'Private methods

#Region "Public methods"

            Public Shared Function instantiateObject(Of T)() As Object
                Dim objectType As Type = GetType(T)
                Dim objectConstructor As ConstructorInfo = objectType.GetConstructor(Type.EmptyTypes)
                Dim objectClassObject As Object = objectConstructor.Invoke({})
                Return objectClassObject
            End Function

            Public Shared Function initializeObjectProperty(Of T)(ByRef objectClassObject As Object, ByVal propertyName As String, ByVal strPropertyValue As String, Optional ByVal strictTypeCast As Boolean = True, Optional fieldTypeMapperEngine As IStringToTypeMapper = Nothing, Optional ByRef fieldType As Type = Nothing) As Type
                If IsNothing(objectClassObject) Then
                    Throw New EvaluateException("Cannot reflexively initialize property for a null class object passed")
                ElseIf Utils.isNullOrEmptyString(propertyName) Then
                    Throw New EvaluateException("Cannot reflexively initialize null property passed")
                Else
                    Dim objectType As Type = GetType(T)
                    Dim getterMethodName As String = "get" & propertyName
                    Dim setterMethodName As String = "set" & propertyName
                    Dim setterMethodFound As Boolean = False
                    Dim setterMethodArgType As Type = Nothing
                    Dim setterMethodArgTypeFullName As String = Nothing
                    Dim propertyInfoArray As PropertyInfo() = GetType(T).GetProperties()
                    For Each propertyInfo As PropertyInfo In propertyInfoArray
                        Dim methodInfoArray As MethodInfo() = propertyInfo.GetAccessors()
                        For Each methodInfo As MethodInfo In methodInfoArray
                            If (getterMethodName = methodInfo.Name) Then
                                setterMethodArgType = methodInfo.ReturnType
                                fieldType = setterMethodArgType
                                setterMethodArgTypeFullName = methodInfo.ReturnType.FullName
                            ElseIf (setterMethodName = methodInfo.Name) Then
                                setterMethodFound = True
                            End If
                        Next
                    Next
                    If IsNothing(setterMethodArgType) Then setterMethodFound = False
                    If (setterMethodFound) Then
                        Dim objectSetterMethod As MethodInfo = objectType.GetMethod(setterMethodName)
                        Dim objPropertyValue As Object
                        If (Utils.isNullOrEmptyString(strPropertyValue)) Then
                            objPropertyValue = Utils.getDefaultValue(setterMethodArgTypeFullName)
                        ElseIf Not strictTypeCast Then
                            objPropertyValue = strPropertyValue
                        Else
                            If IsNothing(fieldTypeMapperEngine) Then
                                objPropertyValue = Utils.parseToObject(strPropertyValue, setterMethodArgTypeFullName)
                            Else
                                objPropertyValue = fieldTypeMapperEngine.mapTo(setterMethodArgType, strPropertyValue)
                                Utils.parseToObject(strPropertyValue, setterMethodArgTypeFullName)
                            End If
                        End If
                        objectSetterMethod.Invoke(objectClassObject, {CTypeDynamic(objPropertyValue, setterMethodArgType)})
                    Else
                        Throw New EvaluateException(String.Format("Cannot reflexively initialize property '{0}' for object type '{1}'", propertyName, setterMethodArgTypeFullName))
                    End If
                    objectClassObject = CType(objectClassObject, T)
                    Return setterMethodArgType
                End If
            End Function

            'Not used
            Public Shared Function getObjectPropertyValue(Of T_inObject, T_outProperty)(ByVal objectClassObject As T_inObject, ByVal propertyName As String) As T_outProperty
                If IsNothing(objectClassObject) Then
                    Throw New EvaluateException("Cannot reflexively initialize property for a null class object passed")
                ElseIf Utils.isNullOrEmptyString(propertyName) Then
                    Throw New EvaluateException("Cannot reflexively initialize null property passed")
                Else
                    Dim objectType As Type = GetType(T_inObject)
                    Dim getterMethodName As String = "get" & propertyName
                    Dim getterMethodFound As Boolean = False
                    Dim getterMethodArgType As Type = Nothing
                    Dim getterMethodArgTypeFullName As String = Nothing
                    Dim propertyInfoArray As PropertyInfo() = GetType(T_inObject).GetProperties()
                    For Each propertyInfo As PropertyInfo In propertyInfoArray
                        Dim methodInfoArray As MethodInfo() = propertyInfo.GetAccessors()
                        For Each methodInfo As MethodInfo In methodInfoArray
                            If (getterMethodName = methodInfo.Name) Then
                                getterMethodArgType = methodInfo.ReturnType
                                getterMethodArgTypeFullName = methodInfo.ReturnType.FullName
                                getterMethodFound = True
                            End If
                        Next
                    Next
                    If IsNothing(getterMethodArgType) Then getterMethodFound = False
                    If (getterMethodFound) Then
                        Dim objectGetterMethod As MethodInfo = objectType.GetMethod(getterMethodName)
                        Try
                            Return (CType(objectGetterMethod.Invoke(objectClassObject, {}), T_outProperty))
                        Catch myException As Exception
                            Throw New EvaluateException(String.Format("Cannot cast property '{0}' result from type '{1}' into output type '{2}'", propertyName, getterMethodArgTypeFullName, GetType(T_outProperty).FullName))
                        End Try
                    Else
                        Throw New EvaluateException(String.Format("Cannot reflexively initialize property '{0}' for object type '{1}'", propertyName, getterMethodArgTypeFullName))
                    End If
                End If
            End Function

            Public Shared Function getTypeOf(ByVal obj As Object) As Type
                If TypeOf obj Is Boolean Then Return GetType(Boolean)
                If TypeOf obj Is Boolean? Then Return GetType(Boolean?)
                If TypeOf obj Is Byte Then Return GetType(Byte)
                If TypeOf obj Is Byte? Then Return GetType(Byte?)
                If TypeOf obj Is Char Then Return GetType(Char)
                If TypeOf obj Is Char? Then Return GetType(Char?)
                If TypeOf obj Is DateTime Then Return GetType(DateTime)
                If TypeOf obj Is DateTime? Then Return GetType(DateTime?)
                If TypeOf obj Is Decimal Then Return GetType(Decimal)
                If TypeOf obj Is Decimal? Then Return GetType(Decimal?)
                If TypeOf obj Is Double Then Return GetType(Double)
                If TypeOf obj Is Double? Then Return GetType(Double?)
                If TypeOf obj Is Single Then Return GetType(Single)
                If TypeOf obj Is Single? Then Return GetType(Single?)
                If TypeOf obj Is Int16 Then Return GetType(Int16) 'Short
                If TypeOf obj Is Int16? Then Return GetType(Int16?) 'Short?
                If TypeOf obj Is Int32 Then Return GetType(Int32) 'Integer
                If TypeOf obj Is Int32? Then Return GetType(Int32?) 'Integer?
                If TypeOf obj Is Int64 Then Return GetType(Int64) 'Long
                If TypeOf obj Is Int64? Then Return GetType(Int64?) 'Long?
                If TypeOf obj Is String Then Return GetType(String)
                If TypeOf obj Is UInt16 Then Return GetType(UInt16) 'UShort
                If TypeOf obj Is UInt16? Then Return GetType(UInt16) 'UShort?
                If TypeOf obj Is UInt32 Then Return GetType(UInt32) 'UInteger
                If TypeOf obj Is UInt32? Then Return GetType(UInt32) 'UInteger?
                If TypeOf obj Is UInt64 Then Return GetType(UInt64) 'ULong
                If TypeOf obj Is UInt64? Then Return GetType(UInt64) 'ULong?
                Return GetType(Object)
            End Function

#End Region 'Public methods

        End Class

    End Namespace
End Namespace