namespace PocoSqlGenerator
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnSQLNClasses = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGenClasses = new System.Windows.Forms.Button();
            this.cbxMultipleFiles = new System.Windows.Forms.CheckBox();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.lblConString = new System.Windows.Forms.Label();
            this.btnGenSQL = new System.Windows.Forms.Button();
            this.cboDatabases = new System.Windows.Forms.ComboBox();
            this.sqlServerGroupBox = new System.Windows.Forms.GroupBox();
            this.lblSelectDataBase = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSPPrefix = new System.Windows.Forms.TextBox();
            this.txtgrantUser = new System.Windows.Forms.TextBox();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtQueryFilePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFilesPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.grpOutPut = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGetDBList = new System.Windows.Forms.Button();
            this.cblTableList = new System.Windows.Forms.CheckedListBox();
            this.sqlServerGroupBox.SuspendLayout();
            this.grpOutPut.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSQLNClasses
            // 
            this.btnSQLNClasses.Location = new System.Drawing.Point(410, 304);
            this.btnSQLNClasses.Name = "btnSQLNClasses";
            this.btnSQLNClasses.Size = new System.Drawing.Size(174, 24);
            this.btnSQLNClasses.TabIndex = 18;
            this.btnSQLNClasses.Text = "Generate &Both SQL && Classes";
            this.btnSQLNClasses.UseVisualStyleBackColor = true;
            this.btnSQLNClasses.Click += new System.EventHandler(this.btnSQLNClasses_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(590, 304);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(94, 24);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "&Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnGenClasses
            // 
            this.btnGenClasses.Location = new System.Drawing.Point(289, 304);
            this.btnGenClasses.Name = "btnGenClasses";
            this.btnGenClasses.Size = new System.Drawing.Size(115, 24);
            this.btnGenClasses.TabIndex = 15;
            this.btnGenClasses.Text = "&Generate Classes";
            this.btnGenClasses.UseVisualStyleBackColor = true;
            this.btnGenClasses.Click += new System.EventHandler(this.btnGenClasses_Click);
            // 
            // cbxMultipleFiles
            // 
            this.cbxMultipleFiles.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbxMultipleFiles.Location = new System.Drawing.Point(14, 39);
            this.cbxMultipleFiles.Name = "cbxMultipleFiles";
            this.cbxMultipleFiles.Size = new System.Drawing.Size(318, 24);
            this.cbxMultipleFiles.TabIndex = 8;
            this.cbxMultipleFiles.Text = "Create multiple files for stored procedures/ Queries";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(128, 19);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(420, 20);
            this.txtConnectionString.TabIndex = 3;
            // 
            // lblConString
            // 
            this.lblConString.AutoSize = true;
            this.lblConString.Location = new System.Drawing.Point(11, 22);
            this.lblConString.Name = "lblConString";
            this.lblConString.Size = new System.Drawing.Size(116, 13);
            this.lblConString.TabIndex = 3;
            this.lblConString.Text = "Connection String :";
            // 
            // btnGenSQL
            // 
            this.btnGenSQL.Location = new System.Drawing.Point(168, 304);
            this.btnGenSQL.Name = "btnGenSQL";
            this.btnGenSQL.Size = new System.Drawing.Size(115, 24);
            this.btnGenSQL.TabIndex = 17;
            this.btnGenSQL.Text = "Generate &SQL";
            this.btnGenSQL.UseVisualStyleBackColor = true;
            this.btnGenSQL.Click += new System.EventHandler(this.btnGenSQL_Click);
            // 
            // cboDatabases
            // 
            this.cboDatabases.FormattingEnabled = true;
            this.cboDatabases.Location = new System.Drawing.Point(119, 19);
            this.cboDatabases.Name = "cboDatabases";
            this.cboDatabases.Size = new System.Drawing.Size(283, 21);
            this.cboDatabases.TabIndex = 14;
            this.cboDatabases.SelectedIndexChanged += new System.EventHandler(this.cboDatabases_SelectedIndexChanged);
            // 
            // sqlServerGroupBox
            // 
            this.sqlServerGroupBox.Controls.Add(this.cboDatabases);
            this.sqlServerGroupBox.Controls.Add(this.lblSelectDataBase);
            this.sqlServerGroupBox.Controls.Add(this.label6);
            this.sqlServerGroupBox.Controls.Add(this.label5);
            this.sqlServerGroupBox.Controls.Add(this.txtSPPrefix);
            this.sqlServerGroupBox.Controls.Add(this.txtgrantUser);
            this.sqlServerGroupBox.Location = new System.Drawing.Point(6, 93);
            this.sqlServerGroupBox.Name = "sqlServerGroupBox";
            this.sqlServerGroupBox.Size = new System.Drawing.Size(414, 110);
            this.sqlServerGroupBox.TabIndex = 10;
            this.sqlServerGroupBox.TabStop = false;
            this.sqlServerGroupBox.Text = "SQL Server";
            // 
            // lblSelectDataBase
            // 
            this.lblSelectDataBase.AutoSize = true;
            this.lblSelectDataBase.Location = new System.Drawing.Point(5, 22);
            this.lblSelectDataBase.Name = "lblSelectDataBase";
            this.lblSelectDataBase.Size = new System.Drawing.Size(109, 13);
            this.lblSelectDataBase.TabIndex = 13;
            this.lblSelectDataBase.Text = "Select Database :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "SP Prefix :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Grant User :";
            // 
            // txtSPPrefix
            // 
            this.txtSPPrefix.Location = new System.Drawing.Point(119, 81);
            this.txtSPPrefix.Name = "txtSPPrefix";
            this.txtSPPrefix.Size = new System.Drawing.Size(283, 20);
            this.txtSPPrefix.TabIndex = 7;
            // 
            // txtgrantUser
            // 
            this.txtgrantUser.Location = new System.Drawing.Point(119, 51);
            this.txtgrantUser.Name = "txtgrantUser";
            this.txtgrantUser.Size = new System.Drawing.Size(283, 20);
            this.txtgrantUser.TabIndex = 6;
            // 
            // txtNamespace
            // 
            this.txtNamespace.Location = new System.Drawing.Point(128, 67);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(283, 20);
            this.txtNamespace.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(423, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Select Tables :";
            // 
            // txtQueryFilePath
            // 
            this.txtQueryFilePath.Location = new System.Drawing.Point(119, 43);
            this.txtQueryFilePath.Name = "txtQueryFilePath";
            this.txtQueryFilePath.Size = new System.Drawing.Size(283, 20);
            this.txtQueryFilePath.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "SQL Query File :";
            // 
            // txtFilesPath
            // 
            this.txtFilesPath.Location = new System.Drawing.Point(119, 16);
            this.txtFilesPath.Name = "txtFilesPath";
            this.txtFilesPath.Size = new System.Drawing.Size(283, 20);
            this.txtFilesPath.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Class Files Path :";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 70);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Class Namespace :";
            // 
            // grpOutPut
            // 
            this.grpOutPut.Controls.Add(this.txtQueryFilePath);
            this.grpOutPut.Controls.Add(this.label4);
            this.grpOutPut.Controls.Add(this.txtFilesPath);
            this.grpOutPut.Controls.Add(this.label3);
            this.grpOutPut.Location = new System.Drawing.Point(6, 209);
            this.grpOutPut.Name = "grpOutPut";
            this.grpOutPut.Size = new System.Drawing.Size(414, 70);
            this.grpOutPut.TabIndex = 8;
            this.grpOutPut.TabStop = false;
            this.grpOutPut.Text = "Output Details";
            this.grpOutPut.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGetDBList);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.grpOutPut);
            this.groupBox1.Controls.Add(this.txtNamespace);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cblTableList);
            this.groupBox1.Controls.Add(this.sqlServerGroupBox);
            this.groupBox1.Controls.Add(this.cbxMultipleFiles);
            this.groupBox1.Controls.Add(this.txtConnectionString);
            this.groupBox1.Controls.Add(this.lblConString);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(675, 289);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            // 
            // btnGetDBList
            // 
            this.btnGetDBList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGetDBList.Location = new System.Drawing.Point(554, 16);
            this.btnGetDBList.Name = "btnGetDBList";
            this.btnGetDBList.Size = new System.Drawing.Size(111, 24);
            this.btnGetDBList.TabIndex = 19;
            this.btnGetDBList.Text = "Get &Database List";
            this.btnGetDBList.UseVisualStyleBackColor = true;
            this.btnGetDBList.Click += new System.EventHandler(this.btnGetDBList_Click);
            // 
            // cblTableList
            // 
            this.cblTableList.FormattingEnabled = true;
            this.cblTableList.Location = new System.Drawing.Point(426, 67);
            this.cblTableList.Name = "cblTableList";
            this.cblTableList.Size = new System.Drawing.Size(239, 214);
            this.cblTableList.TabIndex = 8;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 336);
            this.Controls.Add(this.btnSQLNClasses);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGenClasses);
            this.Controls.Add(this.btnGenSQL);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "POCO SQL Generator";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.sqlServerGroupBox.ResumeLayout(false);
            this.sqlServerGroupBox.PerformLayout();
            this.grpOutPut.ResumeLayout(false);
            this.grpOutPut.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSQLNClasses;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenClasses;
        private System.Windows.Forms.CheckBox cbxMultipleFiles;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label lblConString;
        private System.Windows.Forms.Button btnGenSQL;
        private System.Windows.Forms.ComboBox cboDatabases;
        private System.Windows.Forms.GroupBox sqlServerGroupBox;
        private System.Windows.Forms.Label lblSelectDataBase;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSPPrefix;
        private System.Windows.Forms.TextBox txtgrantUser;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtQueryFilePath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFilesPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox grpOutPut;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox cblTableList;
        private System.Windows.Forms.Button btnGetDBList;
    }
}

