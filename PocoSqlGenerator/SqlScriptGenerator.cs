using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PocoSqlGenerator
{
    /// <summary>
    /// Generates DML SQL Scripts for basic CRUD operations of selected database tables.
    /// </summary>
    internal static class SqlScriptGenerator
    {
        /// <summary>
        /// Generate CRUD SP SQL scripts for the DB selected Tables
        /// </summary>
        /// <param name="outputDirectory">The directory where the SQL code should be created.</param>
        /// <param name="connectionString">The connection string to be used to connect the to the database.</param>
        /// <param name="grantLoginName">The SQL Server login name that should be granted execute rights on the generated stored procedures.</param>
        /// <param name="storedProcedurePrefix">The prefix that should be used when creating stored procedures.</param>
        /// <param name="createMultipleFiles">A flag indicating if the generated stored procedures should be created in one file or separate files.</param>
        /// <param name="tableNames">ArrayList of Table names whose SPs has to be created.</param>
        /// <returns></returns>
        public static string GenerateSQLFiles(string outputDirectory, string connectionString, string grantLoginName, string storedProcedurePrefix, bool createMultipleFiles, ArrayList tableNames)
        {

            string databaseName = "";
            string sqlPath;
            sqlPath = Path.Combine(outputDirectory, "SQL");
            List<Table> tableList = AppUtility.GetTableList(connectionString, outputDirectory, tableNames, ref databaseName);
            // Generate the necessary SQL for each table
            int count = 0;
            if (tableList.Count > 0)
            {
                // Create the necessary directories
                AppUtility.CreateSubDirectory(sqlPath, true);
                // Create the necessary database logins
                CreateUserQueries(databaseName, grantLoginName, sqlPath, createMultipleFiles);

                // Create the CRUD stored procedures and data access code for each table
                foreach (Table table in tableList)
                {
                    CreateInsertStoredProcedure(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    CreateUpdateStoredProcedure(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    CreateDeleteStoredProcedure(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    CreateDeleteAllByStoredProcedures(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    CreateSelectStoredProcedure(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    CreateSelectAllStoredProcedure(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    CreateSelectAllByStoredProcedures(table, grantLoginName, storedProcedurePrefix, sqlPath, createMultipleFiles);
                    count++;
                }
            }

            return sqlPath;
        }

        /// <summary>
        /// Creates the SQL script that is responsible for granting the specified login access to the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database that the login will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="path">Path where the script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the script should be created in its own file.</param>
        internal static void CreateUserQueries(string databaseName, string grantLoginName, string path, bool createMultipleFiles)
        {
            if (grantLoginName.Length > 0)
            {
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, "GrantUserPermissions.sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    writer.Write(AppUtility.GetUserQueries(databaseName, grantLoginName));
                }
            }
        }

        /// <summary>
        /// Creates an insert stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateInsertStoredProcedure(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            // Create the stored procedure name
            string procedureName = storedProcedurePrefix + table.Name + "Insert";
            string fileName;

            // Determine the file name to be used
            if (createMultipleFiles)
            {
                fileName = Path.Combine(path, procedureName + ".sql");
            }
            else
            {
                fileName = Path.Combine(path, "StoredProcedures.sql");
            }

            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                // Create the seperator
                if (createMultipleFiles == false)
                {
                    writer.WriteLine();
                    writer.WriteLine("/******************************************************************************");
                    writer.WriteLine("******************************************************************************/");
                }

                // Create the drop statment
                writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                writer.WriteLine("GO");
                writer.WriteLine();

                // Create the SQL for the stored procedure
                writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                writer.WriteLine("(");

                // Create the parameter list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];
                    if (column.IsIdentity == false && column.IsRowGuidCol == false)
                    {
                        writer.Write("\t" + AppUtility.CreateParameterString(column, true));
                        if (i < (table.Columns.Count - 1))
                        {
                            writer.Write(",");
                        }
                        writer.WriteLine();
                    }
                }
                writer.WriteLine(")");

                writer.WriteLine();
                writer.WriteLine("AS");
                writer.WriteLine();
                writer.WriteLine("SET NOCOUNT ON");
                writer.WriteLine();

                // Initialize all RowGuidCol columns
                foreach (Column column in table.Columns)
                {
                    if (column.IsRowGuidCol)
                    {
                        writer.WriteLine("SET @" + column.Name + " = NEWID()");
                        writer.WriteLine();
                        break;
                    }
                }

                writer.WriteLine("INSERT INTO [" + table.Name + "]");
                writer.WriteLine("(");

                // Create the parameter list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];

                    // Ignore any identity columns
                    if (column.IsIdentity == false)
                    {
                        // Append the column name as a parameter of the insert statement
                        if (i < (table.Columns.Count - 1))
                        {
                            writer.WriteLine("\t[" + column.Name + "],");
                        }
                        else
                        {
                            writer.WriteLine("\t[" + column.Name + "]");
                        }
                    }
                }

                writer.WriteLine(")");
                writer.WriteLine("VALUES");
                writer.WriteLine("(");

                // Create the values list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];

                    // Is the current column an identity column?
                    if (column.IsIdentity == false)
                    {
                        // Append the necessary line breaks and commas
                        if (i < (table.Columns.Count - 1))
                        {
                            writer.WriteLine("\t@" + column.Name + ",");
                        }
                        else
                        {
                            writer.WriteLine("\t@" + column.Name);
                        }
                    }
                }

                writer.WriteLine(")");

                // Should we include a line for returning the identity?
                foreach (Column column in table.Columns)
                {
                    // Is the current column an identity column?
                    if (column.IsIdentity)
                    {
                        writer.WriteLine();
                        writer.WriteLine("SELECT SCOPE_IDENTITY()");
                        break;
                    }
                    if (column.IsRowGuidCol)
                    {
                        writer.WriteLine();
                        writer.WriteLine("SELECT @" + column.Name);
                        break;
                    }
                }

                writer.WriteLine("GO");

                // Create the grant statement, if a user was specified
                if (grantLoginName.Length > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                    writer.WriteLine("GO");
                }
            }
        }

        /// <summary>
        /// Creates an update stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateUpdateStoredProcedure(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            if (table.PrimaryKeys.Count > 0 && table.Columns.Count != table.PrimaryKeys.Count && table.Columns.Count != table.ForeignKeys.Count)
            {
                // Create the stored procedure name
                string procedureName = storedProcedurePrefix + table.Name + "Update";
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Create the seperator
                    if (createMultipleFiles == false)
                    {
                        writer.WriteLine();
                        writer.WriteLine("/******************************************************************************");
                        writer.WriteLine("******************************************************************************/");
                    }

                    // Create the drop statment
                    writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                    writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Create the SQL for the stored procedure
                    writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                    writer.WriteLine("(");

                    // Create the parameter list
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        Column column = table.Columns[i];

                        if (i == 0)
                        {

                        }
                        if (i < (table.Columns.Count - 1))
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false) + ",");
                        }
                        else
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false));
                        }
                    }
                    writer.WriteLine(")");

                    writer.WriteLine();
                    writer.WriteLine("AS");
                    writer.WriteLine();
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine();
                    writer.WriteLine("UPDATE [" + table.Name + "]");
                    writer.Write("SET");

                    // Create the set statement
                    bool firstLine = true;
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        var column = table.Columns[i];

                        // Ignore Identity and RowGuidCol columns
                        if (table.PrimaryKeys.Contains(column) == false)
                        {
                            if (firstLine)
                            {
                                writer.Write(" ");
                                firstLine = false;
                            }
                            else
                            {
                                writer.Write("\t");
                            }

                            writer.Write("[" + column.Name + "] = @" + column.Name);

                            if (i < (table.Columns.Count - 1))
                            {
                                writer.Write(",");
                            }

                            writer.WriteLine();
                        }
                    }

                    writer.Write("WHERE");

                    // Create the where clause
                    for (int i = 0; i < table.PrimaryKeys.Count; i++)
                    {
                        Column column = table.PrimaryKeys[i];

                        if (i == 0)
                        {
                            writer.Write(" [" + column.Name + "] = @" + column.Name);
                        }
                        else
                        {
                            writer.Write("\tAND [" + column.Name + "] = @" + column.Name);
                        }
                    }
                    writer.WriteLine();

                    writer.WriteLine("GO");

                    // Create the grant statement, if a user was specified
                    if (grantLoginName.Length > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                        writer.WriteLine("GO");
                    }
                }
            }
        }

        /// <summary>
        /// Creates an delete stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateDeleteStoredProcedure(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            if (table.PrimaryKeys.Count > 0)
            {
                // Create the stored procedure name
                string procedureName = storedProcedurePrefix + table.Name + "Delete";
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Create the seperator
                    if (createMultipleFiles == false)
                    {
                        writer.WriteLine();
                        writer.WriteLine("/******************************************************************************");
                        writer.WriteLine("******************************************************************************/");
                    }

                    // Create the drop statment
                    writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                    writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Create the SQL for the stored procedure
                    writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                    writer.WriteLine("(");

                    // Create the parameter list
                    for (int i = 0; i < table.PrimaryKeys.Count; i++)
                    {
                        Column column = table.PrimaryKeys[i];

                        if (i < (table.PrimaryKeys.Count - 1))
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false) + ",");
                        }
                        else
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false));
                        }
                    }
                    writer.WriteLine(")");

                    writer.WriteLine();
                    writer.WriteLine("AS");
                    writer.WriteLine();
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine();
                    writer.WriteLine("DELETE FROM [" + table.Name + "]");
                    writer.Write("WHERE");

                    // Create the where clause
                    for (int i = 0; i < table.PrimaryKeys.Count; i++)
                    {
                        Column column = table.PrimaryKeys[i];

                        if (i == 0)
                        {
                            writer.WriteLine(" [" + column.Name + "] = @" + column.Name);
                        }
                        else
                        {
                            writer.WriteLine("\tAND [" + column.Name + "] = @" + column.Name);
                        }
                    }

                    writer.WriteLine("GO");

                    // Create the grant statement, if a user was specified
                    if (grantLoginName.Length > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                        writer.WriteLine("GO");
                    }
                }
            }
        }

        /// <summary>
        /// Creates one or more delete stored procedures SQL script for the specified table and its foreign keys
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateDeleteAllByStoredProcedures(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            // Create a stored procedure for each foreign key
            foreach (List<Column> compositeKeyList in table.ForeignKeys.Values)
            {
                // Create the stored procedure name
                StringBuilder stringBuilder = new StringBuilder(255);
                stringBuilder.Append(storedProcedurePrefix + table.Name + "DeleteAllBy");

                // Create the parameter list
                for (int i = 0; i < compositeKeyList.Count; i++)
                {
                    Column column = compositeKeyList[i];
                    if (i > 0)
                    {
                        stringBuilder.Append("_" + AppUtility.FormatPascal(column.Name));
                    }
                    else
                    {
                        stringBuilder.Append(AppUtility.FormatPascal(column.Name));
                    }
                }

                string procedureName = stringBuilder.ToString();
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Create the seperator
                    if (createMultipleFiles == false)
                    {
                        writer.WriteLine();
                        writer.WriteLine("/******************************************************************************");
                        writer.WriteLine("******************************************************************************/");
                    }

                    // Create the drop statment
                    writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                    writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Create the SQL for the stored procedure
                    writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                    writer.WriteLine("(");

                    // Create the parameter list
                    for (int i = 0; i < compositeKeyList.Count; i++)
                    {
                        Column column = compositeKeyList[i];

                        if (i < (compositeKeyList.Count - 1))
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false) + ",");
                        }
                        else
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false));
                        }
                    }
                    writer.WriteLine(")");

                    writer.WriteLine();
                    writer.WriteLine("AS");
                    writer.WriteLine();
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine();
                    writer.WriteLine("DELETE FROM [" + table.Name + "]");
                    writer.Write("WHERE");

                    // Create the where clause
                    for (int i = 0; i < compositeKeyList.Count; i++)
                    {
                        Column column = compositeKeyList[i];

                        if (i == 0)
                        {
                            writer.WriteLine(" [" + column.Name + "] = @" + column.Name);
                        }
                        else
                        {
                            writer.WriteLine("\tAND [" + column.Name + "] = @" + column.Name);
                        }
                    }

                    writer.WriteLine("GO");

                    // Create the grant statement, if a user was specified
                    if (grantLoginName.Length > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                        writer.WriteLine("GO");
                    }
                }
            }
        }

        /// <summary>
        /// Creates an select stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateSelectStoredProcedure(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            if (table.PrimaryKeys.Count > 0 && table.ForeignKeys.Count != table.Columns.Count)
            {
                // Create the stored procedure name
                string procedureName = storedProcedurePrefix + table.Name + "Select";
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Create the seperator
                    if (createMultipleFiles == false)
                    {
                        writer.WriteLine();
                        writer.WriteLine("/******************************************************************************");
                        writer.WriteLine("******************************************************************************/");
                    }

                    // Create the drop statment
                    writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                    writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Create the SQL for the stored procedure
                    writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                    writer.WriteLine("(");

                    // Create the parameter list
                    for (int i = 0; i < table.PrimaryKeys.Count; i++)
                    {
                        Column column = table.PrimaryKeys[i];

                        if (i == (table.PrimaryKeys.Count - 1))
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false));
                        }
                        else
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false) + ",");
                        }
                    }

                    writer.WriteLine(")");

                    writer.WriteLine();
                    writer.WriteLine("AS");
                    writer.WriteLine();
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine();
                    writer.Write("SELECT");

                    // Create the list of columns
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        Column column = table.Columns[i];

                        if (i == 0)
                        {
                            writer.Write(" ");
                        }
                        else
                        {
                            writer.Write("\t");
                        }

                        writer.Write("[" + column.Name + "]");

                        if (i < (table.Columns.Count - 1))
                        {
                            writer.Write(",");
                        }

                        writer.WriteLine();
                    }

                    writer.WriteLine("FROM [" + table.Name + "]");
                    writer.Write("WHERE");

                    // Create the where clause
                    for (int i = 0; i < table.PrimaryKeys.Count; i++)
                    {
                        Column column = table.PrimaryKeys[i];

                        if (i == 0)
                        {
                            writer.WriteLine(" [" + column.Name + "] = @" + column.Name);
                        }
                        else
                        {
                            writer.WriteLine("\tAND [" + column.Name + "] = @" + column.Name);
                        }
                    }

                    writer.WriteLine("GO");

                    // Create the grant statement, if a user was specified
                    if (grantLoginName.Length > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                        writer.WriteLine("GO");
                    }
                }
            }
        }

        /// <summary>
        /// Creates an select all stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateSelectAllStoredProcedure(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            if (table.PrimaryKeys.Count > 0 && table.ForeignKeys.Count != table.Columns.Count)
            {
                // Create the stored procedure name
                string procedureName = storedProcedurePrefix + table.Name + "SelectAll";
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Create the seperator
                    if (createMultipleFiles == false)
                    {
                        writer.WriteLine();
                        writer.WriteLine("/******************************************************************************");
                        writer.WriteLine("******************************************************************************/");
                    }

                    // Create the drop statment
                    writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                    writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Create the SQL for the stored procedure
                    writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                    writer.WriteLine();
                    writer.WriteLine("AS");
                    writer.WriteLine();
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine();
                    writer.Write("SELECT");

                    // Create the list of columns
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        Column column = table.Columns[i];

                        if (i == 0)
                        {
                            writer.Write(" ");
                        }
                        else
                        {
                            writer.Write("\t");
                        }

                        writer.Write("[" + column.Name + "]");

                        if (i < (table.Columns.Count - 1))
                        {
                            writer.Write(",");
                        }

                        writer.WriteLine();
                    }

                    writer.WriteLine("FROM [" + table.Name + "]");

                    writer.WriteLine("GO");

                    // Create the grant statement, if a user was specified
                    if (grantLoginName.Length > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                        writer.WriteLine("GO");
                    }
                }
            }
        }

        /// <summary>
        /// Creates one or more select stored procedures SQL script for the specified table and its foreign keys
        /// </summary>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        internal static void CreateSelectAllByStoredProcedures(Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            // Create a stored procedure for each foreign key
            foreach (List<Column> compositeKeyList in table.ForeignKeys.Values)
            {
                // Create the stored procedure name
                StringBuilder stringBuilder = new StringBuilder(255);
                stringBuilder.Append(storedProcedurePrefix + table.Name + "SelectAllBy");

                // Create the parameter list
                for (int i = 0; i < compositeKeyList.Count; i++)
                {
                    Column column = compositeKeyList[i];
                    if (i > 0)
                    {
                        stringBuilder.Append("_" + AppUtility.FormatPascal(column.Name));
                    }
                    else
                    {
                        stringBuilder.Append(AppUtility.FormatPascal(column.Name));
                    }
                }

                string procedureName = stringBuilder.ToString();
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Create the seperator
                    if (createMultipleFiles == false)
                    {
                        writer.WriteLine();
                        writer.WriteLine("/******************************************************************************");
                        writer.WriteLine("******************************************************************************/");
                    }

                    // Create the drop statment
                    writer.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)");
                    writer.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Create the SQL for the stored procedure
                    writer.WriteLine("CREATE PROCEDURE [dbo].[" + procedureName + "]");
                    writer.WriteLine("(");

                    // Create the parameter list
                    for (int i = 0; i < compositeKeyList.Count; i++)
                    {
                        Column column = compositeKeyList[i];

                        if (i < (compositeKeyList.Count - 1))
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false) + ",");
                        }
                        else
                        {
                            writer.WriteLine("\t" + AppUtility.CreateParameterString(column, false));
                        }
                    }
                    writer.WriteLine(")");

                    writer.WriteLine();
                    writer.WriteLine("AS");
                    writer.WriteLine();
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine();
                    writer.Write("SELECT");

                    // Create the list of columns
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        Column column = table.Columns[i];

                        if (i == 0)
                        {
                            writer.Write(" ");
                        }
                        else
                        {
                            writer.Write("\t");
                        }

                        writer.Write("[" + column.Name + "]");

                        if (i < (table.Columns.Count - 1))
                        {
                            writer.Write(",");
                        }

                        writer.WriteLine();
                    }

                    writer.WriteLine("FROM [" + table.Name + "]");
                    writer.Write("WHERE");

                    // Create the where clause
                    for (int i = 0; i < compositeKeyList.Count; i++)
                    {
                        Column column = compositeKeyList[i];

                        if (i == 0)
                        {
                            writer.WriteLine(" [" + column.Name + "] = @" + column.Name);
                        }
                        else
                        {
                            writer.WriteLine("\tAND [" + column.Name + "] = @" + column.Name);
                        }
                    }

                    writer.WriteLine("GO");

                    // Create the grant statement, if a user was specified
                    if (grantLoginName.Length > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("GRANT EXECUTE ON [dbo].[" + procedureName + "] TO [" + grantLoginName + "]");
                        writer.WriteLine("GO");
                    }
                }
            }
        }
     }

    /// <summary>
    /// Class that stores information for tables in a database.
    /// </summary>
    public class Table
    {
        string name;
        List<Column> columns;
        List<Column> primaryKeys;
        Dictionary<string, List<Column>> foreignKeys;

        /// <summary>
        /// Default constructor.  All collections are initialized.
        /// </summary>
        public Table()
        {
            columns = new List<Column>();
            primaryKeys = new List<Column>();
            foreignKeys = new Dictionary<string, List<Column>>();
        }

        /// <summary>
        /// Contains the list of Column instances that define the table.
        /// </summary>
        public List<Column> Columns
        {
            get { return columns; }
        }

        /// <summary>
        /// Contains the list of Column instances that define the table.  The Dictionary returned 
        /// is keyed on the foreign key name, and the value associated with the key is an 
        /// List of Column instances that compose the foreign key.
        /// </summary>
        public Dictionary<string, List<Column>> ForeignKeys
        {
            get { return foreignKeys; }
        }

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Contains the list of primary key Column instances that define the table.
        /// </summary>
        public List<Column> PrimaryKeys
        {
            get { return primaryKeys; }
        }
    }

    /// <summary>
    /// Class that stores information for columns in a database table.
    /// </summary>
    public class Column
    {
        // Private variable used to hold the property values
        private string name;
        private string type;
        private string length;
        private string precision;
        private string scale;
        private bool isRowGuidCol;
        private bool isIdentity;
        private bool isComputed;

        /// <summary>
        /// Name of the column.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Data type of the column.
        /// </summary>
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Length in bytes of the column.
        /// </summary>
        public string Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// Precision of the column.  Applicable to decimal, float, and numeric data types only.
        /// </summary>
        public string Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        /// <summary>
        /// Scale of the column.  Applicable to decimal, and numeric data types only.
        /// </summary>
        public string Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Flags the column as a uniqueidentifier column.
        /// </summary>
        public bool IsRowGuidCol
        {
            get { return isRowGuidCol; }
            set { isRowGuidCol = value; }
        }

        /// <summary>
        /// Flags the column as an identity column.
        /// </summary>
        public bool IsIdentity
        {
            get { return isIdentity; }
            set { isIdentity = value; }
        }

        /// <summary>
        /// Flags the column as being computed.
        /// </summary>
        public bool IsComputed
        {
            get { return isComputed; }
            set { isComputed = value; }
        }
    }
}
