Imports System.Runtime.InteropServices
Imports System.Diagnostics
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.Win32
Imports IWshRuntimeLibrary

Public Class Form1
    Public WindowList As New Dictionary(Of String, Process)
    Private identifyForms As New List(Of Form)
    Private identifyTimer As New Timer()
    Private isDragging As Boolean = False
    Private dragOffset As Point
    Private activeMonitorPanel As Panel
    Private Const SCALE_FACTOR As Integer = 15
    Private mapMinX As Integer = 0
    Private mapMinY As Integer = 0
    Private mapStartX As Integer = 0
    Private mapStartY As Integer = 0
    Private currentScaleFactor As Double = 1.0
    Private zoomMultiplier As Double = 1.0
    Private isPanning As Boolean = False
    Private panStartMouse As Point
    Private panStartX As Integer = 0
    Private panStartY As Integer = 0
    Private isIdentifying As Boolean = False
    Private IsDarkModeActive As Boolean = False
    Private checkedDisplays As New HashSet(Of String)()
    Private displayBackup As New Dictionary(Of String, DisplayManager.DEVMODE)()

    Private Declare Function GetWindowPlacement Lib "user32" (ByVal hwnd As IntPtr, ByRef lpwndpl As WINDOWPLACEMENT) As Integer
    <DllImport("user32.dll")>
    Private Shared Function SetWindowPlacement(ByVal hWnd As IntPtr, ByRef lpwndpl As WINDOWPLACEMENT) As Boolean
    End Function
    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    Private Declare Function GetWindowRect Lib "user32" (ByVal hWnd As IntPtr, ByRef lpRect As RECT) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInteger) As Boolean
    End Function
    ' --- DARK TITLE BAR API ---
    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(ByVal hwnd As IntPtr, ByVal attr As Integer, ByRef attrValue As Integer, ByVal attrSize As Integer) As Integer
    End Function
    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function SetWindowTheme(ByVal hWnd As IntPtr, ByVal pszSubAppName As String, ByVal pszSubIdList As String) As Integer
    End Function
    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20
    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 As Integer = 19
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

    Private Sub btnIdentify_Click(sender As Object, e As EventArgs) Handles btnIdentify.Click
        If isIdentifying Then
            ' Turn OFF
            For Each f As Form In identifyForms
                f.Close()
            Next
            identifyForms.Clear()
            isIdentifying = False
            Exit Sub
        End If

        ' Turn ON
        isIdentifying = True
        For Each item As ListViewItem In lvDisplays.Items
            Dim displayNumber As String = item.Text.Replace("Display ", "").Replace(" (Primary)", "")
            Dim monitorName As String = item.Tag.ToString()

            Dim targetScreen As Screen = Nothing
            For Each scr As Screen In Screen.AllScreens
                If scr.DeviceName = monitorName Then
                    targetScreen = scr
                    Exit For
                End If
            Next

            If targetScreen IsNot Nothing Then
                Dim overlay As New RulerOverlayForm()
                overlay.FormBorderStyle = FormBorderStyle.None
                overlay.StartPosition = FormStartPosition.Manual

                ' Make it cover the entire physical monitor
                overlay.Bounds = targetScreen.Bounds
                overlay.TopMost = True
                overlay.BackColor = Color.Fuchsia
                overlay.TransparencyKey = Color.Fuchsia ' Makes the center of the screen invisible
                overlay.ShowInTaskbar = False

                ' Pass the data to the Paint event
                Dim dm As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(monitorName)
                overlay.Tag = New Tuple(Of String, DisplayManager.DEVMODE)(displayNumber, dm)

                ' Draw the rulers and info box
                AddHandler overlay.Paint, Sub(s, ev)
                                              Dim f = CType(s, Form)
                                              Dim data = CType(f.Tag, Tuple(Of String, DisplayManager.DEVMODE))
                                              Dim num = data.Item1
                                              Dim dev = data.Item2
                                              Dim g = ev.Graphics

                                              ' --- DRAW RULERS ---
                                              Dim tickPen As New Pen(Color.White, 2)
                                              Dim majorTickPen As New Pen(Color.White, 3)

                                              ' Solid dark grey background for the ruler
                                              Dim rulerBrush As New SolidBrush(Color.FromArgb(40, 40, 40)) ' <--- REMOVED TRANSPARENCY
                                              Dim rulerFont As New Font("Consolas", 9, FontStyle.Bold)
                                              Dim textBrush As New SolidBrush(Color.White)

                                              Dim rulerThickness As Integer = 30

                                              ' Top Ruler
                                              g.FillRectangle(rulerBrush, 0, 0, f.Width, rulerThickness)
                                              For i As Integer = 0 To f.Width Step 25
                                                  If i Mod 100 = 0 Then
                                                      g.DrawLine(majorTickPen, i, 0, i, 20)
                                                      g.DrawString(i.ToString(), rulerFont, textBrush, i + 2, 2)
                                                  ElseIf i Mod 50 = 0 Then
                                                      g.DrawLine(tickPen, i, 0, i, 15)
                                                  Else
                                                      g.DrawLine(tickPen, i, 0, i, 10)
                                                  End If
                                              Next

                                              ' Bottom Ruler
                                              g.FillRectangle(rulerBrush, 0, f.Height - rulerThickness, f.Width, rulerThickness)
                                              For i As Integer = 0 To f.Width Step 25
                                                  If i Mod 100 = 0 Then
                                                      g.DrawLine(majorTickPen, i, f.Height - 20, i, f.Height)
                                                      g.DrawString(i.ToString(), rulerFont, textBrush, i + 2, f.Height - 28)
                                                  ElseIf i Mod 50 = 0 Then
                                                      g.DrawLine(tickPen, i, f.Height - 15, i, f.Height)
                                                  Else
                                                      g.DrawLine(tickPen, i, f.Height - 10, i, f.Height)
                                                  End If
                                              Next

                                              ' Left Ruler
                                              g.FillRectangle(rulerBrush, 0, 0, rulerThickness, f.Height)
                                              For i As Integer = 0 To f.Height Step 25
                                                  If i Mod 100 = 0 Then
                                                      g.DrawLine(majorTickPen, 0, i, 20, i)
                                                      g.DrawString(i.ToString(), rulerFont, textBrush, 2, i + 2)
                                                  ElseIf i Mod 50 = 0 Then
                                                      g.DrawLine(tickPen, 0, i, 15, i)
                                                  Else
                                                      g.DrawLine(tickPen, 0, i, 10, i)
                                                  End If
                                              Next

                                              ' Right Ruler
                                              g.FillRectangle(rulerBrush, f.Width - rulerThickness, 0, rulerThickness, f.Height)
                                              For i As Integer = 0 To f.Height Step 25
                                                  If i Mod 100 = 0 Then
                                                      g.DrawLine(majorTickPen, f.Width - 20, i, f.Width, i)
                                                      g.DrawString(i.ToString(), rulerFont, textBrush, f.Width - 28, i + 2)
                                                  ElseIf i Mod 50 = 0 Then
                                                      g.DrawLine(tickPen, f.Width - 15, i, f.Width, i)
                                                  Else
                                                      g.DrawLine(tickPen, f.Width - 10, i, f.Width, i)
                                                  End If
                                              Next

                                              ' --- DRAW INFO BOX (Bottom Left) ---
                                              Dim boxW As Integer = 250
                                              Dim boxH As Integer = 150
                                              Dim boxX As Integer = 50
                                              Dim boxY As Integer = f.Height - boxH - 50

                                              ' Solid dark grey for the box
                                              g.FillRectangle(New SolidBrush(Color.FromArgb(40, 40, 40)), boxX, boxY, boxW, boxH) ' <--- REMOVED TRANSPARENCY
                                              g.DrawRectangle(New Pen(Color.Gray, 2), boxX, boxY, boxW, boxH)

                                              ' Big Number
                                              Dim numFont As New Font("Segoe UI", 72, FontStyle.Regular)
                                              Dim numSize = g.MeasureString(num, numFont)
                                              g.DrawString(num, numFont, Brushes.White, boxX + (boxW - numSize.Width) / 2, boxY + (boxH - numSize.Height) / 2)

                                              ' Resolution
                                              Dim resFont As New Font("Segoe UI", 14, FontStyle.Bold)
                                              g.DrawString(dev.dmPelsWidth & " x " & dev.dmPelsHeight, resFont, Brushes.LightGreen, boxX + 10, boxY + 10)

                                              ' Coordinates
                                              Dim coordFont As New Font("Segoe UI", 10, FontStyle.Regular)
                                              g.DrawString("X: " & dev.dmPositionX & ", Y: " & dev.dmPositionY, coordFont, Brushes.Gray, boxX + 10, boxY + boxH - 25)
                                          End Sub

                overlay.Show()
                identifyForms.Add(overlay)
            End If
        Next
    End Sub

    ' Helper sub to close the screens when the timer finishes
    Private Sub CloseIdentifyForms(sender As Object, e As EventArgs)
        identifyTimer.Stop()
        For Each f As Form In identifyForms
            f.Close()
        Next
        identifyForms.Clear()
    End Sub

    Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If lvDisplays.Visible Then
            ' If we are on the Displays tab, save display settings
            SaveDisplaySettings()
        Else
            ' Otherwise, save window settings
            btnSave.Enabled = False
            btnSaveAll.Enabled = False

            GetWindowList()

            Dim lines As New List(Of String)
            Dim profileName As String = cboProfiles.SelectedItem.ToString()

            If System.IO.File.Exists("Settings.ini") Then
                lines = System.IO.File.ReadAllLines("Settings.ini").ToList

                For Each line In lines.ToList
                    Dim specs() = line.Split(",")

                    If (cboProfiles.SelectedItem = "Default" And specs.Length = 6) OrElse (specs.Length = 7 AndAlso specs(6) = cboProfiles.SelectedItem) Then
                        lines.Remove(line)
                    End If
                Next
            End If

            For Each item As ListViewItem In lvApps.Items
                If item.Checked Then
                    Dim hWnd As IntPtr = WindowList(item.Text).MainWindowHandle
                    Dim wpTemp As WINDOWPLACEMENT
                    Dim rectTemp As RECT

                    ' Make sure the length is set so GetWindowPlacement works
                    wpTemp.Length = Marshal.SizeOf(GetType(WINDOWPLACEMENT))

                    GetWindowPlacement(hWnd, wpTemp)
                    GetWindowRect(hWnd, rectTemp)

                    lines.Add(item.Text & "," & rectTemp.Left & "," & rectTemp.Top & "," & rectTemp.Bottom & "," & rectTemp.Right & "," & wpTemp.showCmd & "," & cboProfiles.SelectedItem)
                End If
            Next

            System.IO.File.WriteAllLines("Settings.ini", lines)
            btnSave.Enabled = True
            btnSaveAll.Enabled = True
            Dim msg As New frmDarkMsgBox("Success", "Window layout saved to profile: " & profileName)
            msg.ShowDialog()
        End If
    End Sub

    Sub GetWindowList()
        Dim pList() = Process.GetProcesses
        WindowList.Clear()

        For Each proc As Process In pList
            If Not String.IsNullOrEmpty(proc.MainWindowTitle) Then
                Try
                    If Not WindowList.ContainsKey(proc.ProcessName) And proc.Responding And proc.ProcessName <> "ApplicationFrameHostx" And proc.ProcessName <> "TANWindowMgr" And proc.MainWindowTitle <> "" Then WindowList.Add(proc.ProcessName, proc)
                Catch ex As Exception
                    Debug.WriteLine(ex.Message)
                End Try
            End If
        Next
    End Sub

    Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        GetWindowList()

        For Each window As KeyValuePair(Of String, Process) In WindowList
            Dim lvi As New ListViewItem(window.Key)
            lvi.SubItems.Add(window.Value.MainWindowTitle)
            lvApps.Items.Add(lvi)
        Next

        ApplyWindowsTheme()
        lv_Resize(lvApps, EventArgs.Empty)
        lv_Resize(lvDisplays, EventArgs.Empty)
        UpdateAutoLoadButtonState()
        LoadStartupSetting()
    End Sub

    Sub RestoreMenuProfile(sender As Object, e As EventArgs)
        Dim profName As String = CType(sender, ToolStripMenuItem).Tag
        RestoreSettings(profName)
    End Sub

    Sub RestoreSettings(ProfName As String)
        If System.IO.File.Exists("Settings.ini") Then
            GetWindowList()

            Dim file As StreamReader = My.Computer.FileSystem.OpenTextFileReader("Settings.ini")

            Do Until file.EndOfStream
                Dim line = file.ReadLine()
                Dim specs() = line.Split(",")

                If (ProfName.ToLower = "default" And specs.Length = 6) OrElse (specs.Length = 7 AndAlso specs(6).ToLower = ProfName.ToLower) Then
                    If WindowList.ContainsKey(specs(0)) Then
                        Dim hWnd As IntPtr = WindowList(specs(0)).MainWindowHandle

                        If hWnd <> IntPtr.Zero Then
                            ShowWindow(hWnd, 9) ' SW_RESTORE = 9

                            Dim l As Integer = CInt(specs(1))
                            Dim t As Integer = CInt(specs(2))
                            Dim b As Integer = CInt(specs(3))
                            Dim r As Integer = CInt(specs(4))
                            SetWindowPos(hWnd, IntPtr.Zero, l, t, r - l, b - t, &H4 Or &H40)

                            SetForegroundWindow(hWnd)
                        End If
                    End If

                ElseIf specs.Length = 9 AndAlso specs(0) = "Display" AndAlso specs(8).ToLower = ProfName.ToLower Then
                    Dim monitorName As String = specs(1)
                    Dim w As Integer = CInt(specs(2))
                    Dim h As Integer = CInt(specs(3))
                    Dim orient As Integer = CInt(specs(4))
                    Dim x As Integer = CInt(specs(5))
                    Dim y As Integer = CInt(specs(6))
                    Dim rate As Integer = CInt(specs(7))
                    Dim dm As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(monitorName)

                    dm.dmPositionX = x
                    dm.dmPositionY = y
                    dm.dmPelsWidth = w
                    dm.dmPelsHeight = h
                    dm.dmDisplayOrientation = orient
                    dm.dmDisplayFrequency = rate

                    dm.dmFields = DisplayManager.DM_POSITION Or DisplayManager.DM_PELSWIDTH Or DisplayManager.DM_PELSHEIGHT Or DisplayManager.DM_DISPLAYORIENTATION Or DisplayManager.DM_DISPLAYFREQUENCY

                    ' Apply the changes physically
                    DisplayManager.ChangeDisplaySettingsEx(monitorName, dm, IntPtr.Zero, DisplayManager.CDS_UPDATEREGISTRY, IntPtr.Zero)
                End If
            Loop

            file.Close()

            ' If the Displays tab is currently open, refresh the UI so the map jumps to the new positions
            If pnlTopology.Visible Then
                LoadDisplaysUI()
                LoadTopologyMap()
            End If

        Else
            Dim msg As New frmDarkMsgBox("Error", "Settings file does not exist!")
            msg.ShowDialog()
        End If
    End Sub

    Sub CheckAllWindows()
        For Each item As ListViewItem In lvApps.Items
            item.Checked = True
        Next
    End Sub

    Sub btnSaveAll_Click(sender As Object, e As EventArgs) Handles btnSaveAll.Click
        If lvDisplays.Visible Then
            SaveDisplaySettings()
        Else
            CheckAllWindows()
            btnSave.PerformClick()
        End If
    End Sub

    Private Sub btnDelProfile_Click(sender As Object, e As EventArgs) Handles btnDelProfile.Click
        If cboProfiles.SelectedItem = "Default" Then
            Dim msg As New frmDarkMsgBox("Error", "You can't delete the Default profile.")
            msg.ShowDialog()
        Else
            Dim lines As List(Of String) = System.IO.File.ReadAllLines("Settings.ini").ToList

            For Each line In lines.ToList
                Dim specs() = line.Split(",")
                If (specs.Length = 7 AndAlso specs(6) = cboProfiles.SelectedItem) Then
                    lines.Remove(line)
                End If
            Next

            cboProfiles.Items.Remove(cboProfiles.SelectedItem)
            cboProfiles.SelectedIndex = 0
            System.IO.File.WriteAllLines("Settings.ini", lines)
        End If
    End Sub
    Private Sub btnAddProfile_Click(sender As Object, e As EventArgs) Handles btnAddProfile.Click
        Dim inputDlg As New frmDarkInputBox("Add Profile", "Supply a profile name")

        If inputDlg.ShowDialog() = DialogResult.OK Then
            Dim response As String = inputDlg.InputText.Replace(",", "").Replace("""", "").Trim

            If response.ToLower = "default" Then
                Dim msg As New frmDarkMsgBox("Error", "'Default' is a reserved profile name.")
                msg.ShowDialog()
            ElseIf cboProfiles.Items.Contains(response) Then
                Dim msg As New frmDarkMsgBox("Error", "This profile name already exists.")
                msg.ShowDialog()
            ElseIf response <> "" Then
                ' 1. Add to the UI
                cboProfiles.Items.Add(response)
                cboProfiles.SelectedItem = response

                ' 2. Save a dummy line to Settings.ini so it persists
                Dim lines As New List(Of String)
                If System.IO.File.Exists("Settings.ini") Then lines = System.IO.File.ReadAllLines("Settings.ini").ToList()

                ' This line has 7 parts, so the tray menu will read it, but the app will skip it when loading windows
                lines.Add($",0,0,0,0,0,{response}")
                System.IO.File.WriteAllLines("Settings.ini", lines)
            End If
        End If
    End Sub

    Private Sub btnSetAuto_Click(sender As Object, e As EventArgs) Handles btnSetAuto.Click
        If cboProfiles.SelectedItem Is Nothing Then Return

        Dim selectedProf As String = cboProfiles.SelectedItem.ToString()
        Dim lines As New List(Of String)
        Dim isCurrentlyActive As Boolean = False

        ' 1. Read the file and check if this profile is already active
        If System.IO.File.Exists("Settings.ini") Then
            lines = System.IO.File.ReadAllLines("Settings.ini").ToList()
            For Each line In lines.ToList()
                If line.StartsWith("AutoLoadProfile,") Then
                    Dim activeProf = line.Split(","c)(1).Trim()
                    If activeProf.Equals(selectedProf, StringComparison.OrdinalIgnoreCase) Then
                        isCurrentlyActive = True
                    End If
                    lines.Remove(line) ' Always remove the old line first
                End If
            Next
        End If

        ' 2. Toggle the state
        If isCurrentlyActive Then
            ' CLEAR THE AUTO LOAD
            System.IO.File.WriteAllLines("Settings.ini", lines)

            Dim msg As New frmDarkMsgBox("Auto Load Cleared", "The profile '" & selectedProf & "' is no longer set to auto-load.")
            msg.ShowDialog()

        Else
            ' SET THE AUTO LOAD
            lines.Insert(0, "AutoLoadProfile," & selectedProf)
            System.IO.File.WriteAllLines("Settings.ini", lines)

            Dim msg As New frmDarkMsgBox("Auto Load Set", "'" & selectedProf & "' is now the active Auto-Restore profile!")
            msg.ShowDialog()
        End If

        ' 3. Update the button text
        UpdateAutoLoadButtonState()
    End Sub
    ' --- AUTO LOAD BUTTON STATE LOGIC ---
    Private Sub UpdateAutoLoadButtonState()
        If cboProfiles.SelectedItem Is Nothing Then Return
        Dim currentProfile As String = cboProfiles.SelectedItem.ToString()
        Dim isActive As Boolean = False

        If System.IO.File.Exists("Settings.ini") Then
            For Each line In System.IO.File.ReadAllLines("Settings.ini")
                If line.StartsWith("AutoLoadProfile,") Then
                    Dim activeProf = line.Split(","c)(1).Trim()
                    If activeProf.Equals(currentProfile, StringComparison.OrdinalIgnoreCase) Then
                        isActive = True
                    End If
                    Exit For
                End If
            Next
        End If

        If isActive Then
            btnSetAuto.Text = "Clear Auto Load"
        Else
            btnSetAuto.Text = "Set Auto Load"
        End If
    End Sub
    ' --- STARTUP REGISTRY LOGIC ---

    Private ReadOnly startupFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
    Private ReadOnly shortcutPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "WindowsArrangementManager.lnk")

    Private Sub LoadStartupSetting()
        ' Check if the shortcut exists in the Startup folder
        chkStartup.Checked = System.IO.File.Exists(shortcutPath)
    End Sub

    Private Sub chkStartup_CheckedChanged(sender As Object, e As EventArgs) Handles chkStartup.CheckedChanged
        Try
            If chkStartup.Checked Then
                ' Create the shortcut
                Dim WshShell As New WshShell()
                Dim shortcut As IWshShortcut = CType(WshShell.CreateShortcut(shortcutPath), IWshShortcut)

                shortcut.TargetPath = System.Windows.Forms.Application.ExecutablePath
                shortcut.WorkingDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)
                shortcut.Description = "Windows Arrangement Manager"
                shortcut.Save()
            Else
                ' Delete the shortcut
                If System.IO.File.Exists(shortcutPath) Then
                    System.IO.File.Delete(shortcutPath)
                End If
            End If
        Catch ex As Exception
            Dim msg As New frmDarkMsgBox("Error", "Could not update startup shortcut: " & ex.Message)
            msg.ShowDialog()
        End Try
    End Sub
    Private Sub btnTabWindows_Click(sender As Object, e As EventArgs) Handles btnTabWindows.Click
        lvDisplays.Visible = False
        pnlTopology.Visible = False
        lvApps.Visible = True
    End Sub

    Private Sub btnTabDisplays_Click(sender As Object, e As EventArgs) Handles btnTabDisplays.Click
        Try
            lvApps.Visible = False
            lvDisplays.Visible = True
            pnlTopology.Visible = True

            lvDisplays.BringToFront()
            pnlTopology.BringToFront()
            btnResetLayout.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
            btnApplyTopology.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

            btnIdentify.BringToFront()
            btnApplyTopology.BringToFront()
            LoadDisplaysUI()
            LoadTopologyMap()

        Catch ex As Exception
            Dim msg As New frmDarkMsgBox("Error", "Error: " & ex.Message)
            msg.ShowDialog()
        End Try
    End Sub
    Private Sub btnApplyTopology_Click(sender As Object, e As EventArgs) Handles btnApplyTopology.Click
        ' 1. Backup the current physical layout before changing it
        displayBackup.Clear()
        For Each item As ListViewItem In lvDisplays.Items
            Dim monitorName As String = item.Tag.ToString()
            displayBackup(monitorName) = DisplayManager.GetCurrentSettings(monitorName)
        Next

        ' 2. Apply the new layout
        ApplyTopology()
    End Sub

    ' --- PASTE THE RESET CODE RIGHT HERE ---
    Private Sub btnResetLayout_Click(sender As Object, e As EventArgs) Handles btnResetLayout.Click
        ' Check if we actually have a backup to revert to
        If displayBackup.Count = 0 Then
            Dim msg As New frmDarkMsgBox("Info", "No previous layout to revert to. Try moving monitors and clicking Apply first.")
            msg.ShowDialog()
            Return
        End If

        ' Loop through the backup and apply the old settings
        For Each kvp In displayBackup
            Dim monitorName As String = kvp.Key
            Dim dm As DisplayManager.DEVMODE = kvp.Value

            ' Tell Windows to change position, resolution, etc., back to the backup
            dm.dmFields = DisplayManager.DM_POSITION Or DisplayManager.DM_PELSWIDTH Or DisplayManager.DM_PELSHEIGHT Or DisplayManager.DM_DISPLAYORIENTATION Or DisplayManager.DM_DISPLAYFREQUENCY

            DisplayManager.ChangeDisplaySettingsEx(monitorName, dm, IntPtr.Zero, DisplayManager.CDS_UPDATEREGISTRY, IntPtr.Zero)
        Next

        ' Clear the backup so you can't undo the same thing twice
        displayBackup.Clear()

        ' Refresh the UI and map
        LoadDisplaysUI()
        LoadTopologyMap()

        ' Keep buttons on top
        btnIdentify.BringToFront()
        btnApplyTopology.BringToFront()
        btnResetLayout.BringToFront()
    End Sub
    ' --- DISPLAY SAVING LOGIC ---
    Private Sub LoadDisplaysUI()
        lvDisplays.Items.Clear()

        Dim allScreens = Screen.AllScreens.ToList()
        Dim primaryScreen = allScreens.FirstOrDefault(Function(s) s.Primary)
        Dim orderedScreens As New List(Of Screen)

        If primaryScreen IsNot Nothing Then
            orderedScreens.Add(primaryScreen)
            allScreens.Remove(primaryScreen)
        End If

        Dim otherScreens = allScreens.OrderBy(Function(s) s.Bounds.X).ThenBy(Function(s) s.Bounds.Y)
        orderedScreens.AddRange(otherScreens)

        Dim displayIndex As Integer = 1
        For Each scr As Screen In orderedScreens
            Dim dm As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(scr.DeviceName)

            Dim orientation As String = "Landscape"
            If dm.dmDisplayOrientation = 1 Then orientation = "Portrait"
            If dm.dmDisplayOrientation = 2 Then orientation = "Landscape (Flipped)"
            If dm.dmDisplayOrientation = 3 Then orientation = "Portrait (Flipped)"

            Dim friendlyName As String = "Display " & displayIndex
            If scr.Primary Then friendlyName &= " (Primary)"

            Dim lvi As New ListViewItem(friendlyName)
            lvi.Tag = scr.DeviceName

            lvi.SubItems.Add(dm.dmPelsWidth & " x " & dm.dmPelsHeight)
            lvi.SubItems.Add(dm.dmDisplayFrequency & " Hz")
            lvi.SubItems.Add(orientation)
            lvi.SubItems.Add("X: " & dm.dmPositionX & ", Y: " & dm.dmPositionY)
            lvi.Checked = checkedDisplays.Contains(scr.DeviceName)

            lvDisplays.Items.Add(lvi)
            displayIndex += 1
        Next
    End Sub

    Private Sub SaveDisplaySettings()
        If cboProfiles.SelectedItem Is Nothing Then Return
        Dim profileName As String = cboProfiles.SelectedItem.ToString()
        Dim lines As New List(Of String)

        ' 1. Read the existing Settings.ini
        If System.IO.File.Exists("Settings.ini") Then
            lines = System.IO.File.ReadAllLines("Settings.ini").ToList()

            ' Remove old display settings for THIS specific profile
            lines.RemoveAll(Function(l)
                                Dim parts = l.Split(","c)
                                Return parts.Length >= 9 AndAlso parts(0) = "Display" AndAlso parts(8) = profileName
                            End Function)
        End If

        ' 2. Add the new display settings
        For Each item As ListViewItem In lvDisplays.Items
            If item.Checked Then
                Dim monitorName As String = item.Tag.ToString()

                Dim dm As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(monitorName)

                Dim saveString As String = $"Display,{monitorName},{dm.dmPelsWidth},{dm.dmPelsHeight},{dm.dmDisplayOrientation},{dm.dmPositionX},{dm.dmPositionY},{dm.dmDisplayFrequency},{profileName}"
                lines.Add(saveString)
            End If
        Next

        ' 4. Save everything back to Settings.ini
        System.IO.File.WriteAllLines("Settings.ini", lines)
        Dim msg As New frmDarkMsgBox("Success", "Display layout saved to profile: " & profileName)
        msg.ShowDialog()
    End Sub
    ' --- TOPOLOGY MAP LOGIC ---
    Private Sub LoadTopologyMap(Optional customStartX As Integer = -1, Optional customStartY As Integer = -1)
        ' Remove old boxes, keep the Identify button safe
        For i As Integer = pnlTopology.Controls.Count - 1 To 0 Step -1
            If pnlTopology.Controls(i).Name.StartsWith("box_") Then
                pnlTopology.Controls.RemoveAt(i)
            End If
        Next
        If lvDisplays.Items.Count = 0 Then Return
        Dim minX As Integer = Integer.MaxValue
        Dim minY As Integer = Integer.MaxValue
        Dim maxX As Integer = Integer.MinValue
        Dim maxY As Integer = Integer.MinValue

        Dim screenData As New List(Of Tuple(Of String, DisplayManager.DEVMODE, String))

        For Each item As ListViewItem In lvDisplays.Items
            Dim monitorName As String = item.Tag.ToString()
            Dim dm As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(monitorName)

            If dm.dmPositionX < minX Then minX = dm.dmPositionX
            If dm.dmPositionY < minY Then minY = dm.dmPositionY
            If dm.dmPositionX + dm.dmPelsWidth > maxX Then maxX = dm.dmPositionX + dm.dmPelsWidth
            If dm.dmPositionY + dm.dmPelsHeight > maxY Then maxY = dm.dmPositionY + dm.dmPelsHeight

            screenData.Add(New Tuple(Of String, DisplayManager.DEVMODE, String)(monitorName, dm, item.Text))
        Next

        mapMinX = minX
        mapMinY = minY

        Dim totalW As Double = CDbl(maxX) - CDbl(minX)
        Dim totalH As Double = CDbl(maxY) - CDbl(minY)
        Dim availW As Double = Math.Max(100, pnlTopology.Width - 40)
        Dim availH As Double = Math.Max(100, pnlTopology.Height - 40)

        ' Calculate the base scale, then apply the zoom multiplier
        Dim baseScale As Double = Math.Max(totalW / availW, totalH / availH)
        If baseScale < 1.0 Then baseScale = 1.0

        currentScaleFactor = baseScale / zoomMultiplier

        Dim mapW As Integer = CInt(totalW / currentScaleFactor)
        Dim mapH As Integer = CInt(totalH / currentScaleFactor)

        If customStartX = -1 Then
            mapStartX = Math.Max(10, (pnlTopology.Width - mapW) \ 2)
            mapStartY = Math.Max(10, (pnlTopology.Height - mapH) \ 2)
        Else
            mapStartX = customStartX
            mapStartY = customStartY
        End If

        For Each scrData In screenData
            Dim monitorName = scrData.Item1
            Dim dm = scrData.Item2
            Dim displayText = scrData.Item3

            Dim monitorBox As New Panel()
            monitorBox.Name = "box_" & monitorName
            monitorBox.Tag = monitorName
            monitorBox.Size = New Size(CInt(dm.dmPelsWidth / currentScaleFactor), CInt(dm.dmPelsHeight / currentScaleFactor))
            monitorBox.BackColor = Color.FromArgb(50, 50, 50)
            monitorBox.BorderStyle = BorderStyle.FixedSingle
            monitorBox.Cursor = Cursors.SizeAll

            Dim lbl As New Label()
            ' --- FIXED: Only show the number ---
            lbl.Text = displayText.Replace("Display ", "").Replace(" (Primary)", "")
            ' ------------------------------------
            lbl.ForeColor = Color.White
            lbl.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            lbl.Dock = DockStyle.Fill
            lbl.TextAlign = ContentAlignment.MiddleCenter

            AddHandler lbl.MouseDown, AddressOf Monitor_MouseDown
            AddHandler lbl.MouseMove, AddressOf Monitor_MouseMove
            AddHandler lbl.MouseUp, AddressOf Monitor_MouseUp
            monitorBox.Controls.Add(lbl)

            Dim offsetX As Integer = mapStartX + CInt((dm.dmPositionX - mapMinX) / currentScaleFactor)
            Dim offsetY As Integer = mapStartY + CInt((dm.dmPositionY - mapMinY) / currentScaleFactor)
            monitorBox.Location = New Point(offsetX, offsetY)

            AddHandler monitorBox.MouseDown, AddressOf Monitor_MouseDown
            AddHandler monitorBox.MouseMove, AddressOf Monitor_MouseMove
            AddHandler monitorBox.MouseUp, AddressOf Monitor_MouseUp

            pnlTopology.Controls.Add(monitorBox)
            monitorBox.BringToFront()
        Next

        btnIdentify.BringToFront()
    End Sub

    ' Sends the new coordinates to Windows
    Private Sub ApplyTopology()
        For Each ctrl As Control In pnlTopology.Controls
            If TypeOf ctrl Is Panel AndAlso ctrl.Name.StartsWith("box_") Then
                Dim monitorName As String = ctrl.Tag.ToString()
                Dim dm As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(monitorName)

                ' Reverse the center-offset math using the dynamic scale factor
                Dim realX As Integer = CInt((ctrl.Left - mapStartX) * currentScaleFactor) + mapMinX
                Dim realY As Integer = CInt((ctrl.Top - mapStartY) * currentScaleFactor) + mapMinY

                dm.dmPositionX = realX
                dm.dmPositionY = realY
                dm.dmFields = DisplayManager.DM_POSITION

                DisplayManager.ChangeDisplaySettingsEx(monitorName, dm, IntPtr.Zero, DisplayManager.CDS_UPDATEREGISTRY, IntPtr.Zero)
            End If
        Next
    End Sub
    ' 1. Snapping Logic (Only for magnetic pulling)
    Private Sub SnapToNeighbors(panel As Panel)
        Dim others = pnlTopology.Controls.OfType(Of Panel)().Where(Function(p) p.Name.StartsWith("box_") AndAlso p IsNot panel).ToList()
        Dim snapDist = 20

        For Each other In others
            ' X-axis snapping
            If Math.Abs(panel.Left - other.Left) < snapDist Then panel.Left = other.Left
            If Math.Abs(panel.Left - other.Right) < snapDist Then panel.Left = other.Right
            If Math.Abs(panel.Right - other.Left) < snapDist Then panel.Left = other.Left - panel.Width
            If Math.Abs(panel.Right - other.Right) < snapDist Then panel.Left = other.Right - panel.Width

            ' Y-axis snapping
            If Math.Abs(panel.Top - other.Top) < snapDist Then panel.Top = other.Top
            If Math.Abs(panel.Top - other.Bottom) < snapDist Then panel.Top = other.Bottom
            If Math.Abs(panel.Bottom - other.Top) < snapDist Then panel.Top = other.Top - panel.Height
            If Math.Abs(panel.Bottom - other.Bottom) < snapDist Then panel.Top = other.Bottom - panel.Height
        Next
    End Sub

    ' 2. Overlap Logic (Always runs to push them apart)
    Private Sub ResolveOverlap(panel As Panel)
        Dim others = pnlTopology.Controls.OfType(Of Panel)().Where(Function(p) p.Name.StartsWith("box_") AndAlso p IsNot panel).ToList()

        For Each other In others
            If panel.Bounds.IntersectsWith(other.Bounds) Then
                ' Find the smallest push distance
                Dim pushLeft = Math.Abs(panel.Right - other.Left)
                Dim pushRight = Math.Abs(other.Right - panel.Left)
                Dim pushUp = Math.Abs(panel.Bottom - other.Top)
                Dim pushDown = Math.Abs(other.Bottom - panel.Top)

                Dim minPush = Math.Min(Math.Min(pushLeft, pushRight), Math.Min(pushUp, pushDown))

                If minPush = pushLeft Then panel.Left = other.Left - panel.Width
                If minPush = pushRight Then panel.Left = other.Right
                If minPush = pushUp Then panel.Top = other.Top - panel.Height
                If minPush = pushDown Then panel.Top = other.Bottom
            End If
        Next
    End Sub
    ' Drag Start
    Private Sub Monitor_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            isDragging = True

            ' Safely figure out if we clicked the box or the label
            Dim clickedControl As Control = CType(sender, Control)

            If TypeOf clickedControl Is Label Then
                activeMonitorPanel = CType(clickedControl.Parent, Panel)
            Else
                activeMonitorPanel = CType(clickedControl, Panel)
            End If
            activeMonitorPanel.BackColor = Color.FromArgb(0, 120, 215) ' Windows Blue

            dragOffset = e.Location
            activeMonitorPanel.BringToFront()
            btnIdentify.BringToFront()
        End If
    End Sub

    ' Drag Move
    Private Sub Monitor_MouseMove(sender As Object, e As MouseEventArgs)
        If isDragging AndAlso activeMonitorPanel IsNot Nothing Then
            activeMonitorPanel.Left += e.X - dragOffset.X
            activeMonitorPanel.Top += e.Y - dragOffset.Y
        End If
    End Sub

    Private Sub Monitor_MouseUp(sender As Object, e As MouseEventArgs)
        If isDragging Then
            isDragging = False
            If (Control.ModifierKeys And Keys.Shift) <> Keys.Shift Then
                SnapToNeighbors(activeMonitorPanel)
            End If
            ResolveOverlap(activeMonitorPanel)
            activeMonitorPanel.BackColor = If(IsDarkModeActive, Color.FromArgb(60, 60, 60), Color.FromArgb(200, 200, 200))
            activeMonitorPanel = Nothing
        End If
    End Sub
    Private Sub Form1_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        If (Control.ModifierKeys And Keys.Control) = Keys.Control Then
            Dim mousePos As Point = pnlTopology.PointToClient(Cursor.Position)

            If pnlTopology.ClientRectangle.Contains(mousePos) Then
                ' 1. Calculate the real-world coordinate under the mouse
                Dim realX As Double = (mousePos.X - mapStartX) * currentScaleFactor + mapMinX
                Dim realY As Double = (mousePos.Y - mapStartY) * currentScaleFactor + mapMinY

                ' 2. Adjust zoom
                If e.Delta > 0 Then
                    zoomMultiplier += 0.1
                Else
                    zoomMultiplier -= 0.1
                End If

                If zoomMultiplier < 0.5 Then zoomMultiplier = 0.5
                If zoomMultiplier > 3.0 Then zoomMultiplier = 3.0

                ' 3. We need the new scale before we can calculate the new start position
                ' Quick calculation just to get the new scale
                Dim minX As Integer = Integer.MaxValue, minY As Integer = Integer.MaxValue
                Dim maxX As Integer = Integer.MinValue, maxY As Integer = Integer.MinValue
                For Each item As ListViewItem In lvDisplays.Items
                    Dim dm = DisplayManager.GetCurrentSettings(item.Tag.ToString())
                    If dm.dmPositionX < minX Then minX = dm.dmPositionX
                    If dm.dmPositionY < minY Then minY = dm.dmPositionY
                    If dm.dmPositionX + dm.dmPelsWidth > maxX Then maxX = dm.dmPositionX + dm.dmPelsWidth
                    If dm.dmPositionY + dm.dmPelsHeight > maxY Then maxY = dm.dmPositionY + dm.dmPelsHeight
                Next

                Dim totalW As Double = maxX - minX
                Dim totalH As Double = maxY - minY
                Dim availW As Double = Math.Max(100, pnlTopology.Width - 40)
                Dim availH As Double = Math.Max(100, pnlTopology.Height - 40)

                Dim baseScale As Double = Math.Max(totalW / availW, totalH / availH)
                If baseScale < 1.0 Then baseScale = 1.0

                Dim newScale As Double = baseScale / zoomMultiplier

                ' 4. Calculate where the new top-left corner needs to be so the mouse stays over the same spot
                Dim newStartX As Integer = CInt(mousePos.X - (realX - minX) / newScale)
                Dim newStartY As Integer = CInt(mousePos.Y - (realY - minY) / newScale)

                ' 5. Reload map with the new zoom and shifted center
                LoadTopologyMap(newStartX, newStartY)

                ' Keep buttons on top
                btnIdentify.BringToFront()
                btnApplyTopology.BringToFront()
            End If
        End If
    End Sub
    ' --- PANNING THE BACKGROUND ---
    Private Sub pnlTopology_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlTopology.MouseDown
        If e.Button = MouseButtons.Left Then
            Dim hit = pnlTopology.GetChildAtPoint(e.Location)
            If hit Is Nothing OrElse Not hit.Name.StartsWith("box_") Then
                isPanning = True
                panStartMouse = e.Location
                panStartX = mapStartX
                panStartY = mapStartY
                pnlTopology.Cursor = Cursors.Hand
            End If
        End If
    End Sub

    Private Sub pnlTopology_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlTopology.MouseMove
        If isPanning Then
            Dim dx = e.X - panStartMouse.X
            Dim dy = e.Y - panStartMouse.Y

            For Each ctrl As Control In pnlTopology.Controls
                If TypeOf ctrl Is Panel AndAlso ctrl.Name.StartsWith("box_") Then
                    Dim monitorName = ctrl.Tag.ToString()
                    Dim dm = DisplayManager.GetCurrentSettings(monitorName)

                    ctrl.Left = panStartX + CInt((dm.dmPositionX - mapMinX) / currentScaleFactor) + dx
                    ctrl.Top = panStartY + CInt((dm.dmPositionY - mapMinY) / currentScaleFactor) + dy
                End If
            Next
        End If
    End Sub

    Private Sub pnlTopology_MouseUp(sender As Object, e As MouseEventArgs) Handles pnlTopology.MouseUp
        If isPanning Then
            isPanning = False
            pnlTopology.Cursor = Cursors.Default
            mapStartX = panStartX + (e.X - panStartMouse.X)
            mapStartY = panStartY + (e.Y - panStartMouse.Y)
        End If
    End Sub
    Private Sub pnlTopology_Resize(sender As Object, e As EventArgs) Handles pnlTopology.Resize
        If Not isPanning AndAlso Not isDragging AndAlso pnlTopology.Visible AndAlso lvDisplays.Items.Count > 0 Then
            LoadTopologyMap()
        End If
    End Sub
    Private Sub ApplyWindowsTheme()
        Dim isDarkMode As Boolean = False
        IsDarkModeActive = isDarkMode

        Try
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")
                If key IsNot Nothing Then
                    Dim val = key.GetValue("AppsUseLightTheme")
                    If val IsNot Nothing AndAlso CInt(val) = 0 Then
                        isDarkMode = True
                    End If
                End If
            End Using
        Catch
        End Try
        IsDarkModeActive = isDarkMode

        ' Define colors
        Dim bgColor As Color = If(isDarkMode, Color.FromArgb(32, 32, 32), SystemColors.Control)
        Dim fgColor As Color = If(isDarkMode, Color.White, SystemColors.ControlText)
        Dim panelColor As Color = If(isDarkMode, Color.FromArgb(45, 45, 48), SystemColors.Control)

        ' Apply to the main form
        Me.BackColor = bgColor
        Me.ForeColor = fgColor
        SetTitleBarDarkMode(isDarkMode)
        ' Apply to all controls recursively
        ApplyThemeToControls(Me, bgColor, fgColor, panelColor)

        ' Make the Topology Panel and ListViews look native
        pnlTopology.BackColor = panelColor
        lvApps.BackColor = If(isDarkMode, Color.FromArgb(40, 40, 40), Color.White)
        lvDisplays.BackColor = If(isDarkMode, Color.FromArgb(40, 40, 40), Color.White)

        ' Enable Custom Drawing for ListViews
        lvApps.OwnerDraw = isDarkMode
        lvDisplays.OwnerDraw = isDarkMode
        If isDarkMode Then
            SetWindowTheme(lvApps.Handle, "DarkMode_Explorer", Nothing)
            SetWindowTheme(lvDisplays.Handle, "DarkMode_Explorer", Nothing)
        Else
            SetWindowTheme(lvApps.Handle, "Explorer", Nothing)
            SetWindowTheme(lvDisplays.Handle, "Explorer", Nothing)
        End If

        ' Enable Custom Drawing for ComboBox
        cboProfiles.FlatStyle = FlatStyle.Flat
        cboProfiles.DrawMode = If(isDarkMode, DrawMode.OwnerDrawFixed, DrawMode.Normal)

        ' Update the Topology Monitor Boxes so they look good in dark mode
        For Each ctrl As Control In pnlTopology.Controls
            If TypeOf ctrl Is Panel AndAlso ctrl.Name.StartsWith("box_") Then
                ctrl.BackColor = If(isDarkMode, Color.FromArgb(60, 60, 60), Color.FromArgb(200, 200, 200))
                For Each lbl As Control In ctrl.Controls
                    lbl.ForeColor = fgColor
                Next
            End If
        Next

        ' Force the ListViews to redraw with the new colors
        lvApps.Invalidate()
        lvDisplays.Invalidate()
    End Sub
    Private Sub SetTitleBarDarkMode(isDark As Boolean)
        Dim useDarkMode As Integer = If(isDark, 1, 0)

        ' Try the newer attribute (Windows 10 20H1+ and Windows 11)
        Dim result = DwmSetWindowAttribute(Me.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, useDarkMode, Marshal.SizeOf(useDarkMode))

        ' If it fails, try the older attribute (Windows 10 1809 - 1909)
        If result <> 0 Then
            DwmSetWindowAttribute(Me.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, useDarkMode, Marshal.SizeOf(useDarkMode))
        End If
    End Sub

    Private Sub ApplyThemeToControls(parent As Control, bg As Color, fg As Color, panelBg As Color)
        For Each ctrl As Control In parent.Controls
            If TypeOf ctrl Is Button Then
                ctrl.BackColor = If(bg = Color.FromArgb(32, 32, 32), Color.FromArgb(60, 60, 60), SystemColors.Control)
                ctrl.ForeColor = fg
                CType(ctrl, Button).FlatStyle = FlatStyle.Flat
                CType(ctrl, Button).FlatAppearance.BorderColor = fg
            ElseIf TypeOf ctrl Is ComboBox OrElse TypeOf ctrl Is TextBox Then
                ctrl.BackColor = If(bg = Color.FromArgb(32, 32, 32), Color.FromArgb(45, 45, 45), Color.White)
                ctrl.ForeColor = fg
            ElseIf TypeOf ctrl Is ListView Then
                ctrl.ForeColor = fg
            Else
                ctrl.BackColor = bg
                ctrl.ForeColor = fg
            End If

            ' Go deeper
            If ctrl.HasChildren Then
                ApplyThemeToControls(ctrl, bg, fg, panelBg)
            End If
        Next
    End Sub
    Private Sub lv_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs) Handles lvDisplays.DrawColumnHeader, lvApps.DrawColumnHeader
        e.DrawDefault = False

        Dim lv As ListView = CType(sender, ListView)
        Dim rect As Rectangle = e.Bounds

        ' --- THE WHITE BOX FIX ---
        ' If this is the last column, stretch its drawing area all the way to the right edge
        If e.ColumnIndex = lv.Columns.Count - 1 Then
            rect.Width = lv.ClientSize.Width - rect.Left
        End If

        ' Draw the dark background (covers the native white gap)
        e.Graphics.FillRectangle(New SolidBrush(Color.FromArgb(45, 45, 45)), rect)

        Dim headerFont As New Font("Segoe UI", 9, FontStyle.Regular)
        Dim headerBrush As New SolidBrush(Color.White)
        Dim format As New StringFormat()
        format.LineAlignment = StringAlignment.Center

        ' Draw the text using the original bounds
        e.Graphics.DrawString(e.Header.Text, headerFont, headerBrush, e.Bounds, format)

        ' Draw the divider line
        Using pen As New Pen(Color.FromArgb(60, 60, 60))
            e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom - 1)
        End Using
    End Sub

    Private Sub lv_DrawItem(sender As Object, e As DrawListViewItemEventArgs) Handles lvDisplays.DrawItem, lvApps.DrawItem
        ' Handled in DrawSubItem
    End Sub

    Private Sub lv_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs) Handles lvDisplays.DrawSubItem, lvApps.DrawSubItem
        e.DrawDefault = False

        ' Alternate row colors (Dark gray)
        Dim rowColor As Color = Color.FromArgb(35, 35, 35)
        If e.ItemIndex Mod 2 = 0 Then
            rowColor = Color.FromArgb(40, 40, 40)
        End If

        If e.Item.Selected Then
            rowColor = Color.FromArgb(0, 120, 215) ' Windows Blue
        End If

        e.Graphics.FillRectangle(New SolidBrush(rowColor), e.Bounds)

        Dim rect As Rectangle = e.Bounds

        ' --- DRAW THE CHECKBOX IF IT'S THE FIRST COLUMN ---
        If e.ColumnIndex = 0 Then
            Dim cbRect As New Rectangle(e.Bounds.Left + 2, e.Bounds.Top + (e.Bounds.Height - 13) \ 2, 13, 13)
            ControlPaint.DrawCheckBox(e.Graphics, cbRect, If(e.Item.Checked, ButtonState.Checked, ButtonState.Normal))
            rect.X += 20 ' Shift the text over so it doesn't overlap the checkbox
        Else
            rect.X += 5
        End If

        Dim textBrush As New SolidBrush(Color.White)
        Dim cellFormat As New StringFormat()
        cellFormat.LineAlignment = StringAlignment.Center

        e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, textBrush, rect, cellFormat)
    End Sub

    Private Sub cboProfiles_DrawItem(sender As Object, e As DrawItemEventArgs) Handles cboProfiles.DrawItem
        If e.Index < 0 Then Return

        Dim bg As Color = Color.FromArgb(45, 45, 45)
        Dim textCol As Color = Color.White

        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            bg = Color.FromArgb(0, 120, 215) ' Blue highlight
        End If

        e.Graphics.FillRectangle(New SolidBrush(bg), e.Bounds)
        e.Graphics.DrawString(cboProfiles.Items(e.Index).ToString(), e.Font, New SolidBrush(textCol), e.Bounds)
    End Sub
    ' --- SMART COLUMN STRETCHING LOGIC ---
    Private Sub lv_Resize(sender As Object, e As EventArgs) Handles lvApps.Resize, lvDisplays.Resize
        Dim lv As ListView = CType(sender, ListView)
        If lv.Columns.Count = 0 OrElse lv.View <> View.Details Then Return

        lv.SuspendLayout()

        ' 1. Calculate the minimum width needed for every column
        Dim colMinWidths(lv.Columns.Count - 1) As Integer
        Using g As Graphics = lv.CreateGraphics()
            For i As Integer = 0 To lv.Columns.Count - 1
                Dim headerWidth As Integer = TextRenderer.MeasureText(g, lv.Columns(i).Text, lv.Font).Width
                Dim contentWidth As Integer = 0

                For Each item As ListViewItem In lv.Items
                    If i < item.SubItems.Count Then
                        Dim textWidth As Integer = TextRenderer.MeasureText(g, item.SubItems(i).Text, lv.Font).Width
                        If textWidth > contentWidth Then contentWidth = textWidth
                    End If
                Next

                Dim padding As Integer = 20
                If i = 0 Then padding = 35 ' Extra space for the checkbox

                colMinWidths(i) = Math.Max(headerWidth, contentWidth) + padding
            Next
        End Using

        ' 2. Calculate space
        Dim totalRequired As Integer = 0
        For i As Integer = 0 To lv.Columns.Count - 1
            totalRequired += colMinWidths(i)
        Next

        Dim availableWidth As Integer = lv.ClientSize.Width
        ' Check if scrollbar is visible
        If lv.Items.Count > 0 Then
            Dim lastItemRect = lv.GetItemRect(lv.Items.Count - 1)
            If lastItemRect.Bottom > lv.ClientSize.Height Then
                availableWidth -= SystemInformation.VerticalScrollBarWidth
            End If
        End If

        ' 3. Apply the widths
        If totalRequired < availableWidth Then
            ' Distribute extra space evenly
            Dim extraSpace As Integer = availableWidth - totalRequired
            Dim extraPerColumn As Integer = extraSpace \ lv.Columns.Count
            Dim remainder As Integer = extraSpace Mod lv.Columns.Count

            For i As Integer = 0 To lv.Columns.Count - 1
                Dim finalWidth As Integer = colMinWidths(i) + extraPerColumn
                If i < remainder Then finalWidth += 1
                lv.Columns(i).Width = finalWidth
            Next
        Else
            ' Not enough space, set minimum widths
            For i As Integer = 0 To lv.Columns.Count - 1
                lv.Columns(i).Width = colMinWidths(i)
            Next
        End If

        lv.ResumeLayout()
        lv.Invalidate()
    End Sub
    Private Sub lvDisplays_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lvDisplays.ItemCheck
        If lvDisplays.Items.Count = 0 Then Return
        Dim monitorName As String = lvDisplays.Items(e.Index).Tag.ToString()

        If e.NewValue = CheckState.Checked Then
            checkedDisplays.Add(monitorName)
        Else
            checkedDisplays.Remove(monitorName)
        End If
    End Sub
    ' --- DROPDOWN COMBOBOX LOGIC ---
    Private activeComboBox As ComboBox = Nothing

    Private Sub lvDisplays_MouseClick(sender As Object, e As MouseEventArgs) Handles lvDisplays.MouseClick
        ' If a combo box is already open, close it
        If activeComboBox IsNot Nothing Then
            lvDisplays.Controls.Remove(activeComboBox)
            activeComboBox = Nothing
        End If

        Dim hitInfo As ListViewHitTestInfo = lvDisplays.HitTest(e.X, e.Y)

        ' Only trigger if we clicked an item, on a valid subitem
        If hitInfo.Item IsNot Nothing AndAlso hitInfo.SubItem IsNot Nothing Then
            Dim colIndex As Integer = hitInfo.Item.SubItems.IndexOf(hitInfo.SubItem)

            ' Columns: 1 = Resolution, 2 = Refresh Rate, 3 = Orientation
            If colIndex = 1 OrElse colIndex = 2 OrElse colIndex = 3 Then
                Dim monitorName As String = hitInfo.Item.Tag.ToString()

                ' Create the ComboBox
                Dim cmb As New ComboBox()
                cmb.DropDownStyle = ComboBoxStyle.DropDownList
                cmb.Font = lvDisplays.Font
                cmb.DrawMode = DrawMode.OwnerDrawFixed
                AddHandler cmb.DrawItem, AddressOf DynamicComboBox_DrawItem
                If IsDarkModeActive Then
                    cmb.BackColor = Color.FromArgb(45, 45, 45)
                    cmb.ForeColor = Color.White
                End If

                ' Position it exactly over the clicked cell
                cmb.Bounds = hitInfo.SubItem.Bounds
                cmb.Tag = New Tuple(Of ListViewItem, Integer)(hitInfo.Item, colIndex)

                ' Get available modes
                Dim modes = DisplayManager.GetAvailableModes(monitorName)
                If modes.Count = 0 Then Return

                ' Get current mode to pre-select it
                Dim currentMode = DisplayManager.GetCurrentSettings(monitorName)

                ' Populate the ComboBox
                If colIndex = 1 Then ' Resolution
                    Dim resolutions = modes.Select(Function(m) $"{m.dmPelsWidth} x {m.dmPelsHeight}").Distinct().ToList()
                    cmb.Items.AddRange(resolutions.ToArray())
                    cmb.Text = $"{currentMode.dmPelsWidth} x {currentMode.dmPelsHeight}"
                ElseIf colIndex = 2 Then ' Refresh Rate
                    ' Filter by current resolution so we only show valid refresh rates for this size
                    Dim rates = modes.Where(Function(m) m.dmPelsWidth = currentMode.dmPelsWidth AndAlso m.dmPelsHeight = currentMode.dmPelsHeight).Select(Function(m) m.dmDisplayFrequency & " Hz").Distinct().ToList()
                    cmb.Items.AddRange(rates.ToArray())
                    cmb.Text = currentMode.dmDisplayFrequency & " Hz"
                ElseIf colIndex = 3 Then ' Orientation
                    cmb.Items.AddRange({"Landscape", "Portrait", "Landscape (Flipped)", "Portrait (Flipped)"})
                    cmb.Text = hitInfo.SubItem.Text
                End If

                ' Add to ListView and show
                lvDisplays.Controls.Add(cmb)
                If IsDarkModeActive Then
                    SetWindowTheme(cmb.Handle, "DarkMode_Explorer", Nothing)
                End If
                cmb.BringToFront()
                cmb.Focus()
                cmb.DroppedDown = True

                activeComboBox = cmb

                ' Handle selection
                AddHandler cmb.SelectedIndexChanged, AddressOf ComboBox_SelectedIndexChanged
            End If
        End If
    End Sub

    Private Sub ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim cmb As ComboBox = CType(sender, ComboBox)
        If cmb.SelectedIndex < 0 Then Return

        Dim data = CType(cmb.Tag, Tuple(Of ListViewItem, Integer))
        Dim item As ListViewItem = data.Item1
        Dim colIndex As Integer = data.Item2
        Dim monitorName As String = item.Tag.ToString()
        Dim newText As String = cmb.Text

        ' Update the ListView text temporarily
        item.SubItems(colIndex).Text = newText

        ' Get the original settings BEFORE we change anything (in case we need to revert)
        Dim originalMode As DisplayManager.DEVMODE = DisplayManager.GetCurrentSettings(monitorName)
        Dim newMode As DisplayManager.DEVMODE = originalMode ' Clone it

        ' Apply the change to our newMode object
        If colIndex = 1 Then ' Resolution
            Dim parts = newText.Split(" x ")
            newMode.dmPelsWidth = CInt(parts(0))
            newMode.dmPelsHeight = CInt(parts(1))
            newMode.dmFields = DisplayManager.DM_PELSWIDTH Or DisplayManager.DM_PELSHEIGHT
        ElseIf colIndex = 2 Then ' Refresh Rate
            newMode.dmDisplayFrequency = CInt(newText.Replace(" Hz", ""))
            newMode.dmFields = DisplayManager.DM_DISPLAYFREQUENCY
        ElseIf colIndex = 3 Then ' Orientation
            Dim orient As Integer = 0
            If newText = "Portrait" Then orient = 1
            If newText = "Landscape (Flipped)" Then orient = 2
            If newText = "Portrait (Flipped)" Then orient = 3
            newMode.dmDisplayOrientation = orient
            newMode.dmFields = DisplayManager.DM_DISPLAYORIENTATION
        End If

        ' 1. Apply the new settings TEMPORARILY (Flag 0 means do not write to registry)
        DisplayManager.ChangeDisplaySettingsEx(monitorName, newMode, IntPtr.Zero, 0, IntPtr.Zero)

        ' 2. Show the Confirmation Popup
        Dim confirm As New frmConfirmDisplay()
        Dim result As DialogResult = confirm.ShowDialog()

        ' 3. Keep or Revert based on user input
        If result = DialogResult.OK Then
            ' User clicked "Keep changes" -> Save permanently
            DisplayManager.ChangeDisplaySettingsEx(monitorName, newMode, IntPtr.Zero, DisplayManager.CDS_UPDATEREGISTRY, IntPtr.Zero)
        Else
            ' Timer ran out OR user clicked "Revert" -> Restore original settings
            DisplayManager.ChangeDisplaySettingsEx(monitorName, originalMode, IntPtr.Zero, DisplayManager.CDS_UPDATEREGISTRY, IntPtr.Zero)
        End If

        ' Remove the ComboBox
        lvDisplays.Controls.Remove(cmb)
        activeComboBox = Nothing

        ' Reload the map and list to reflect the FINAL state
        LoadDisplaysUI()
        LoadTopologyMap()
    End Sub

    ' Close combo box if they click somewhere else
    Private Sub lvDisplays_MouseMove(sender As Object, e As MouseEventArgs) Handles lvDisplays.MouseMove
        If activeComboBox IsNot Nothing Then
            Dim pt As Point = lvDisplays.PointToClient(Cursor.Position)
            If Not activeComboBox.Bounds.Contains(pt) AndAlso Not activeComboBox.DroppedDown Then
                lvDisplays.Controls.Remove(activeComboBox)
                activeComboBox = Nothing
            End If
        End If
    End Sub
    Private Sub DynamicComboBox_DrawItem(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 Then Return

        Dim cmb As ComboBox = CType(sender, ComboBox)

        ' (DELETE the e.DrawDefault = False line here)

        Dim bg As Color = If(IsDarkModeActive, Color.FromArgb(45, 45, 45), Color.White)
        Dim textCol As Color = If(IsDarkModeActive, Color.White, Color.Black)

        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            bg = Color.FromArgb(0, 120, 215) ' Windows Blue
        End If

        ' Stretch the background fill to the full width of the box to prevent white slivers
        Dim fillRect As New Rectangle(0, e.Bounds.Top, cmb.Width, e.Bounds.Height)
        e.Graphics.FillRectangle(New SolidBrush(bg), fillRect)

        ' Draw the text
        e.Graphics.DrawString(cmb.Items(e.Index).ToString(), e.Font, New SolidBrush(textCol), e.Bounds)
    End Sub
End Class

' --- DISPLAY CONFIRMATION POPUP ---
Public Class frmConfirmDisplay
    Inherits Form
    Private timeLeft As Integer = 15
    Private WithEvents timer As New Timer()
    Private lblMessage As Label

    Public Sub New()
        ' Form Setup
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.TopMost = True
        Me.BackColor = Color.FromArgb(32, 32, 32)
        Me.ForeColor = Color.White
        Me.ClientSize = New Size(420, 180)
        Me.Text = "Display Settings"

        ' Title
        Dim lblTitle As New Label()
        lblTitle.Text = "Keep these display settings?"
        lblTitle.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblTitle.ForeColor = Color.White
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        ' Message
        lblMessage = New Label()
        lblMessage.Text = $"Reverting to previous display settings in {timeLeft} seconds."
        lblMessage.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lblMessage.ForeColor = Color.LightGray
        lblMessage.Location = New Point(20, 60)
        lblMessage.AutoSize = True
        Me.Controls.Add(lblMessage)

        ' Revert Button
        Dim btnRevert As New Button()
        btnRevert.Text = "Revert"
        btnRevert.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        btnRevert.Size = New Size(180, 40)
        btnRevert.Location = New Point(220, 120)
        btnRevert.BackColor = Color.FromArgb(60, 60, 60)
        btnRevert.ForeColor = Color.White
        btnRevert.FlatStyle = FlatStyle.Flat
        btnRevert.FlatAppearance.BorderSize = 0
        AddHandler btnRevert.Click, Sub()
                                        Me.DialogResult = DialogResult.Cancel
                                        Me.Close()
                                    End Sub
        Me.Controls.Add(btnRevert)

        ' Keep Button
        Dim btnKeep As New Button()
        btnKeep.Text = "Keep changes"
        btnKeep.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnKeep.Size = New Size(180, 40)
        btnKeep.Location = New Point(20, 120)
        btnKeep.BackColor = Color.FromArgb(0, 120, 215) ' Windows Blue
        btnKeep.ForeColor = Color.White
        btnKeep.FlatStyle = FlatStyle.Flat
        btnKeep.FlatAppearance.BorderSize = 0
        AddHandler btnKeep.Click, Sub()
                                      Me.DialogResult = DialogResult.OK
                                      Me.Close()
                                  End Sub
        Me.Controls.Add(btnKeep)

        ' Start Timer
        timer.Interval = 1000
        timer.Start()
    End Sub

    Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
        timeLeft -= 1
        lblMessage.Text = $"Reverting to previous display settings in {timeLeft} seconds."

        If timeLeft <= 0 Then
            timer.Stop()
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        End If
    End Sub
End Class
' --- CUSTOM DARK MESSAGE BOX ---
Public Class frmDarkMsgBox
    Inherits Form

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(ByVal hwnd As IntPtr, ByVal attr As Integer, ByRef attrValue As Integer, ByVal attrSize As Integer) As Integer
    End Function

    Public Sub New(title As String, message As String)
        Me.Text = title
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.FromArgb(32, 32, 32)
        Me.ClientSize = New Size(400, 160)
        Me.TopMost = True

        ' Apply Dark Title Bar
        Dim useDarkMode As Integer = 1
        DwmSetWindowAttribute(Me.Handle, 20, useDarkMode, Marshal.SizeOf(useDarkMode))

        Dim lbl As New Label()
        lbl.Text = message
        lbl.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lbl.ForeColor = Color.White
        lbl.Location = New Point(20, 20)
        lbl.MaximumSize = New Size(360, 80)
        lbl.AutoSize = True
        Me.Controls.Add(lbl)

        Dim btnOk As New Button()
        btnOk.Text = "OK"
        btnOk.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnOk.Size = New Size(120, 35)
        btnOk.Location = New Point(260, 105)
        btnOk.BackColor = Color.FromArgb(0, 120, 215)
        btnOk.ForeColor = Color.White
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.FlatAppearance.BorderSize = 0
        AddHandler btnOk.Click, Sub() Me.Close()
        Me.Controls.Add(btnOk)
    End Sub
End Class

' --- CUSTOM DARK INPUT BOX ---
Public Class frmDarkInputBox
    Inherits Form
    Public Property InputText As String = ""

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(ByVal hwnd As IntPtr, ByVal attr As Integer, ByRef attrValue As Integer, ByVal attrSize As Integer) As Integer
    End Function

    Public Sub New(title As String, prompt As String)
        Me.Text = title
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.FromArgb(32, 32, 32)
        Me.ClientSize = New Size(400, 200)
        Me.TopMost = True

        ' Apply Dark Title Bar
        Dim useDarkMode As Integer = 1
        DwmSetWindowAttribute(Me.Handle, 20, useDarkMode, Marshal.SizeOf(useDarkMode))

        Dim lbl As New Label()
        lbl.Text = prompt
        lbl.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lbl.ForeColor = Color.White
        lbl.Location = New Point(20, 20)
        lbl.AutoSize = True
        Me.Controls.Add(lbl)

        Dim txt As New TextBox()
        txt.Font = New Font("Segoe UI", 11, FontStyle.Regular)
        txt.Location = New Point(20, 50)
        txt.Size = New Size(360, 30)
        txt.BackColor = Color.FromArgb(45, 45, 45)
        txt.ForeColor = Color.White
        txt.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txt)

        Dim btnOk As New Button()
        btnOk.Text = "OK"
        btnOk.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnOk.Size = New Size(120, 35)
        btnOk.Location = New Point(260, 140)
        btnOk.BackColor = Color.FromArgb(0, 120, 215)
        btnOk.ForeColor = Color.White
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.FlatAppearance.BorderSize = 0
        AddHandler btnOk.Click, Sub()
                                    InputText = txt.Text
                                    Me.DialogResult = DialogResult.OK
                                    Me.Close()
                                End Sub
        Me.Controls.Add(btnOk)

        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        btnCancel.Size = New Size(120, 35)
        btnCancel.Location = New Point(130, 140)
        btnCancel.BackColor = Color.FromArgb(60, 60, 60)
        btnCancel.ForeColor = Color.White
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderSize = 0
        AddHandler btnCancel.Click, Sub()
                                        Me.DialogResult = DialogResult.Cancel
                                        Me.Close()
                                    End Sub
        Me.Controls.Add(btnCancel)

        Me.AcceptButton = btnOk
    End Sub
End Class
Public Class RulerOverlayForm
    Inherits Form

    ' This makes the form completely click-through so you can still use your PC!
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or &H80000 Or &H20 ' WS_EX_LAYERED | WS_EX_TRANSPARENT
            Return cp
        End Get
    End Property
End Class