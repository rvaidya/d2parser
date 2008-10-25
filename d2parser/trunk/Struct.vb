
Public Structure FLASHWINFO
    Public cbSize As Int32
    Public hwnd As IntPtr
    Public dwFlags As Int32
    Public uCount As Int32
    Public dwTimeout As Int32
End Structure

Public Structure HeroStruct
    Public Name As String
    Public Intown As Boolean
    Public Account As String
    Public Realm As String

    Public Classe As D2Data.CharacterClass
    Public Level As Integer

    Public UID As UInteger
    Public MercUID As UInteger
    Public X As Integer
    Public Y As Integer

    Public RightSkill As D2Data.SkillType
    Public LeftSkill As D2Data.SkillType

    Public Ingame As Boolean
End Structure

Structure TownStruct
    Public id As D2Data.AreaLevel
    Public minX As Integer
    Public minY As Integer
    Public MaxX As Integer
    Public MaxY As Integer
End Structure

Structure ItemStruct
    Public Id As UInteger
    Public OwnerType As String
    Public Name As String
    Public BaseItemName As String
    Public Level As String
    Public Color As String
    Public Category As String
    Public ItemClass As String
    Public Container As String
    Public Flags As String
    Public Mods As String
    Public Prefix As String
    Public PrefixVar As String
    Public Quality As String
    Public Runeword As String
    Public RunewordID As String
    Public RunewordParam As String
    Public SetBonuses As String
    Public SetItem As String
    Public Stats As String
    Public Suffix As String
    Public SuffixVar As String
    Public SuperiorType As String
    Public UniqueItem As String
    Public UsedSockets As String
    Public Image As String
    Public Location As String
    Public X As String
    Public Y As String
End Structure