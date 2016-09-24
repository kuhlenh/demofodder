Imports PersonDemo
Imports VBProject

Module Module1

    Sub Main()
        Dim personsVB = New Person() {New Teacher("Obi-Wan Kenobi", "The Force"), New Student("Anakin Skywalker", 2.75)}
        Dim lightSabers = New LightSaber() {New LightSaber("Green"), New LightSaber("Blue"), New LightSaber("Red")}

        Dim tuple As (saber As LightSaber,
                      jedi As Person,
                      lightScore As Integer) = CalcaulateLightness(personsVB, lightSabers)

        Console.WriteLine(String.Format("{0} with {1} = {2}", tuple.jedi.Name, tuple.saber.Color, tuple.lightScore))
        Console.ReadLine()
    End Sub

    Private Function CalcaulateLightness(personsVB() As Person, lightSabers() As LightSaber) As (saber As LightSaber, jedi As Person, lightScore As Integer)
        For Each p In personsVB
            For Each s In lightSabers
                Return Score(p, s)
            Next
        Next
    End Function

    Private Function Score(p As Person, s As LightSaber) As (saber As LightSaber, jedi As Person, lightScore As Integer)
        If p.Name = "Obi-Wan Kenobi" Then
            If s.Color = "Red" Then Return (s, p, p.Name.Length * -1 * s.Color.Length)
            If s.Color = "Green" Then Return (s, p, p.Name.Length)
            If s.Color = "Blue" Then Return (s, p, p.Name.Length * s.Color.Length)
        ElseIf p.Name = "Anakin Skywalker" Then
            If s.Color = "Red" Then Return (s, p, p.Name.Length * -1 * s.Color.Length)
            If s.Color = "Green" Then Return (s, p, p.Name.Length * s.Color.Length)
            If s.Color = "Blue" Then Return (s, p, p.Name.Length)
        Else
            Return (s, p, 0)
        End If
        Return (s, p, 0)
    End Function


End Module

Friend Class LightSaber
    Private _color As String
    Private v As String

    Public Sub New(v As String)
        Me.v = v
    End Sub

    Public Property Color() As String
        Get
            Return _color
        End Get
        Set(ByVal value As String)
            _color = value
        End Set
    End Property
End Class
