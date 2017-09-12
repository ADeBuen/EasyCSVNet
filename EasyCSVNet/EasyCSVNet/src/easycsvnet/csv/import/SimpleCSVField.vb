#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

#Region "Imports"

Imports EasyCSVNet.easycsvnet.util

#End Region 'Imports

Namespace easycsvnet
    Namespace csv
        Namespace import

            Public Class SimpleCSVField

#Region "Private attributes"

                Private _id As String = Nothing
                Private _position As UShort = Nothing
                Private _nillable As Boolean = Nothing
                Private _quoted As Boolean = Nothing
                Private _description As String = Nothing
                Private _mapped_AttributeName As String = Nothing
                Private _mapped_Description As String = Nothing

#End Region 'Private attributes

#Region "Public methods"

#Region "Constructors"

                Public Sub New()
                    MyBase.New()
                End Sub

#End Region 'Constructors

#Region "Properties (getters & setters)"

                Public Property id() As String
                    Get
                        Return Me._id
                    End Get
                    Set(ByVal value As String)
                        If Utils.isNullOrEmptyString(value) Then
                            Me._id = ""
                        Else
                            Me._id = value
                        End If
                    End Set
                End Property

                Public Property position() As UShort
                    Get
                        Return Me._position
                    End Get
                    Set(ByVal value As UShort)
                        If IsNothing(value) Then
                            Me._position = 0
                        Else
                            Me._position = value
                        End If
                    End Set
                End Property

                Public Property nillable() As Boolean
                    Get
                        Return Me._nillable
                    End Get
                    Set(ByVal value As Boolean)
                        If IsNothing(value) Then
                            Me._nillable = True
                        Else
                            Me._nillable = value
                        End If
                    End Set
                End Property

                Public Property quoted() As Boolean
                    Get
                        Return Me._quoted
                    End Get
                    Set(ByVal value As Boolean)
                        If IsNothing(value) Then
                            Me._quoted = False
                        Else
                            Me._quoted = value
                        End If
                    End Set
                End Property

                Public Property description() As String
                    Get
                        Return Me._description
                    End Get
                    Set(ByVal value As String)
                        If Utils.isNullOrEmptyString(value) Then
                            Me._description = ""
                        Else
                            Me._description = value
                        End If
                    End Set
                End Property

                Public Property mappedAttributeName() As String
                    Get
                        Return Me._mapped_AttributeName
                    End Get
                    Set(ByVal value As String)
                        If Utils.isNullOrEmptyString(value) Then
                            Me._mapped_AttributeName = ""
                        Else
                            Me._mapped_AttributeName = value
                        End If
                    End Set
                End Property

                Public Property mappedDescription() As String
                    Get
                        Return Me._mapped_Description
                    End Get
                    Set(ByVal value As String)
                        If Utils.isNullOrEmptyString(value) Then
                            Me._mapped_Description = ""
                        Else
                            Me._mapped_Description = value
                        End If
                    End Set
                End Property

#End Region 'Properties (getters & setters)

#End Region 'Public methods

            End Class

        End Namespace
    End Namespace
End Namespace