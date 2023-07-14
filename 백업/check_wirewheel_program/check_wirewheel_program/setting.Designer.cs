namespace check_wirewheel_program
{
    partial class setting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PG_System = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // PG_System
            // 
            this.PG_System.Location = new System.Drawing.Point(1, -27);
            this.PG_System.Name = "PG_System";
            this.PG_System.Size = new System.Drawing.Size(363, 762);
            this.PG_System.TabIndex = 0;
            this.PG_System.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PG_System_PropertyValueChanged);
            // 
            // setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 731);
            this.Controls.Add(this.PG_System);
            this.Name = "setting";
            this.Text = "setting";
            this.Load += new System.EventHandler(this.setting_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid PG_System;
    }
}