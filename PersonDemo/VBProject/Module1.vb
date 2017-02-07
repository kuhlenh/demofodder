Imports PersonDemo
Imports Xunit

Module Module1

    Sub Main()

    End Sub

    Public Function Score(p As Person, s As LightSaber) As (saber As LightSaber, jedi As Person, lightScore As Integer)
        If p.Name = "Obi-Wan Kenobi" Then
            If s.Color = "Red" Then Return (s, p, p.Name.Length * -1 * s.Color.Length)
            If s.Color = "Blue" Then Return (s, p, p.Name.Length)
            If s.Color = "Green" Then Return (s, p, p.Name.Length * s.Color.Length)
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
    Public Color As String

    Public Sub New(color As String)
        Me.Color = color
    End Sub

End Class


Public Class TestVB
    Dim jedis = New Person() {New Teacher("Obi-Wan Kenobi", "The Force"), New Student("Anakin Skywalker", 3)}
    Dim sabers = New LightSaber() {New LightSaber("Green"), New LightSaber("Blue"), New LightSaber("Red")}

    <Fact>
    Public Sub TestScoreObi()
        Assert.Equal(70, Score(jedis(0), sabers(0)).lightScore)
    End Sub

    <Fact>
    Public Sub TestScoreAni()
        Assert.Equal(-48, Score(jedis(1), sabers(2)).lightScore)
    End Sub
End Class