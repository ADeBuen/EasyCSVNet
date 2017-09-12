#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports System.Reflection

#End Region 'Imports

Namespace easycsvnet
    Namespace util

        Public Class ObjectCloner

#Region "Private methods"

#Region "Constructors"

            Private Sub New()
                'Static class
            End Sub

#End Region 'Constructors

            Private Shared Function isTransientAttributeType(ByVal type As Type) As Boolean
                If Not IsNothing(type) Then
                    If GetType(System.NonSerializedAttribute).Equals(type) Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return True
                End If
            End Function

            Private Shared Function _clone(ByVal obj As Object, Optional ByVal omitTransientAttributes As Boolean = True) As Object
                If obj Is Nothing Then Return Nothing
                Dim type As System.Type = obj.GetType()
                If type.IsValueType OrElse type Is GetType(String) Then
                    Return obj
                ElseIf type.IsArray Then
                    Dim typeElement As System.Type = type.GetType(type.FullName.Replace("[]", String.Empty))
                    Dim array As System.Array = TryCast(obj, System.Array)
                    Dim copiedArray As System.Array = System.Array.CreateInstance(typeElement, array.Length)
                    For i As Integer = 0 To array.Length - 1
                        copiedArray.SetValue(_clone(array.GetValue(i)), i)
                    Next i
                    Return copiedArray
                ElseIf type.IsClass Then
                    Dim copiedObject As Object = Activator.CreateInstance(obj.GetType())
                    Dim fieldArray As FieldInfo() = type.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
                    Dim memberArray As MemberInfo() = type.GetMembers(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
                    For Each field As FieldInfo In fieldArray
                        Dim serializable As Boolean = True
                        If omitTransientAttributes Then
                            Dim publicPropertyName As String = CType(IIf(field.Name.StartsWith("_"), field.Name.Substring(1), field.Name), String)
                            Dim iterMember As IEnumerator = memberArray.GetEnumerator()
                            Dim memberFound As MemberInfo = Nothing
                            While (IsNothing(memberFound) AndAlso iterMember.MoveNext)
                                Dim thisMember As MemberInfo = CType(iterMember.Current, MemberInfo)
                                If (publicPropertyName.Equals(thisMember.Name)) Then memberFound = thisMember
                            End While
                            If Not IsNothing(memberFound) Then
                                Dim _attributeArray As Object() = memberFound.GetCustomAttributes(False)
                                Dim iterAttribute As IEnumerator = _attributeArray.GetEnumerator()
                                While (serializable AndAlso iterAttribute.MoveNext)
                                    Dim attribute As Attribute = CType(iterAttribute.Current, Attribute)
                                    If (isTransientAttributeType(attribute.GetType)) Then serializable = False
                                End While
                            End If
                        End If
                        If (serializable) Then
                            Dim fieldValue As Object = field.GetValue(obj)
                            If Not IsNothing(fieldValue) Then field.SetValue(copiedObject, _clone(fieldValue))
                        End If
                    Next
                    Return copiedObject
                Else
                    Throw New ArgumentException("Unknown type object")
                End If
            End Function

#End Region 'Private methods

#Region "Public methods"

            Public Shared Function clone(Of T)(ByVal obj As T, Optional ByVal omitTransientAttributes As Boolean = True) As T
                If obj Is Nothing Then Throw New ArgumentNullException("Object is null")
                Return CType(_clone(obj, omitTransientAttributes), T)
            End Function

#End Region 'Public methods

        End Class

    End Namespace
End Namespace