Imports System.Runtime.InteropServices

Public Class DisplayManager
    ' The DEVMODE structure is used by Windows to hold display settings
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    Public Structure DEVMODE
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)>
        Public dmDeviceName As String
        Public dmSpecVersion As Short
        Public dmDriverVersion As Short
        Public dmSize As Short
        Public dmDriverExtra As Short
        Public dmFields As Integer
        Public dmPositionX As Integer
        Public dmPositionY As Integer
        Public dmDisplayOrientation As Integer
        Public dmDisplayFixedOutput As Integer
        Public dmColor As Short
        Public dmDuplex As Short
        Public dmYResolution As Short
        Public dmTTOption As Short
        Public dmCollate As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)>
        Public dmFormName As String
        Public dmLogPixels As Short
        Public dmBitsPerPel As Integer
        Public dmPelsWidth As Integer
        Public dmPelsHeight As Integer
        Public dmDisplayFlags As Integer
        Public dmDisplayFrequency As Integer
        Public Const DM_DISPLAYFREQUENCY As Integer = &H400000
    End Structure

    ' Constants for changing display settings
    Public Const ENUM_CURRENT_SETTINGS As Integer = -1
    Public Const CDS_UPDATEREGISTRY As Integer = &H1
    Public Const CDS_TEST As Integer = &H2

    ' Constants for the dmFields flag (tells Windows what we are changing)
    Public Const DM_PELSWIDTH As Integer = &H80000
    Public Const DM_PELSHEIGHT As Integer = &H100000
    Public Const DM_DISPLAYORIENTATION As Integer = &H80
    Public Const DM_POSITION As Integer = &H20
    Public Const DM_DISPLAYFREQUENCY As Integer = &H400000

    <DllImport("user32.dll", CharSet:=CharSet.Ansi)>
    Public Shared Function EnumDisplaySettings(ByVal lpszDeviceName As String, ByVal iModeNum As Integer, ByRef lpDevMode As DEVMODE) As Boolean
    End Function

    ' API to apply new display settings
    <DllImport("user32.dll", CharSet:=CharSet.Ansi)>
    Public Shared Function ChangeDisplaySettingsEx(ByVal lpszDeviceName As String, ByRef lpDevMode As DEVMODE, ByVal hwnd As IntPtr, ByVal dwflags As Integer, ByVal lParam As IntPtr) As Integer
    End Function

    ' Helper function to get the current settings for a specific monitor
    Public Shared Function GetCurrentSettings(ByVal deviceName As String) As DEVMODE
        Dim dm As New DEVMODE()
        dm.dmSize = CShort(Marshal.SizeOf(GetType(DEVMODE)))
        EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, dm)
        Return dm
    End Function
    Public Shared Function GetAvailableModes(deviceName As String) As List(Of DEVMODE)
        Dim modes As New List(Of DEVMODE)
        Dim dm As New DEVMODE()
        dm.dmSize = CShort(Marshal.SizeOf(GetType(DEVMODE)))
        Dim i As Integer = 0

        ' Loop through all available modes
        While EnumDisplaySettings(deviceName, i, dm)
            ' Only add 32-bit modes to prevent duplicates (Windows often lists 16-bit and 8-bit duplicates)
            If dm.dmBitsPerPel = 32 Then
                modes.Add(dm)
            End If
            i += 1
        End While
        Return modes
    End Function
End Class