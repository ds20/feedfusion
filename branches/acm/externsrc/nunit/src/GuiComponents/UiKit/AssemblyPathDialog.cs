#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NUnit.UiKit
{
	/// <summary>
	/// Summary description for AssemblyPathDialog.
	/// </summary>
	public class AssemblyPathDialog : System.Windows.Forms.Form
	{
		private string path;

		private System.Windows.Forms.TextBox assemblyPathTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button browseButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AssemblyPathDialog( string path )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.path = path;
		}

		public string Path
		{
			get { return path; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AssemblyPathDialog));
			this.assemblyPathTextBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.browseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// assemblyPathTextBox
			// 
			this.assemblyPathTextBox.AccessibleDescription = resources.GetString("assemblyPathTextBox.AccessibleDescription");
			this.assemblyPathTextBox.AccessibleName = resources.GetString("assemblyPathTextBox.AccessibleName");
			this.assemblyPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("assemblyPathTextBox.Anchor")));
			this.assemblyPathTextBox.AutoSize = ((bool)(resources.GetObject("assemblyPathTextBox.AutoSize")));
			this.assemblyPathTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("assemblyPathTextBox.BackgroundImage")));
			this.assemblyPathTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("assemblyPathTextBox.Dock")));
			this.assemblyPathTextBox.Enabled = ((bool)(resources.GetObject("assemblyPathTextBox.Enabled")));
			this.assemblyPathTextBox.Font = ((System.Drawing.Font)(resources.GetObject("assemblyPathTextBox.Font")));
			this.assemblyPathTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("assemblyPathTextBox.ImeMode")));
			this.assemblyPathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("assemblyPathTextBox.Location")));
			this.assemblyPathTextBox.MaxLength = ((int)(resources.GetObject("assemblyPathTextBox.MaxLength")));
			this.assemblyPathTextBox.Multiline = ((bool)(resources.GetObject("assemblyPathTextBox.Multiline")));
			this.assemblyPathTextBox.Name = "assemblyPathTextBox";
			this.assemblyPathTextBox.PasswordChar = ((char)(resources.GetObject("assemblyPathTextBox.PasswordChar")));
			this.assemblyPathTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("assemblyPathTextBox.RightToLeft")));
			this.assemblyPathTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("assemblyPathTextBox.ScrollBars")));
			this.assemblyPathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("assemblyPathTextBox.Size")));
			this.assemblyPathTextBox.TabIndex = ((int)(resources.GetObject("assemblyPathTextBox.TabIndex")));
			this.assemblyPathTextBox.Text = resources.GetString("assemblyPathTextBox.Text");
			this.assemblyPathTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("assemblyPathTextBox.TextAlign")));
			this.assemblyPathTextBox.Visible = ((bool)(resources.GetObject("assemblyPathTextBox.Visible")));
			this.assemblyPathTextBox.WordWrap = ((bool)(resources.GetObject("assemblyPathTextBox.WordWrap")));
			// 
			// okButton
			// 
			this.okButton.AccessibleDescription = resources.GetString("okButton.AccessibleDescription");
			this.okButton.AccessibleName = resources.GetString("okButton.AccessibleName");
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("okButton.Anchor")));
			this.okButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("okButton.BackgroundImage")));
			this.okButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("okButton.Dock")));
			this.okButton.Enabled = ((bool)(resources.GetObject("okButton.Enabled")));
			this.okButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("okButton.FlatStyle")));
			this.okButton.Font = ((System.Drawing.Font)(resources.GetObject("okButton.Font")));
			this.okButton.Image = ((System.Drawing.Image)(resources.GetObject("okButton.Image")));
			this.okButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("okButton.ImageAlign")));
			this.okButton.ImageIndex = ((int)(resources.GetObject("okButton.ImageIndex")));
			this.okButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("okButton.ImeMode")));
			this.okButton.Location = ((System.Drawing.Point)(resources.GetObject("okButton.Location")));
			this.okButton.Name = "okButton";
			this.okButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("okButton.RightToLeft")));
			this.okButton.Size = ((System.Drawing.Size)(resources.GetObject("okButton.Size")));
			this.okButton.TabIndex = ((int)(resources.GetObject("okButton.TabIndex")));
			this.okButton.Text = resources.GetString("okButton.Text");
			this.okButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("okButton.TextAlign")));
			this.okButton.Visible = ((bool)(resources.GetObject("okButton.Visible")));
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.AccessibleDescription = resources.GetString("cancelButton.AccessibleDescription");
			this.cancelButton.AccessibleName = resources.GetString("cancelButton.AccessibleName");
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cancelButton.Anchor")));
			this.cancelButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancelButton.BackgroundImage")));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cancelButton.Dock")));
			this.cancelButton.Enabled = ((bool)(resources.GetObject("cancelButton.Enabled")));
			this.cancelButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cancelButton.FlatStyle")));
			this.cancelButton.Font = ((System.Drawing.Font)(resources.GetObject("cancelButton.Font")));
			this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
			this.cancelButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancelButton.ImageAlign")));
			this.cancelButton.ImageIndex = ((int)(resources.GetObject("cancelButton.ImageIndex")));
			this.cancelButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cancelButton.ImeMode")));
			this.cancelButton.Location = ((System.Drawing.Point)(resources.GetObject("cancelButton.Location")));
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cancelButton.RightToLeft")));
			this.cancelButton.Size = ((System.Drawing.Size)(resources.GetObject("cancelButton.Size")));
			this.cancelButton.TabIndex = ((int)(resources.GetObject("cancelButton.TabIndex")));
			this.cancelButton.Text = resources.GetString("cancelButton.Text");
			this.cancelButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancelButton.TextAlign")));
			this.cancelButton.Visible = ((bool)(resources.GetObject("cancelButton.Visible")));
			// 
			// browseButton
			// 
			this.browseButton.AccessibleDescription = resources.GetString("browseButton.AccessibleDescription");
			this.browseButton.AccessibleName = resources.GetString("browseButton.AccessibleName");
			this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("browseButton.Anchor")));
			this.browseButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("browseButton.BackgroundImage")));
			this.browseButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("browseButton.Dock")));
			this.browseButton.Enabled = ((bool)(resources.GetObject("browseButton.Enabled")));
			this.browseButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("browseButton.FlatStyle")));
			this.browseButton.Font = ((System.Drawing.Font)(resources.GetObject("browseButton.Font")));
			this.browseButton.Image = ((System.Drawing.Image)(resources.GetObject("browseButton.Image")));
			this.browseButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("browseButton.ImageAlign")));
			this.browseButton.ImageIndex = ((int)(resources.GetObject("browseButton.ImageIndex")));
			this.browseButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("browseButton.ImeMode")));
			this.browseButton.Location = ((System.Drawing.Point)(resources.GetObject("browseButton.Location")));
			this.browseButton.Name = "browseButton";
			this.browseButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("browseButton.RightToLeft")));
			this.browseButton.Size = ((System.Drawing.Size)(resources.GetObject("browseButton.Size")));
			this.browseButton.TabIndex = ((int)(resources.GetObject("browseButton.TabIndex")));
			this.browseButton.Text = resources.GetString("browseButton.Text");
			this.browseButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("browseButton.TextAlign")));
			this.browseButton.Visible = ((bool)(resources.GetObject("browseButton.Visible")));
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// AssemblyPathDialog
			// 
			this.AcceptButton = this.okButton;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.cancelButton;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.assemblyPathTextBox);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AssemblyPathDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.AssemblyPathDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void okButton_Click(object sender, System.EventArgs e)
		{
			string path = assemblyPathTextBox.Text;

			try
			{
				FileInfo info = new FileInfo( path );

				if ( !info.Exists )
				{
					DialogResult answer = UserMessage.Ask( string.Format( 
						"The path {0} does not exist. Do you want to use it anyway?", path ) );
					if ( answer != DialogResult.Yes )
						return;
				}

				DialogResult = DialogResult.OK;
				this.path = path;
				this.Close();
			}
			catch( System.Exception exception )
			{
				assemblyPathTextBox.SelectAll();
				UserMessage.DisplayFailure( exception, "Invalid Entry" );
			}	
		}

		private void AssemblyPathDialog_Load(object sender, System.EventArgs e)
		{
			assemblyPathTextBox.Text = path;
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Select Assembly";
			
			dlg.Filter =
				"Assemblies (*.dll,*.exe)|*.dll;*.exe|" +
				"All Files (*.*)|*.*";

			dlg.InitialDirectory = System.IO.Path.GetDirectoryName( path );
			dlg.FilterIndex = 1;
			dlg.FileName = "";

			if ( dlg.ShowDialog( this ) == DialogResult.OK ) 
			{
				assemblyPathTextBox.Text = dlg.FileName;
			}
		}
	}
}