Imports System.Runtime.InteropServices
Imports System.Diagnostics
Imports System.IO
Imports System.Collections.Generic

Public Class AppContext
    Inherits ApplicationContext

    Public WithEvents Tray As NotifyIcon
    Public WithEvents MainMenu As ContextMenuStrip
    Private WithEvents mnuSaveSettings As ToolStripMenuItem
    Private WithEvents mnuSep1 As ToolStripSeparator
    Private WithEvents mnuExit As ToolStripMenuItem

    ' ==========================================
    ' --- WIN32 APIs FOR AUTO-RESTORE ---
    ' ==========================================
    Private Delegate Sub WinEventDelegate(ByVal hWinEventHook As IntPtr, ByVal eventType As UInteger, ByVal hwnd As IntPtr, ByVal idObject As Integer, ByVal idChild As Integer, ByVal dwEventThread As UInteger, ByVal dwmsEventTime As UInteger)

    Private Declare Function SetWinEventHook Lib "user32" (ByVal eventMin As UInteger, ByVal eventMax As UInteger, ByVal hmodWinEventProc As IntPtr, ByVal lpfnWinEventProc As WinEventDelegate, ByVal idProcess As UInteger, ByVal idThread As UInteger, ByVal dwFlags As UInteger) As IntPtr
    Private Declare Function UnhookWinEvent Lib "user32" (ByVal hWinEventHook As IntPtr) As Boolean
    Private Declare Function GetWindowThreadProcessId Lib "user32" (ByVal hWnd As IntPtr, ByRef lpdwProcessId As UInteger) As UInteger
    Private Declare Function IsWindowVisible Lib "user32" (ByVal hWnd As IntPtr) As Boolean
    Private Declare Function GetParent Lib "user32" (ByVal hwnd As IntPtr) As IntPtr
    Private Declare Function RedrawWindow Lib "user32" (ByVal hwnd As IntPtr, ByVal lprcUpdate As IntPtr, ByVal hrgnUpdate As IntPtr, ByVal flags As UInteger) As Boolean
    Private Declare Function ShowWindow Lib "user32" (ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    Private Declare Function SetWindowPos Lib "user32" (ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInteger) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function SetWindowPlacement(ByVal hWnd As IntPtr, ByRef lpwndpl As WINDOWPLACEMENT) As Boolean
    End Function
    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    Private Const RDW_INVALIDATE As UInteger = &H1
    Private Const RDW_ERASE As UInteger = &H4
    Private Const RDW_ALLCHILDREN As UInteger = &H80
    Private Const RDW_UPDATENOW As UInteger = &H100

    ' --- STRUCTS ---
    Private Structure POINTAPI
        Public x As Integer
        Public y As Integer
    End Structure
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure
    Private Structure WINDOWPLACEMENT
        Public Length As Integer
        Public flags As Integer
        Public showCmd As Integer
        Public ptMinPosition As POINTAPI
        Public ptMaxPosition As POINTAPI
        Public rcNormalPosition As RECT
    End Structure

    ' --- HOOK VARIABLES ---
    Private winEventHook As IntPtr = IntPtr.Zero
    Private winEventProcDelegate As WinEventDelegate = Nothing
    Private autoRestoredWindows As New HashSet(Of IntPtr)()

    ' ==========================================
    ' --- AUTO-RESTORE LOGIC ---
    ' ==========================================
    Private Sub WinEventProc(ByVal hWinEventHook As IntPtr, ByVal eventType As UInteger, ByVal hwnd As IntPtr, ByVal idObject As Integer, ByVal idChild As Integer, ByVal dwEventThread As UInteger, ByVal dwmsEventTime As UInteger)
        If eventType = &H8001 Then
            autoRestoredWindows.Remove(hwnd)
            Return
        End If

        If eventType <> &H8002 Then Return
        If idObject <> 0 Then Return
        If hwnd = IntPtr.Zero Then Return
        If GetParent(hwnd) <> IntPtr.Zero Then Return

        Dim pid As UInteger
        GetWindowThreadProcessId(hwnd, pid)
        If pid = 0 Then Return

        Dim procName As String
        Try
            procName = Process.GetProcessById(CInt(pid)).ProcessName
        Catch
            Return
        End Try

        If procName.Equals("TANWindowMgr", StringComparison.OrdinalIgnoreCase) Then Return

        If Not IsWindowVisible(hwnd) Then Return
        If autoRestoredWindows.Contains(hwnd) Then Return

        Dim savedPlacement As WINDOWPLACEMENT
        If Not TryGetSavedPlacement(procName, savedPlacement) Then Return

        autoRestoredWindows.Add(hwnd)

        ShowWindow(hwnd, 9) ' SW_RESTORE

        Dim l As Integer = savedPlacement.rcNormalPosition.Left
        Dim t As Integer = savedPlacement.rcNormalPosition.Top
        Dim r As Integer = savedPlacement.rcNormalPosition.Right
        Dim b As Integer = savedPlacement.rcNormalPosition.Bottom

        ' Move the window using the reliable API
        SetWindowPos(hwnd, IntPtr.Zero, l, t, r - l, b - t, &H4 Or &H40)

        SetForegroundWindow(hwnd)
        RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE Or RDW_ERASE Or RDW_ALLCHILDREN Or RDW_UPDATENOW)
    End Sub

    Private Function TryGetSavedPlacement(ByVal procName As String, ByRef placement As WINDOWPLACEMENT) As Boolean
        If Not File.Exists("Settings.ini") Then Return False

        Dim lines() As String = File.ReadAllLines("Settings.ini")
        Dim autoProfile As String = "Default"

        ' 1. Find the active profile from the top of the file
        For Each line As String In lines
            If line.StartsWith("AutoLoadProfile,") Then
                Dim parts() = line.Split(","c)
                If parts.Length > 1 Then autoProfile = parts(1).Trim()
                Exit For
            End If
        Next

        ' 2. Search for the app in that specific profile
        For Each line As String In lines
            Dim specs() = line.Split(","c)

            If specs.Length >= 6 AndAlso specs(0).Equals(procName, StringComparison.OrdinalIgnoreCase) Then

                Dim lineProfile As String = "Default"
                If specs.Length = 7 Then lineProfile = specs(6)

                ' If it matches the active profile, apply the saved coordinates!
                If lineProfile.Equals(autoProfile, StringComparison.OrdinalIgnoreCase) Then
                    placement.Length = Marshal.SizeOf(GetType(WINDOWPLACEMENT))
                    placement.rcNormalPosition.Left = CInt(specs(1))
                    placement.rcNormalPosition.Top = CInt(specs(2))
                    placement.rcNormalPosition.Bottom = CInt(specs(3))
                    placement.rcNormalPosition.Right = CInt(specs(4))
                    placement.showCmd = CInt(specs(5))
                    Return True
                End If
            End If
        Next
        Return False
    End Function


    Public Sub New()

        'Initialize the menus
        mnuSaveSettings = New ToolStripMenuItem("Manage Profiles")
        mnuSep1 = New ToolStripSeparator()
        mnuExit = New ToolStripMenuItem("Exit")
        MainMenu = New ContextMenuStrip
        MainMenu.Items.AddRange(New ToolStripItem() {mnuSaveSettings, mnuSep1, mnuExit})
        MainMenu.Renderer = New DarkRenderer()
        'Initialize the tray
        Tray = New NotifyIcon
        Tray.Icon = My.Resources.TrayIcon
        Tray.ContextMenuStrip = MainMenu
        Tray.Text = "Windows Arrangement Manager"

        'Display
        Tray.Visible = True

        ' Start the background window hook
        winEventProcDelegate = New WinEventDelegate(AddressOf WinEventProc)
        winEventHook = SetWinEventHook(&H8001, &H8002, IntPtr.Zero, winEventProcDelegate, 0, 0, 0)
    End Sub

    Private Sub AppContext_ThreadExit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ThreadExit
        'Guarantees that the icon will not linger.
        Tray.Visible = False

        ' Cleanly remove the window hook
        If winEventHook <> IntPtr.Zero Then
            UnhookWinEvent(winEventHook)
            winEventHook = IntPtr.Zero
        End If
    End Sub

    Private Sub mnuSaveSettings_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuSaveSettings.Click
        If Not Form1.Visible Then Form1.Show() : Form1.Activate()
    End Sub

    Private Sub mnuExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuExit.Click
        Application.Exit()
    End Sub

    Private Sub Tray_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Tray.DoubleClick
        If Not Form1.Visible Then Form1.Show() : Form1.Activate()
    End Sub

    Private Sub Tray_Click(sender As Object, e As EventArgs) Handles Tray.Click
        Form1.cboProfiles.Items.Clear()
        Form1.cboProfiles.Items.Add("Default")
        Form1.cboProfiles.SelectedIndex = 0

        If Not IO.File.Exists("Settings.ini") Then Exit Sub

        Dim lines As List(Of String) = IO.File.ReadAllLines("Settings.ini").ToList
        Dim lstMenuItems As New List(Of String)

        For i As Integer = MainMenu.Items.Count - 1 To 0 Step -1
            If TypeOf (MainMenu.Items(i)) Is ToolStripMenuItem Then
                If CType(MainMenu.Items(i), ToolStripMenuItem).Text.StartsWith("Load Profile: ") Then MainMenu.Items.Remove(MainMenu.Items(i))
            End If
        Next

        For Each line In lines.ToList
            Dim specs() = line.Split(",")

            If specs.Length = 7 Then
                If Not Form1.cboProfiles.Items.Contains(specs(6)) Then
                    Form1.cboProfiles.Items.Add(specs(6))

                    Dim profMnu = New ToolStripMenuItem("Load Profile: " & specs(6)) With {.Tag = specs(6)}
                    MainMenu.Items.Insert(2, profMnu)
                    AddHandler profMnu.Click, AddressOf Form1.RestoreMenuProfile
                End If
            End If
        Next

        Dim profMnuDef = New ToolStripMenuItem("Load Profile: Default") With {.Tag = "Default"}
        MainMenu.Items.Insert(2, profMnuDef)
        AddHandler profMnuDef.Click, AddressOf Form1.RestoreMenuProfile
    End Sub
End Class

' --- CUSTOM DARK RENDERER FOR TRAY MENU ---
Public Class DarkColorTable
    Inherits ProfessionalColorTable

    Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
        Get
            Return Color.FromArgb(45, 45, 48) ' Dark gray background
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
        Get
            Return Color.FromArgb(32, 32, 32) ' Darker left margin
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
        Get
            Return Color.FromArgb(32, 32, 32)
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
        Get
            Return Color.FromArgb(32, 32, 32)
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemSelected As Color
        Get
            Return Color.FromArgb(0, 120, 215) ' Windows Blue highlight
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemBorder As Color
        Get
            Return Color.FromArgb(0, 120, 215)
        End Get
    End Property

    Public Overrides ReadOnly Property SeparatorDark As Color
        Get
            Return Color.FromArgb(60, 60, 60) ' Separator line color
        End Get
    End Property
End Class

Public Class DarkRenderer
    Inherits ToolStripProfessionalRenderer

    Public Sub New()
        MyBase.New(New DarkColorTable())
    End Sub

    Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)
        ' Make the text white
        e.TextColor = Color.White
        MyBase.OnRenderItemText(e)
    End Sub
End Class