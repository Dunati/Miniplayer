using Microsoft.Web.WebView2.WinForms;
namespace MiniPlayer
{
    partial class MiniPlayer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MiniPlayer));
            SuspendLayout();
            // 
            // MiniPlayer
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(185, 125);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            //Margin = new Padding(3,3,3,3);
            Name = "MiniPlayer";
            Padding = new Padding(3,3,3,3);
            Text = "MiniPlayer";
            MouseDoubleClick += Form1_MouseDoubleClick;
            ResumeLayout(false);
        }

        #endregion

    }
}
