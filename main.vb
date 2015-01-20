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
    Dim blacklist As String
    'The following variables are used for statistics usage.They are not accurate.
    Dim SW_f, SW_c As UInteger 'forward cache
    ' Main entry point
    ' Inits the server and start listening
    ' Exit on keypress
    Sub Main()
        Using file As New System.IO.StreamReader("blacklist.txt")
            blacklist = file.ReadToEnd().Replace(vbCrLf, ";").Replace(",", ";")
            Console.WriteLine("Blacklist:{0}", blacklist)
            file.Close() : End Using
        Dim iptolisten As String = InputBox("Which IP do you want to listen?" + vbCrLf + "Type 0.0.0.0 for all available IPs", "Some information needed", "0.0.0.0")
        If iptolisten = "0.0.0.0" Then
            Using server = New DnsServer(IPAddress.Any, 10, 10, AddressOf ProcessQuery)
                server.Start() : Call UIHander()
            End Using
        Else
            Dim iptemp As IPAddress = IPAddress.Parse(iptolisten)
            Using server = New DnsServer(iptemp, 10, 10, AddressOf ProcessQuery)
                server.Start() : Call UIHander()
            End Using
        End If
    End Sub
    ' Processes the DNS queries
    Private Function ProcessQuery(ByVal message As DnsMessageBase, ByVal clientAddress As IPAddress, ByVal protocol As ProtocolType) As DnsMessageBase
        Dim CTT As String
        message.IsQuery = False
        Dim query As DnsMessage = CType(message, DnsMessage)
        Dim address As IPAddress
    
        If (query IsNot Nothing And query.Questions.Count = 1) Then

            Dim question As DnsQuestion = query.Questions(0)

            Console.WriteLine(clientAddress.ToString() & " : " & question.Name)


            'If the domain is in the black list.
            If IfMatchs(question.Name, blacklist) Then
                CTT = "0.0.0.0;" : GoTo Fnreply
            End If
            'cache
            For i As UInt64 = 0 To currentCache
                If cache(i, 0) = question.Name Then
                    CTT = cache(i, 1)
                    SW_c += 1
                    GoTo Fnreply
                End If
            Next
            SW_f += 1
            Console.WriteLine("Forwarding:" + question.Name)
            query.ReturnCode = ReturnCode.NoError
            Using wc As New WebClient
                Try
                    CTT = System.Text.Encoding.UTF8.GetString(wc.DownloadData(server + question.Name))
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try
            End Using
            Console.WriteLine("Reply:" + question.Name + "=" + CTT)
            If InStr(CTT, ";") > 0 Then
                'Valid response

                'add new cache
                If currentCache \ 1000 = currentCache / 1000 Then ReDim Preserve cache(currentCache + 1000, 1)
                If CacheLimit = currentCache Then GoTo nocache
                cache(currentCache, 0) = question.Name
                cache(currentCache, 1) = CTT
                currentCache += 1
nocache:
Fnreply:
                'Return the request using CTT.
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
    Function IfMatchs(q As String, list As String) As Boolean
        Dim Lenq = Len(q)
        Dim domain As String
        Do Until list = ""
            domain = Mid(list, 1, InStr(list, ";") - 1)
            If Mid(domain, 1, 1) = "." Then
                'If .domain
                If q = Mid(domain, 2) OrElse Mid(q, Lenq - Len(domain) + 1) = domain Then
                    Return True : Exit Function
                End If
            Else
                If q = domain Then
                    Return True : Exit Function
                End If
            End If
            list = Mid(list, InStr(list, ";") + 1)
        Loop
        Return False
    End Function
    Sub UIHander()
        Dim cmd As String
uistart:
        Console.Write("Command>>>")
        cmd = Console.ReadLine
        Select Case LCase(cmd)
            Case "exit"
                End
            Case "cache-used"
                Console.WriteLine("Cached:{0}   Total Space:{1}", currentCache.ToString, CacheLimit)
            Case "summery"
                Console.WriteLine("Total reqs received:{0}  Forwarded:{1}  Cached:{2}", SW_f + SW_c, SW_f, SW_c)
            Case Else
                Console.WriteLine("Unknown command.")
        End Select
        GoTo uistart
    End Sub
End Module
