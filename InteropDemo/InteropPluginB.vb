Imports System.Configuration
Imports System.Reflection
Imports Current.PluginApi


<PluginMetadata("Interop Demo PluginB", "1.0", "Tester", "Usage examples.")>
Public Class InteropDemoPluginB
    Implements IPlugin

    Public Sub Execute(api As ICurrentApi) Implements IPlugin.Execute


        '   Retrieve the provider by its registration key
        Dim provider As Object = PluginLocator.Get(Of Object)("Interop Demo PluginA")
        If provider Is Nothing Then
            Console.WriteLine("Provider plugin not found.")
            Return
        End If

        '   Use reflection to look up and invoke methods on 'provider'…
        '   Examples below demonstrate the various method signatures.


        ' Example 1: Sub With no parameters, no Return
        Dim mi1 As MethodInfo = provider.GetType().GetMethod("SayHello")
        If mi1 IsNot Nothing Then
            ' Empty Object() array for no parameters
            mi1.Invoke(provider, New Object() {})
        End If

        ' Example 2: Sub with() parameters, no Return
        Dim mi2 As MethodInfo = provider.GetType().GetMethod("PrintMessage")
        If mi2 IsNot Nothing Then
            ' Pass arguments in an Object() array
            mi2.Invoke(provider, New Object() {"This is a test message."})
        End If

        ' Example 3: Function with no parameters, return value
        Dim mi3 As MethodInfo = provider.GetType().GetMethod("GetGreeting")
        If mi3 IsNot Nothing Then
            Dim result As Object = mi3.Invoke(provider, New Object() {})
            Dim greeting As String = CType(result, String)
            Console.WriteLine($"Received: {greeting}")
        End If

        'Example 4: Function with parameters and return value
        Dim mi4 As MethodInfo = provider.GetType().GetMethod("Add")
        If mi4 IsNot Nothing Then
            Dim result As Object = mi4.Invoke(provider, New Object() {2, 3})
            Dim sum As Integer = CType(result, Integer)
            Console.WriteLine($"2 + 3 = {sum}")
        End If


    End Sub

End Class
