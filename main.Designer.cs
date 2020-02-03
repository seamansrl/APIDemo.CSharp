namespace APIDemo
{
    partial class main
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.Preview = new System.Windows.Forms.PictureBox();
            this.FromServer = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Preview)).BeginInit();
            this.SuspendLayout();
            // 
            // Preview
            // 
            this.Preview.Location = new System.Drawing.Point(12, 12);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(640, 480);
            this.Preview.TabIndex = 0;
            this.Preview.TabStop = false;
            // 
            // FromServer
            // 
            this.FromServer.Location = new System.Drawing.Point(12, 498);
            this.FromServer.Multiline = true;
            this.FromServer.Name = "FromServer";
            this.FromServer.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.FromServer.Size = new System.Drawing.Size(640, 165);
            this.FromServer.TabIndex = 1;
            // 
            // main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 675);
            this.Controls.Add(this.FromServer);
            this.Controls.Add(this.Preview);
            this.Name = "main";
            this.Text = "Demo";
            this.Load += new System.EventHandler(this.main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Preview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Preview;
        private System.Windows.Forms.TextBox FromServer;
    }
}

