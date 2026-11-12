<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.lvApps = New System.Windows.Forms.ListView()
        Me.Application = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.WindowTitle = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.btnSave = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnSaveAll = New System.Windows.Forms.Button()
        Me.cboProfiles = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnDelProfile = New System.Windows.Forms.Button()
        Me.btnAddProfile = New System.Windows.Forms.Button()
        Me.btnSetAuto = New System.Windows.Forms.Button()
        Me.btnTabWindows = New System.Windows.Forms.Button()
        Me.btnTabDisplays = New System.Windows.Forms.Button()
        Me.lvDisplays = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.pnlTopology = New System.Windows.Forms.Panel()
        Me.btnResetLayout = New System.Windows.Forms.Button()
        Me.btnApplyTopology = New System.Windows.Forms.Button()
        Me.btnIdentify = New System.Windows.Forms.Button()
        Me.chkStartup = New System.Windows.Forms.CheckBox()
        Me.pnlTopology.SuspendLayout()
        Me.SuspendLayout()
        '
        'lvApps
        '
        Me.lvApps.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lvApps.CheckBoxes = True
        Me.lvApps.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Application, Me.WindowTitle})
        Me.lvApps.HideSelection = False
        Me.lvApps.Location = New System.Drawing.Point(12, 29)
        Me.lvApps.Name = "lvApps"
        Me.lvApps.Size = New System.Drawing.Size(562, 286)
        Me.lvApps.TabIndex = 0
        Me.lvApps.UseCompatibleStateImageBehavior = False
        Me.lvApps.View = System.Windows.Forms.View.Details
        '
        'Application
        '
        Me.Application.Text = "Application"
        Me.Application.Width = 120
        '
        'WindowTitle
        '
        Me.WindowTitle.Text = "Window Title"
        Me.WindowTitle.Width = 354
        '
        'btnSave
        '
        Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSave.Location = New System.Drawing.Point(377, 320)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(88, 23)
        Me.btnSave.TabIndex = 1
        Me.btnSave.Text = "Save Selected"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(0, 13)
        Me.Label1.TabIndex = 2
        '
        'btnSaveAll
        '
        Me.btnSaveAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSaveAll.Location = New System.Drawing.Point(296, 320)
        Me.btnSaveAll.Name = "btnSaveAll"
        Me.btnSaveAll.Size = New System.Drawing.Size(75, 23)
        Me.btnSaveAll.TabIndex = 4
        Me.btnSaveAll.Text = "Save All"
        Me.btnSaveAll.UseVisualStyleBackColor = True
        '
        'cboProfiles
        '
        Me.cboProfiles.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.cboProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboProfiles.FormattingEnabled = True
        Me.cboProfiles.Location = New System.Drawing.Point(50, 321)
        Me.cboProfiles.Name = "cboProfiles"
        Me.cboProfiles.Size = New System.Drawing.Size(118, 21)
        Me.cboProfiles.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 324)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(36, 13)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "Profile"
        '
        'btnDelProfile
        '
        Me.btnDelProfile.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnDelProfile.BackColor = System.Drawing.Color.Transparent
        Me.btnDelProfile.FlatAppearance.BorderSize = 0
        Me.btnDelProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent
        Me.btnDelProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent
        Me.btnDelProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDelProfile.ForeColor = System.Drawing.Color.Transparent
        Me.btnDelProfile.Image = Global.WindowsArrangementMgr.My.Resources.Resources.x
        Me.btnDelProfile.Location = New System.Drawing.Point(189, 320)
        Me.btnDelProfile.Name = "btnDelProfile"
        Me.btnDelProfile.Size = New System.Drawing.Size(20, 23)
        Me.btnDelProfile.TabIndex = 7
        Me.btnDelProfile.UseVisualStyleBackColor = False
        '
        'btnAddProfile
        '
        Me.btnAddProfile.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnAddProfile.BackColor = System.Drawing.Color.Transparent
        Me.btnAddProfile.FlatAppearance.BorderSize = 0
        Me.btnAddProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent
        Me.btnAddProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent
        Me.btnAddProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAddProfile.ForeColor = System.Drawing.Color.Transparent
        Me.btnAddProfile.Image = WindowsArrangementMgr.My.Resources.Resources.greenPlus
        Me.btnAddProfile.Location = New System.Drawing.Point(171, 320)
        Me.btnAddProfile.Name = "btnAddProfile"
        Me.btnAddProfile.Size = New System.Drawing.Size(20, 23)
        Me.btnAddProfile.TabIndex = 8
        Me.btnAddProfile.UseVisualStyleBackColor = False
        '
        'btnSetAuto
        '
        Me.btnSetAuto.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSetAuto.Location = New System.Drawing.Point(471, 320)
        Me.btnSetAuto.Name = "btnSetAuto"
        Me.btnSetAuto.Size = New System.Drawing.Size(100, 23)
        Me.btnSetAuto.TabIndex = 9
        Me.btnSetAuto.Text = "Set Auto Load"
        Me.btnSetAuto.UseVisualStyleBackColor = True
        '
        'btnTabWindows
        '
        Me.btnTabWindows.Location = New System.Drawing.Point(15, 5)
        Me.btnTabWindows.Name = "btnTabWindows"
        Me.btnTabWindows.Size = New System.Drawing.Size(75, 23)
        Me.btnTabWindows.TabIndex = 10
        Me.btnTabWindows.Text = "Windows"
        Me.btnTabWindows.UseVisualStyleBackColor = True
        '
        'btnTabDisplays
        '
        Me.btnTabDisplays.Location = New System.Drawing.Point(93, 5)
        Me.btnTabDisplays.Name = "btnTabDisplays"
        Me.btnTabDisplays.Size = New System.Drawing.Size(75, 23)
        Me.btnTabDisplays.TabIndex = 11
        Me.btnTabDisplays.Text = "Displays"
        Me.btnTabDisplays.UseVisualStyleBackColor = True
        '
        'lvDisplays
        '
        Me.lvDisplays.AllowColumnReorder = True
        Me.lvDisplays.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lvDisplays.CheckBoxes = True
        Me.lvDisplays.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader5, Me.ColumnHeader3, Me.ColumnHeader4})
        Me.lvDisplays.FullRowSelect = True
        Me.lvDisplays.HideSelection = False
        Me.lvDisplays.Location = New System.Drawing.Point(12, 198)
        Me.lvDisplays.Name = "lvDisplays"
        Me.lvDisplays.Size = New System.Drawing.Size(562, 117)
        Me.lvDisplays.TabIndex = 12
        Me.lvDisplays.UseCompatibleStateImageBehavior = False
        Me.lvDisplays.View = System.Windows.Forms.View.Details
        Me.lvDisplays.Visible = False
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Display"
        Me.ColumnHeader1.Width = 120
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Resolution"
        Me.ColumnHeader2.Width = 80
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Refresh Rate"
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Orientation"
        Me.ColumnHeader3.Width = 120
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Arrangement"
        Me.ColumnHeader4.Width = 100
        '
        'pnlTopology
        '
        Me.pnlTopology.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlTopology.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTopology.Controls.Add(Me.btnResetLayout)
        Me.pnlTopology.Controls.Add(Me.btnApplyTopology)
        Me.pnlTopology.Controls.Add(Me.btnIdentify)
        Me.pnlTopology.Location = New System.Drawing.Point(12, 29)
        Me.pnlTopology.Name = "pnlTopology"
        Me.pnlTopology.Size = New System.Drawing.Size(562, 175)
        Me.pnlTopology.TabIndex = 14
        Me.pnlTopology.Visible = False
        '
        'btnResetLayout
        '
        Me.btnResetLayout.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnResetLayout.Location = New System.Drawing.Point(483, 147)
        Me.btnResetLayout.Name = "btnResetLayout"
        Me.btnResetLayout.Size = New System.Drawing.Size(75, 23)
        Me.btnResetLayout.TabIndex = 17
        Me.btnResetLayout.Text = "Reset"
        Me.btnResetLayout.UseVisualStyleBackColor = True
        '
        'btnApplyTopology
        '
        Me.btnApplyTopology.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnApplyTopology.Location = New System.Drawing.Point(402, 147)
        Me.btnApplyTopology.Name = "btnApplyTopology"
        Me.btnApplyTopology.Size = New System.Drawing.Size(75, 23)
        Me.btnApplyTopology.TabIndex = 16
        Me.btnApplyTopology.Text = "Apply"
        Me.btnApplyTopology.UseVisualStyleBackColor = True
        '
        'btnIdentify
        '
        Me.btnIdentify.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnIdentify.Location = New System.Drawing.Point(3, 147)
        Me.btnIdentify.Name = "btnIdentify"
        Me.btnIdentify.Size = New System.Drawing.Size(75, 23)
        Me.btnIdentify.TabIndex = 15
        Me.btnIdentify.Text = "Identify"
        Me.btnIdentify.UseVisualStyleBackColor = True
        '
        'chkStartup
        '
        Me.chkStartup.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.chkStartup.AutoSize = True
        Me.chkStartup.Location = New System.Drawing.Point(225, 324)
        Me.chkStartup.Name = "chkStartup"
        Me.chkStartup.Size = New System.Drawing.Size(65, 17)
        Me.chkStartup.TabIndex = 15
        Me.chkStartup.Text = "Start Up"
        Me.chkStartup.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(586, 346)
        Me.Controls.Add(Me.chkStartup)
        Me.Controls.Add(Me.pnlTopology)
        Me.Controls.Add(Me.lvDisplays)
        Me.Controls.Add(Me.btnTabDisplays)
        Me.Controls.Add(Me.btnTabWindows)
        Me.Controls.Add(Me.btnSetAuto)
        Me.Controls.Add(Me.btnAddProfile)
        Me.Controls.Add(Me.btnDelProfile)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.cboProfiles)
        Me.Controls.Add(Me.btnSaveAll)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.lvApps)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Windows Arrangement Manager"
        Me.pnlTopology.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lvApps As ListView
    Friend WithEvents Application As ColumnHeader
    Friend WithEvents WindowTitle As ColumnHeader
    Friend WithEvents btnSave As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents btnSaveAll As Button
    Friend WithEvents cboProfiles As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents btnDelProfile As Button
    Friend WithEvents btnAddProfile As Button
    Friend WithEvents btnSetAuto As Button
    Friend WithEvents btnTabWindows As Button
    Friend WithEvents btnTabDisplays As Button
    Friend WithEvents lvDisplays As ListView
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents ColumnHeader3 As ColumnHeader
    Friend WithEvents ColumnHeader4 As ColumnHeader
    Friend WithEvents pnlTopology As Panel
    Friend WithEvents btnIdentify As Button
    Friend WithEvents btnApplyTopology As Button
    Friend WithEvents ColumnHeader5 As ColumnHeader
    Friend WithEvents btnResetLayout As Button
    Friend WithEvents chkStartup As CheckBox
End Class
