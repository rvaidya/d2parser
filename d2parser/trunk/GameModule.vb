Imports System.Configuration

Public Class GameModule
    Implements BlueVex.IGameModule
    Implements BlueVex.IChatModule
    Public WithEvents Game As BlueVex.IGame
    Public WithEvents Chat As BlueVex.IChat
    Public ItemNum As Integer = 0
    Private InventoryOwners(200) As String
    Private Inventory(200) As ItemStruct
    Dim Hero As HeroStruct
    Dim Town As TownStruct
    Dim Instance As Integer
    Dim Version As String = "1.0.0.9"
    Dim D2PIni As New IniHandler(My.Application.Info.DirectoryPath.Replace("ManagedPlugins", "") + "D2Parser.ini")

#Region " Module Info "

    Public ReadOnly Property Author() As String Implements BlueVex.IGameModule.Author
        Get
            Return "bimf and Buey"
        End Get
    End Property

    Public ReadOnly Property AboutInfo() As String Implements BlueVex.IGameModule.AboutInfo
        Get
            Return "Logs All your Items and Puts them in a powerful Database"
        End Get
    End Property

    Public ReadOnly Property Name() As String Implements BlueVex.IGameModule.Name
        Get
            Return "D2Parser"
        End Get
    End Property

    Public ReadOnly Property ReleaseDate() As String Implements BlueVex.IGameModule.ReleaseDate
        Get
            Return "August 2008"
        End Get
    End Property

    Public ReadOnly Property Version2() As String Implements BlueVex.IGameModule.Version
        Get
            Return Version
        End Get
    End Property

    Public ReadOnly Property AboutInfo1() As String Implements BlueVex.IChatModule.AboutInfo
        Get
            Return "Logs All your Items and Puts them in a powerful Database"
        End Get
    End Property

    Public ReadOnly Property Author1() As String Implements BlueVex.IChatModule.Author
        Get
            Return "bimf and Buey"
        End Get
    End Property

    Public ReadOnly Property Name1() As String Implements BlueVex.IChatModule.Name
        Get
            Return "D2Parser"
        End Get
    End Property

    Public ReadOnly Property ReleaseDate1() As String Implements BlueVex.IChatModule.ReleaseDate
        Get
            Return "August 2008"
        End Get
    End Property

    Public ReadOnly Property Version1() As String Implements BlueVex.IChatModule.Version
        Get
            Return Version
        End Get
    End Property

#End Region

#Region " Module Subs "

    Public Sub Initialize(ByRef Game As BlueVex.IGame) Implements BlueVex.IGameModule.Initialize
        Me.Game = Game
    End Sub

    Public Sub Destroy() Implements BlueVex.IGameModule.Destroy
        Hero.Ingame = False
    End Sub

    Public Sub Update() Implements BlueVex.IGameModule.Update
        'If Hero.Name IsNot Nothing And D2Hwnd = 0 Then
        'Dim D2Process As Process = BlueVex.Memory.ClientDetection.ClientProcessFromCharName(Hero.Name)
        'If D2Process IsNot Nothing Then
        '        D2Hwnd = D2Process.MainWindowHandle.ToInt32
        'End If
        'End If
    End Sub

    Public Sub Update1() Implements BlueVex.IChatModule.Update

    End Sub

    Public Sub Destroy1() Implements BlueVex.IChatModule.Destroy

    End Sub

    Public Function GetInventoryFromName(ByRef Name As String) As Integer
        Dim i As Integer
        For i = 0 To 200
            If InventoryOwners(i) = Name Then
                Return i
            End If
        Next
        Return -1

    End Function

    Public Sub Initialize1(ByRef Chat As BlueVex.IChat) Implements BlueVex.IChatModule.Initialize
        Me.Chat = Chat
    End Sub

#End Region

