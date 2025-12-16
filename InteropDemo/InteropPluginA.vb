Imports Current.PluginApi


<PluginMetadata("Interop Demo PluginA", "1.0", "Tester", "Usage examples.")>
Public Class InteropDemoPluginA
    Implements IPlugin

    Public Sub Execute(api As ICurrentApi) Implements IPlugin.Execute
        ' Register *this* plugin instance under a unique key.
        PluginLocator.Register("Interop Demo PluginA", Me)
    End Sub


    ' 'Example 1: Sub With no parameters, no Return
    Public Sub SayHello()
        Console.WriteLine("Hello from Provider!")
    End Sub

    ' Example 2: Sub With parameters, no Return
    Public Sub PrintMessage(msg As String)
        Console.WriteLine(msg)
    End Sub

    ' Example 3: Function With no parameters, Return value
    Public Function GetGreeting() As String
        Return "Hello from Provider!"
    End Function

    ' Example 4 Function with() parameters And Return value
    Public Function Add(x As Integer, y As Integer) As Integer
        Return x + y
    End Function


End Class

