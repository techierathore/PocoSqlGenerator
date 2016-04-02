using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace PocoSqlGenerator
{
    public partial class MainForm : Form
    {
        #region Module Variables
        string mSSqlDatabase = "";
        #endregion
        #region Constructors
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void btnGetDBList_Click(object sender, EventArgs e)
        {
            String conxString = txtConnectionString.Text.Trim();
            using (var sqlConx = new SqlConnection(conxString))
            {
                sqlConx.Open();
                var tblDatabases = sqlConx.GetSchema("Databases");
                sqlConx.Close();
                foreach (DataRow row in tblDatabases.Rows)
                {
                    cboDatabases.Items.Add(row["database_name"]);
                }
            }
            cboDatabases.Items.Add("Select Database");
            cboDatabases.SelectedIndex = cboDatabases.Items.Count - 1;
        }

        private void btnGenSQL_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateSQLScripts();
                MessageBox.Show("SQL file(s) created Successfully at path mentioned in 'SQL Query File'", "Success");
                grpOutPut.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenClasses_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateCSharpClasses();
                MessageBox.Show("Class file(s) created Successfully at path mentioned in 'Class Files Path'", "Success");
                grpOutPut.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSQLNClasses_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateSQLScripts();
                GenerateCSharpClasses();
                MessageBox.Show("SQL file(s) & Class file(s) created Successfully at path mentioned in 'SQL Query File' & 'Class Files Path' respectively", "Success");
                grpOutPut.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cboDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboDatabases.Text.Trim() != "Select Database")
                {
                    //if ((cboCustomerName.SelectedValue.ToString().Trim() != "System.Data.DataRowView"))
                    mSSqlDatabase = cboDatabases.Text.Trim();
                    string strConn = txtConnectionString.Text.Trim() + ";Initial Catalog=" + mSSqlDatabase;
                    SqlConnection cbConnection = null;
                    try
                    {
                        DataTable dtSchemaTable = new DataTable("Tables");
                        using (cbConnection = new SqlConnection(strConn))
                        {
                            SqlCommand cmdCommand = cbConnection.CreateCommand();
                            cmdCommand.CommandText = "select table_name as Name from INFORMATION_SCHEMA.Tables where TABLE_TYPE ='BASE TABLE'";
                            cbConnection.Open();
                            dtSchemaTable.Load(cmdCommand.ExecuteReader(CommandBehavior.CloseConnection));
                        }
                        cblTableList.Items.Clear();
                        for (int iCount = 0; iCount < dtSchemaTable.Rows.Count; iCount++)
                        {
                            cblTableList.Items.Add(dtSchemaTable.Rows[iCount][0].ToString());
                        }
                    }
                    finally
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        cbConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
           txtConnectionString.Text = ConfigurationManager.AppSettings["ConnString"];
        }

        #endregion

        #region Private Methods
        private string CreateOutputDir(string aSDirName)
        {
            string sRootDirPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + aSDirName;
            if (!Directory.Exists(sRootDirPath)) Directory.CreateDirectory(sRootDirPath);
            return sRootDirPath;
        }
        private void GenerateSQLScripts()
        {
            string sFolderPath = CreateOutputDir(txtNamespace.Text.Trim() != "" ? txtNamespace.Text.Trim() : mSSqlDatabase);
            var objTableNames = new ArrayList();
            string sConString = txtConnectionString.Text + ";Initial Catalog=" + mSSqlDatabase;
            for (int iTableCount = 0; iTableCount < cblTableList.CheckedItems.Count; iTableCount++)
            {
                objTableNames.Add(cblTableList.CheckedItems[iTableCount].ToString());
            }
            txtQueryFilePath.Text = SqlScriptGenerator.GenerateSQLFiles(sFolderPath, sConString, txtgrantUser.Text.Trim(), txtSPPrefix.Text.Trim(), cbxMultipleFiles.Checked, objTableNames);
        }
        private void GenerateCSharpClasses()
        {
            string sFolderPath, sNameSpace;
            if (txtNamespace.Text.Trim() != "")
            {
                sFolderPath = CreateOutputDir(txtNamespace.Text.Trim());
                sNameSpace = txtNamespace.Text.Trim();
            }
            else
            {
                sFolderPath = CreateOutputDir(mSSqlDatabase);
                sNameSpace = mSSqlDatabase;
            }
                CreateBaseRepoClass(sFolderPath + "\\BaseRepository.cs", sNameSpace);
                var objTableNames = new ArrayList();
                string sConString = "";
                sConString = txtConnectionString.Text + ";Initial Catalog=" + mSSqlDatabase;

                for (int iTableCount = 0; iTableCount < cblTableList.CheckedItems.Count; iTableCount++)
                {
                    objTableNames.Add(cblTableList.CheckedItems[iTableCount].ToString());
                }
                txtFilesPath.Text = CSharpCodeGenerator.GenerateClassFiles(sFolderPath, sConString, txtSPPrefix.Text.Trim(), sNameSpace, "", objTableNames);
            CSharpCodeGenerator.GenerateRepoFiles(sFolderPath, sConString, txtSPPrefix.Text.Trim(), sNameSpace, "", objTableNames);

            }
        private void CreateBaseRepoClass(string aSFilePath, string targetNamespace)
        {
            using (var streamWriter = new StreamWriter(aSFilePath))
            {
                #region Add Referances

                streamWriter.WriteLine("using System;");
                streamWriter.WriteLine("using System.Data;");
                streamWriter.WriteLine("using System.Data.SqlClient;");
                streamWriter.WriteLine("using System.Linq;");
                streamWriter.WriteLine("using System.Web.Configuration;");
                streamWriter.WriteLine();
                streamWriter.WriteLine("namespace " + targetNamespace);
                streamWriter.WriteLine("{");

                #endregion

                #region Create Base Repository Class

                streamWriter.WriteLine("\t public abstract class BaseRepository ");
                streamWriter.WriteLine("\t\t {");
                streamWriter.WriteLine(
                    "\t\t\t protected static void SetIdentity<T>(IDbConnection connection, Action<T> setId) ");
                streamWriter.WriteLine("\t\t\t {");
                streamWriter.WriteLine(
                    "\t\t\t dynamic identity = connection.Query(\"SELECT @@IDENTITY AS Id\").Single(); ");
                streamWriter.WriteLine("\t\t\t T newId = (T)identity.Id; ");
                streamWriter.WriteLine("\t\t\t setId(newId); ");
                streamWriter.WriteLine("\t\t\t }");

                streamWriter.WriteLine(
                    "\t\t\t protected static IDbConnection OpenConnection() ");
                streamWriter.WriteLine("\t\t\t {");
                streamWriter.WriteLine(
                    "\t\t\t IDbConnection connection = new SqlConnection(WebConfigurationManager.ConnectionStrings[\"DBConString\"].ConnectionString); ");
                streamWriter.WriteLine("\t\t\t connection.Open(); ");
                streamWriter.WriteLine("\t\t\t return connection; ");
                streamWriter.WriteLine("\t\t\t }");
                streamWriter.WriteLine("\t\t }");

                #endregion
            }
        }
        #endregion


    }
}
