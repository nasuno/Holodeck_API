<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
Public Class PluginMetadataAttribute
    Inherits Attribute

    Public Property Name As String
    Public Property Version As String
    Public Property Author As String
    Public Property Description As String

    Public Sub New(name As String, version As String, author As String, description As String)
        Me.Name = name
        Me.Version = version
        Me.Author = author
        Me.Description = description
    End Sub
End Class