#Region " Chat Part "
    Public Sub OnEnterChatResponse(ByVal Packet As BnetServer.EnterChatResponse, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Chat.OnEnterChatResponse
        Dim Path As String = ReadIniValue("Settings", "Path", "")
        'Check if Directory Exists, Else create Directory
        If Not My.Computer.FileSystem.DirectoryExists(Path) Then
            My.Computer.FileSystem.CreateDirectory(Path)
        End If

        'Check if File Exists, Else Create File
        If Not My.Computer.FileSystem.FileExists(Path + "\logins.txt") Then
            Dim NewFile As System.IO.StreamWriter
            NewFile = System.IO.File.CreateText(Path + "\logins.txt")
            NewFile.Close()
        End If
        My.Computer.FileSystem.WriteAllText(Path + "\logins.txt", Packet.Name + "," + Packet.Account + "," + Packet.Realm + vbNewLine, True)
    End Sub

#End Region

    Sub OnGameHandshake(ByVal Packet As GameServer.GameHandshake, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnGameHandshake
        If Packet.UnitType = D2Data.UnitType.Player Then
            Hero.UID = Packet.UID
        End If
    End Sub

    Sub OnPlayerInGame(ByVal Packet As GameServer.PlayerInGame, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnPlayerInGame
        If Hero.UID = Packet.UID Then
            Hero.Ingame = True
            Hero.Name = Packet.Name
            Hero.Level = Packet.Level
            Hero.Classe = Packet.Class
        End If
    End Sub

    Sub OnAssignMerc(ByVal Packet As GameServer.AssignMerc, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnAssignMerc
        If Packet.OwnerUID = Hero.UID Then
            Hero.MercUID = Packet.UID
        End If
    End Sub

    Sub OnItemOAction(ByVal Packet As GameServer.OwnedItemAction, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnOwnedItemAction
        'SaveToLog(Packet.ToLongInfoString())
        If Not (Packet.OwnerUID = Hero.UID Or Packet.OwnerUID = Hero.MercUID) Then Return
        Dim Item As GameServer.OwnedItemAction = Packet
        Dim Stopper As Boolean = False
        Dim Index As Integer = 0
        If Item.Action = D2Data.ItemActionType.RemoveFromContainer Or Item.Action = D2Data.ItemActionType.Unequip Then
            If Item.Action = D2Data.ItemActionType.RemoveFromContainer Then
                If Item.Container = D2Data.ItemContainer.TraderOffer Then Return
                If Item.Container = D2Data.ItemContainer.ForTrade Then Return
            End If
            Dim NewInventory(200) As ItemStruct
            Dim i As Integer
            Dim NewIndex As Integer = 0
            Dim c As Integer = 0
            For i = 0 To ItemNum - 1
                If Inventory(i).Id = Item.UID Then
                    c = c + 1
                Else
                    NewInventory(NewIndex) = Inventory(i)
                    NewIndex = NewIndex + 1
                End If
            Next
            ItemNum = ItemNum - c
            Inventory = NewInventory
        ElseIf Item.Action = D2Data.ItemActionType.PutInContainer Or Item.Action = D2Data.ItemActionType.Equip Then
            If (AlreadyGot(Item.UID)) Then
                Return
            End If
            If Item.Action = D2Data.ItemActionType.PutInContainer Then
                If Item.Container = D2Data.ItemContainer.TraderOffer Then Return
                If Item.Container = D2Data.ItemContainer.ForTrade Then Return
                Inventory(ItemNum).X = Item.X.ToString
                Inventory(ItemNum).Y = Item.Y.ToString
                Inventory(ItemNum).Container = Item.Container.ToString
            End If
            If Item.Action = D2Data.ItemActionType.Equip Then
                Inventory(ItemNum).OwnerType = Item.OwnerType.ToString
                Inventory(ItemNum).Location = Item.Location.ToString
            End If
            If Not Item.BaseItem Is Nothing And Not Item.BaseItem.Name Is Nothing Then
                Inventory(ItemNum).BaseItemName = Item.BaseItem.Name
            End If

            Inventory(ItemNum).Color = Item.Color.ToString
            Inventory(ItemNum).Id = Item.UID

            If Not Item.Runeword Is Nothing Then
                Inventory(ItemNum).Runeword = Item.Runeword.Name
                Inventory(ItemNum).RunewordID = Item.RunewordID.ToString
                Inventory(ItemNum).RunewordParam = Item.RunewordParam.ToString
            Else
                Inventory(ItemNum).Runeword = "-1"
            End If
            Log(Inventory(ItemNum).Runeword)

            Inventory(ItemNum).Quality = Quality2String(Item.Quality)
            Inventory(ItemNum).Level = Item.Level.ToString
            Inventory(ItemNum).Image = Item.Graphic.ToString

            If (Item.Stats.Count > 0) Then
                For i As Integer = 0 To Item.Stats.Count - 1
                    Inventory(ItemNum).Stats += Item.Stats.Item(i).ToString + ";"
                Next i
            End If

            If Not Item.Mods Is Nothing Then
                If (Item.Mods.Count > 0) Then
                    For i As Integer = 0 To Item.Mods.Count - 1
                        Inventory(ItemNum).Mods += Item.Mods.Item(i).ToString + ";"
                    Next i
                End If
            End If
            If (Item.Quality = D2Data.ItemQuality.Unique) Then
                If Item.UniqueItem Is Nothing Then
                    Inventory(ItemNum).Name = "Unidentified"
                Else
                    Inventory(ItemNum).Name = Item.UniqueItem.Name
                End If
            ElseIf (Item.Quality = D2Data.ItemQuality.Set) Then
                If Item.SetItem Is Nothing Then
                    Inventory(ItemNum).Name = "Unidentified"
                Else
                    Inventory(ItemNum).Name = Item.SetItem.Name
                End If
            ElseIf (Item.Quality = D2Data.ItemQuality.Rare) Then
                Inventory(ItemNum).Name = ""
            ElseIf (Item.Quality = D2Data.ItemQuality.Crafted) Then
                Inventory(ItemNum).Name = ""
            Else
                Inventory(ItemNum).Name = Item.BaseItem.Name
            End If

            If Not Item.Prefix Is Nothing Then
                Inventory(ItemNum).Prefix = ReturnLetters(Item.Prefix.Name)
                Inventory(ItemNum).PrefixVar = ReturnNumbers(Item.Prefix.Name)
                Inventory(ItemNum).Name = Inventory(ItemNum).Prefix + " " + Inventory(ItemNum).Name
            End If

            If Not Item.Suffix Is Nothing Then
                If Not Item.Suffix.Name = "" Then
                    Inventory(ItemNum).Suffix = ReturnLetters(Item.Suffix.Name)
                    Inventory(ItemNum).SuffixVar = ReturnNumbers(Item.Suffix.Name)
                    If (Item.Quality = D2Data.ItemQuality.Rare Or Item.Quality = D2Data.ItemQuality.Crafted) Then
                        Inventory(ItemNum).Name = Inventory(ItemNum).Name + Inventory(ItemNum).Suffix
                    Else
                        Inventory(ItemNum).Name = Inventory(ItemNum).Name + " of " + Inventory(ItemNum).Suffix
                    End If
                End If
            End If
            Inventory(ItemNum).Flags = Item.Flags.ToString
            Inventory(ItemNum).Name = Inventory(ItemNum).Name.Trim()

            Log("Item #" + ItemNum.ToString + ": " + Inventory(ItemNum).Name + " : " + Inventory(ItemNum).Id.ToString)
            Log(Hero.Realm)
            Log(Hero.Realm)
            ItemNum += 1
        End If
    End Sub
    Sub OnItemWAction(ByVal Packet As GameServer.WorldItemAction, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnWorldItemAction
        'SaveToLog(Packet.ToLongInfoString())
        Dim Item As GameServer.WorldItemAction = Packet
        Dim Stopper As Boolean = False
        Dim Index As Integer = 0
        If Item.Action = D2Data.ItemActionType.PutInContainer Then
            If (AlreadyGot(Item.UID)) Then
                Return
            End If
            If Item.Action = D2Data.ItemActionType.PutInContainer Then
                Inventory(ItemNum).X = Item.X.ToString
                Inventory(ItemNum).Y = Item.Y.ToString
                Inventory(ItemNum).Container = Item.Container.ToString
            End If
            If Not Item.BaseItem Is Nothing And Not Item.BaseItem.Name Is Nothing Then
                Inventory(ItemNum).BaseItemName = Item.BaseItem.Name
            End If

            Inventory(ItemNum).Color = Item.Color.ToString
            Inventory(ItemNum).Id = Item.UID

            If Not Item.Runeword Is Nothing Then
                Inventory(ItemNum).Runeword = Item.Runeword.Name
                Inventory(ItemNum).RunewordID = Item.RunewordID.ToString
                Inventory(ItemNum).RunewordParam = Item.RunewordParam.ToString
            Else
                Inventory(ItemNum).Runeword = "-1"
            End If
            Log(Inventory(ItemNum).Runeword)


            Inventory(ItemNum).Quality = Quality2String(Item.Quality)
            Inventory(ItemNum).Level = Item.Level.ToString
            Inventory(ItemNum).Image = Item.Graphic.ToString

            If (Item.Stats.Count > 0) Then
                For i As Integer = 0 To Item.Stats.Count - 1
                    Inventory(ItemNum).Stats += Item.Stats.Item(i).ToString + ";"
                Next i
            End If

            If Not Item.Mods Is Nothing Then
                If (Item.Mods.Count > 0) Then
                    For i As Integer = 0 To Item.Mods.Count - 1
                        Inventory(ItemNum).Mods += Item.Mods.Item(i).ToString + ";"
                    Next i
                End If
            End If
            If (Item.Quality = D2Data.ItemQuality.Unique) Then
                If Item.UniqueItem Is Nothing Then
                    Inventory(ItemNum).Name = "Unidentified"
                Else
                    Inventory(ItemNum).Name = Item.UniqueItem.Name
                End If
            ElseIf (Item.Quality = D2Data.ItemQuality.Set) Then
                If Item.SetItem Is Nothing Then
                    Inventory(ItemNum).Name = "Unidentified"
                Else
                    Inventory(ItemNum).Name = Item.SetItem.Name
                End If
            ElseIf (Item.Quality = D2Data.ItemQuality.Rare) Then
                Inventory(ItemNum).Name = ""
            ElseIf (Item.Quality = D2Data.ItemQuality.Crafted) Then
                Inventory(ItemNum).Name = ""
            Else
                Inventory(ItemNum).Name = Item.BaseItem.Name
            End If

            If Not Item.Prefix Is Nothing Then
                Inventory(ItemNum).Prefix = ReturnLetters(Item.Prefix.Name)
                Inventory(ItemNum).PrefixVar = ReturnNumbers(Item.Prefix.Name)
                Inventory(ItemNum).Name = Inventory(ItemNum).Prefix + " " + Inventory(ItemNum).Name
            End If

            If Not Item.Suffix Is Nothing Then
                If Not Item.Suffix.Name = "" Then
                    Inventory(ItemNum).Suffix = ReturnLetters(Item.Suffix.Name)
                    Inventory(ItemNum).SuffixVar = ReturnNumbers(Item.Suffix.Name)
                    If (Item.Quality = D2Data.ItemQuality.Rare Or Item.Quality = D2Data.ItemQuality.Crafted) Then
                        Inventory(ItemNum).Name = Inventory(ItemNum).Name + Inventory(ItemNum).Suffix
                    Else
                        Inventory(ItemNum).Name = Inventory(ItemNum).Name + " of " + Inventory(ItemNum).Suffix
                    End If
                End If
            End If
            Inventory(ItemNum).Flags = Item.Flags.ToString
            Inventory(ItemNum).Name = Inventory(ItemNum).Name.Trim()

            Log("Item #" + ItemNum.ToString + ": " + Inventory(ItemNum).Name + " : " + Inventory(ItemNum).Id.ToString)
            Log(Hero.Realm)
            Log(Hero.Realm)
            ItemNum += 1
        End If
    End Sub

    Sub OnGameOver(ByVal Packet As GameServer.GameOver, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnGameOver
        ProcessExit()
    End Sub
    Sub OnExitGame(ByVal Packet As GameClient.ExitGame, ByRef Flag As BlueVex.Packet.PacketFlag) Handles Game.OnExitGame
        'SaveToLog(Packet.ToLongInfoString())
        ProcessExit()
    End Sub

    Sub ProcessExit()
        If Not Hero.Ingame Then Return
        Dim Path As String = ReadIniValue("Settings", "Path", "")
        Dim AllLogins As String = My.Computer.FileSystem.ReadAllText(Path + "\logins.txt")
        Dim AllLoginsArray As String() = AllLogins.Split(vbNewLine.ToCharArray()(0))
        Dim ctr As Integer
        Hero.Account = ""
        Hero.Realm = ""
        For ctr = AllLoginsArray.GetLength(0) - 1 To 0 Step -1
            Dim Vals As String() = AllLoginsArray(ctr).Split(",".ToCharArray()(0))
            If Vals(0).Contains(Hero.Name) Then
                Hero.Account = Vals(1)
                Hero.Realm = Vals(2)
            End If
        Next
        Dim Output As String = ""
        Dim i As Integer
        Output += "<Inventory owner=""" + Hero.Name.ToString + """ classe=""" + ClasstoString(Hero.Classe) + """ level=""" + Hero.Level.ToString + """ account=""" + Hero.Account + """ realm=""" + Hero.Realm + """>" + vbNewLine
        For i = 0 To ItemNum - 1
            Output += vbTab + "<Item name=""" + Inventory(i).Name + """>" + vbNewLine

            Output += AddTag("id", Inventory(i).Id.ToString)
            Output += AddTag("color", Inventory(i).Color)
            Output += AddTag("level", Inventory(i).Level)
            Output += AddTag("baseitem", Inventory(i).BaseItemName)
            Output += AddTag("flags", Inventory(i).Flags)
            Output += AddTag("quality", Inventory(i).Quality)
            Output += AddTag("stats", Inventory(i).Stats)
            Output += AddTag("mods", Inventory(i).Mods)
            Output += AddTag("runeword", Inventory(i).Runeword)
            Output += AddTag("runewordid", Inventory(i).RunewordID)
            Output += AddTag("runewordparam", Inventory(i).RunewordParam)
            Output += AddTag("prefix", Inventory(i).Prefix)
            Output += AddTag("prefixvar", Inventory(i).PrefixVar)
            Output += AddTag("suffix", Inventory(i).Suffix)
            Output += AddTag("suffixvar", Inventory(i).SuffixVar)
            Output += AddTag("image", Inventory(i).Image)
            Output += AddTag("location", Inventory(i).Location)
            Output += AddTag("ownertype", Inventory(i).OwnerType)
            Output += AddTag("container", Inventory(i).Container)
            Output += AddTag("X", Inventory(i).X)
            Output += AddTag("Y", Inventory(i).Y)
            Output += vbTab + "</Item>" + vbNewLine
        Next i
        Output += "</Inventory>" + vbNewLine
        SaveFile(Hero.Name, Output)
        Hero.Ingame = False
    End Sub

    Sub RegisterSettings(ByVal myRealm As String, ByVal myAccount As String)
        Hero.Realm = myRealm
        Hero.Account = myAccount
    End Sub

    Public Function AddTag(ByVal Tag As String, ByVal Value As String) As String
        If Value = "" Or Value = "-1" Then
            Return ""
        Else
            Return vbTab + vbTab + "<" + Tag + ">" + Value + "</" + Tag + ">" + vbNewLine
        End If
    End Function

    Function Quality2String(ByVal Quality As Integer) As String
        Select Case Quality
            Case 1
                Return "Inferior"
            Case 2
                Return "Normal"
            Case 3
                Return "Superior"
            Case 4
                Return "Magic"
            Case 5
                Return "Set"
            Case 6
                Return "Rare"
            Case 7
                Return "Unique"
            Case 8
                Return "Crafted"
            Case Else
                Return ""
        End Select
    End Function

    Public Function ReturnNumbers(ByVal sString As String) As String
        Dim retVal As String
        If sString = "" Then
            Return ""
        End If
        retVal = String.Empty
        For i As Int32 = 0 To sString.Length - 1
            If sString.Substring(i, 1) Like "[0-9]" Then
                retVal = retVal + sString.Substring(i, 1)
            End If
        Next i
        Return retVal
    End Function

    Public Function ReturnLetters(ByVal sString As String) As String
        Dim retVal As String
        If sString = "" Then
            Return ""
        End If
        retVal = String.Empty
        For i As Int32 = 0 To sString.Length - 1
            If Not sString.Substring(i, 1) Like "[0-9]" Then
                retVal = retVal + sString.Substring(i, 1)
            End If
        Next i
        Return retVal
    End Function

    Public Function ClasstoString(ByVal Classe As D2Data.CharacterClass) As String
        Select Case Classe
            Case D2Data.CharacterClass.Amazon
                ClasstoString = "Amazon"
            Case D2Data.CharacterClass.Assassin
                ClasstoString = "Assasin"
            Case D2Data.CharacterClass.Barbarian
                ClasstoString = "Barbarian"
            Case D2Data.CharacterClass.Druid
                ClasstoString = "Druid"
            Case D2Data.CharacterClass.Necromancer
                ClasstoString = "Necromancer"
            Case D2Data.CharacterClass.Paladin
                ClasstoString = "Paladin"
            Case D2Data.CharacterClass.Sorceress
                ClasstoString = "Sorceress"
            Case Else
                ClasstoString = "Unknown"
        End Select
    End Function

    Public Function AlreadyGot(ByVal id As UInteger) As Boolean
        Dim i As Integer
        For i = 0 To ItemNum - 1
            If Inventory(i).Id = id Then
                Return True
            End If
        Next i
        Return False
    End Function

    Public Sub Log(ByVal msg As String)
        My.Application.Log.WriteEntry(msg)
    End Sub
    Public Function ReadIniValue(ByVal Section As String, ByVal Key As String, ByVal Def As String) As String
        If Not My.Computer.FileSystem.FileExists("D2Parser.ini") Then
            Dim InstallPath As String = ""
            InstallPath = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Blizzard Entertainment\Diablo II", "InstallPath", 0).ToString
            If InstallPath = "" Then
                InstallPath = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Blizzard Entertainment\Diablo II", "InstallPath", 0).ToString
            End If
            If Not InstallPath.EndsWith("\") Then InstallPath = InstallPath + "\"
            D2PIni.WriteString("Settings", "Path", InstallPath + "d2plog")
        Else
            Dim Val As String = D2PIni.GetString("Settings", "Path", "")
            If Val = "" Then
                'Badly formatted ini file
                My.Computer.FileSystem.DeleteFile("D2Parser.ini")
                Dim NewFile As System.IO.FileStream
                NewFile = System.IO.File.Create("D2Parser.ini")
                NewFile.Close()
                Dim InstallPath As String = ""
                InstallPath = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Blizzard Entertainment\Diablo II", "InstallPath", 0).ToString
                If InstallPath = "" Then
                    InstallPath = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Blizzard Entertainment\Diablo II", "InstallPath", 0).ToString
                End If
                If Not InstallPath.EndsWith("\") Then InstallPath = InstallPath + "\"
                D2PIni.WriteString("Settings", "Path", InstallPath + "d2plog")
            End If
        End If
        Return D2PIni.GetString(Section, Key, Def)
    End Function
    Public Sub SaveFile(ByVal FileName As String, ByVal Text As String)
        Dim FullPath As String

        Dim LogPath As String = ReadIniValue("Settings", "Path", "")
        FullPath = LogPath + "\" + Hero.Realm + "." + Hero.Account + "." + FileName + ".xml"


        'Check if Directory Exists, Else create Directory
        If Not My.Computer.FileSystem.DirectoryExists(LogPath) Then
            My.Computer.FileSystem.CreateDirectory(LogPath)
        End If

        'Check if File Exists, Else Create File
        If Not My.Computer.FileSystem.FileExists(FullPath) Then
            Dim NewFile As System.IO.StreamWriter
            NewFile = System.IO.File.CreateText(FullPath)
            NewFile.Close()
        End If

        My.Computer.FileSystem.WriteAllText(FullPath, Text, False)
    End Sub

    Public Sub SaveToLog(ByVal Text As String)
        Dim FullPath As String

        Dim LogPath As String = ReadIniValue("Settings", "Path", "")
        FullPath = LogPath + "\tester.xml"


        'Check if Directory Exists, Else create Directory
        If Not My.Computer.FileSystem.DirectoryExists(LogPath) Then
            My.Computer.FileSystem.CreateDirectory(LogPath)
        End If

        'Check if File Exists, Else Create File
        If Not My.Computer.FileSystem.FileExists(FullPath) Then
            Dim NewFile As System.IO.StreamWriter
            NewFile = System.IO.File.CreateText(FullPath)
            NewFile.Close()
        End If

        My.Computer.FileSystem.WriteAllText(FullPath, Text & vbCrLf, True)
    End Sub

End Class
