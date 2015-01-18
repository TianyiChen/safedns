'Copyright 2015 - Tianyi Chen,owner of chentianyi.cn
'Developed based on:
' Copyright 2013 - DummyDNServer, Amr Denni <a.denni-yahoo-fr>  From https://github.com/denxp/dummyDNServer
'ARSoft.Tools.Net
' Licensed under the Apache License, Version 2.0
' http://www.apache.org/licenses/LICENSE-2.0

Imports ARSoft.Tools.Net.Dns
Imports ARSoft.Tools.Net.Dns.DnsServer

Imports System.Net
Imports System.Net.Sockets
Imports System.Reflection

Module main
    Const CacheLimit As UInteger = 10000
    Dim cache(1000, 1) As String
    Dim currentCache As UInt64 = 0

    ' Main entry point
    ' Inits the server and start listening
    ' Exit on keypress
    Sub Main()
        Console.WriteLine("Last update @ 2015/1/18 15:35")
        Using server = New DnsServer(IPAddress.Any, 10, 10, AddressOf ProcessQuery)
            server.Start()
            Console.WriteLine("Press any key to stop server")
            Console.ReadKey()
        End Using
    End Sub
    ' Processes the DNS queries
    Private Function ProcessQuery(ByVal message As DnsMessageBase, ByVal clientAddress As IPAddress, ByVal protocol As ProtocolType) As DnsMessageBase
        Dim CTT As String
        message.IsQuery = False

        Dim query As DnsMessage = CType(message, DnsMessage)
        Dim address As IPAddress
    
        If (query IsNot Nothing And query.Questions.Count = 1) Then

            Dim question As DnsQuestion = query.Questions(0)
            For i As UInt64 = 0 To currentCache
                If cache(i, 0) = question.Name Then
                    CTT = cache(i, 1)
                    GoTo Fnreply
                End If
            Next
            Console.WriteLine(clientAddress.ToString() & " :: " & question.Name)
            Console.WriteLine("Starting download:" + server + question.Name)
            query.ReturnCode = ReturnCode.NoError
            Using wc As New WebClient
                Try
                    CTT = System.Text.Encoding.UTF8.GetString(wc.DownloadData(server + question.Name))
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try
            End Using
            Console.WriteLine("Reply:" + CTT)
            If InStr(CTT, ";") > 0 Then
                'Valid response
                ' CTT = Mid(CTT, 1, InStr(CTT, ";") - 1)
                'add new cache
                If currentCache \ 1000 = currentCache / 1000 Then ReDim Preserve cache(currentCache + 1000, 1)
                If CacheLimit = currentCache Then GoTo nocache
                cache(currentCache, 0) = question.Name
                cache(currentCache, 1) = CTT
                currentCache += 1
nocache:
Fnreply:
                CTT = CTT.Replace(" ", "")
                Do Until CTT = ""
                    address = IPAddress.Parse(Mid(CTT, 1, InStr(CTT, ";") - 1))
                    'Get first IP in the content.
                    CTT = Mid(CTT, InStr(CTT, ";") + 1)
                    query.AnswerRecords.Add(New ARecord(question.Name, 180, address))
                Loop
            Else
                Dim answer As DnsMessage = DnsClient.Default.Resolve(question.Name, question.RecordType, question.RecordClass)
            End If
            Return query
        Else
            message.ReturnCode = ReturnCode.ServerFailure
        End If
        message.ReturnCode = ReturnCode.ServerFailure
        Return message

    End Function


End Module
