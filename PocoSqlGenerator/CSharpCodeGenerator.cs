using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PocoSqlGenerator
{
    internal static class CSharpCodeGenerator
    {
        public static string GenerateClassFiles(string outputDirectory, string connectionString, string storedProcedurePrefix, string targetNamespace, string daoSuffix, ArrayList tableNames)
        {
            string databaseName = "";
            string csPath;
            csPath = Path.Combine(outputDirectory, "CS");
            List<Table> tableList = AppUtility.GetTableList(connectionString, outputDirectory, tableNames, ref databaseName);
            // Generate the necessary SQL and C# code for each table            
            if (tableList.Count <= 0) return csPath;
            // Create the necessary directories                
            AppUtility.CreateSubDirectory(csPath, true);
            foreach (Table table in tableList)
            {
                CreateModelClass(databaseName, table, targetNamespace, storedProcedurePrefix, csPath);
            }
            CreateAssemblyInfo(csPath, targetNamespace, databaseName);
            CreateProjectFile(csPath, targetNamespace, tableList, daoSuffix);
            return csPath;
        }

        public static string GenerateRepoFiles(string outputDirectory, string connectionString, string storedProcedurePrefix, string targetNamespace, string daoSuffix, ArrayList tableNames)
        {
            string databaseName = "";
            string csPath = Path.Combine(outputDirectory, "Repo");
            List<Table> tableList = AppUtility.GetTableList(connectionString, outputDirectory, tableNames, ref databaseName);
            // Generate the necessary SQL and C# code for each table            
            if (tableList.Count <= 0) return csPath;
            // Create the necessary directories                
            AppUtility.CreateSubDirectory(csPath, true);
            // Create the CRUD stored procedures and data access code for each table
            foreach (Table table in tableList)
            {
                CreateRepoClass(databaseName, table, targetNamespace, storedProcedurePrefix, csPath);
            }
            CreateAssemblyInfo(csPath, targetNamespace, databaseName);
            CreateProjectFile(csPath, targetNamespace, tableList, daoSuffix);
            return csPath;
        }
        /// <summary>
        /// Creates a project file that references each generated C# code file for data access.
        /// </summary>
        /// <param name="path">The path where the project file should be created.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="tableList">The list of tables code files were created for.</param>
        /// <param name="daoSuffix">The suffix to append to the name of each data access class.</param>
        internal static void CreateProjectFile(string path, string projectName, List<Table> tableList, string daoSuffix)
        {
            string projectXml = AppUtility.GetResource("PocoSqlGenerator.Resources.Project.xml");
            var document = new XmlDocument();
            document.LoadXml(projectXml);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace(String.Empty, "http://schemas.microsoft.com/developer/msbuild/2003");
            namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

            var selectSingleNode = document.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:ProjectGuid", namespaceManager);
            if (selectSingleNode !=
                null)
                selectSingleNode.InnerText = "{" + Guid.NewGuid().ToString() + "}";
            var singleNode = document.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:RootNamespace", namespaceManager);
            if (singleNode !=
                null)
                singleNode.InnerText = projectName;
            var xmlNode = document.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:AssemblyName", namespaceManager);
            if (xmlNode !=
                null)
                xmlNode.InnerText = projectName;

            XmlNode itemGroupNode = document.SelectSingleNode("/msbuild:Project/msbuild:ItemGroup[msbuild:Compile]", namespaceManager);
            foreach (Table table in tableList)
            {
                string className = AppUtility.FormatClassName(table.Name);

                XmlNode dtoCompileNode = document.CreateElement("Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
                XmlAttribute dtoAttribute = document.CreateAttribute("Include");
                dtoAttribute.Value = className + ".cs";
                if (dtoCompileNode.Attributes != null) dtoCompileNode.Attributes.Append(dtoAttribute);
                if (itemGroupNode != null) itemGroupNode.AppendChild(dtoCompileNode);
            }

            document.Save(Path.Combine(path, projectName + ".csproj"));
        }

        /// <summary>
        /// Creates the AssemblyInfo.cs file for the project.
        /// </summary>
        /// <param name="path">The root path of the project.</param>
        /// <param name="assemblyTitle">The title of the assembly.</param>
        /// <param name="databaseName">The name of the database the assembly provides access to.</param>
        internal static void CreateAssemblyInfo(string path, string assemblyTitle, string databaseName)
        {
            string assemblyInfo = AppUtility.GetResource("PocoSqlGenerator.Resources.AssemblyInfo.txt");
            assemblyInfo.Replace("#AssemblyTitle", assemblyTitle);
            assemblyInfo.Replace("#DatabaseName", databaseName);

            string propertiesDirectory = Path.Combine(path, "Properties");
            if (Directory.Exists(propertiesDirectory) == false)
            {
                Directory.CreateDirectory(propertiesDirectory);
            }

            File.WriteAllText(Path.Combine(propertiesDirectory, "AssemblyInfo.cs"), assemblyInfo);
        }

        /// <summary>
        /// Creates a C# Model /POCO class for all of the table's of Database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="table">Instance of the Table class that represents the table this class will be created for.</param>
        /// <param name="targetNamespace">The namespace that the generated C# classes should contained in.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the class should be created.</param>
        internal static void CreateModelClass(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string path)
        {
            var className = AppUtility.FormatClassName(table.Name);
            using (var streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
            {
                #region Create the header for the class
                streamWriter.WriteLine("using System;");
                streamWriter.WriteLine();
                streamWriter.WriteLine("namespace " + targetNamespace);
                streamWriter.WriteLine("{");

                streamWriter.WriteLine("\tpublic class " + className);
                streamWriter.WriteLine("\t{");
                #endregion

                #region  Append the public properties
                streamWriter.WriteLine("\t\t#region Properties");
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];
                    var parameter = AppUtility.CreateMethodParameter(column);
                    var type = parameter.Split(' ')[0];
                    var name = parameter.Split(' ')[1];
                    streamWriter.WriteLine("\t\t/// <summary>");
                    streamWriter.WriteLine("\t\t/// Gets or sets the " + AppUtility.FormatPascal(name) + " value.");
                    streamWriter.WriteLine("\t\t/// </summary>");
                    streamWriter.WriteLine("\t\tpublic " + type + " " + AppUtility.FormatPascal(name));
                    streamWriter.WriteLine("\t\t{ get; set; }");
                    if (i < (table.Columns.Count - 1))
                    {
                        streamWriter.WriteLine();
                    }
                }

                streamWriter.WriteLine();
                streamWriter.WriteLine("\t\t#endregion");
                #endregion
                // Close out the class and namespace
                streamWriter.WriteLine("\t}");
                streamWriter.WriteLine("}");
            }
        }

        internal static void CreateRepoClass(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string path)
        {
            var className = AppUtility.FormatClassName(table.Name);
            using (var streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
            {
                #region Add References & Declare Class
                streamWriter.WriteLine("using System.Collections.Generic;");
                streamWriter.WriteLine("using System.Data;");
                streamWriter.WriteLine("using System.Linq;");
                streamWriter.WriteLine("using Dapper;");
                streamWriter.WriteLine();
                streamWriter.WriteLine("namespace " + targetNamespace);
                streamWriter.WriteLine("{");
                streamWriter.WriteLine("\t public class " + className + "Repo : BaseRepository");
                streamWriter.WriteLine("\t\t {");
                #endregion

                #region Append the access methods
                streamWriter.WriteLine("\t\t#region Methods");
                streamWriter.WriteLine();
                CreateInsertMethod(table, streamWriter);
                CreateUpdateMethod(table, streamWriter);
                CreateSelectMethod(table, streamWriter);
                CreateSelectAllMethod(table, streamWriter);
                CreateSelectAllByMethods(table, storedProcedurePrefix, streamWriter);
                #endregion

                streamWriter.WriteLine();
                streamWriter.WriteLine("\t\t#endregion");

                // Close out the class and namespace
                streamWriter.WriteLine("\t\t}");
                streamWriter.WriteLine("}");
            }
        }

        /// <summary>
        /// Creates a string that represents the insert functionality of the data access class.
        /// </summary>
        /// <param name="table">The Table instance that this method will be created for.</param>
        /// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
        private static void CreateInsertMethod(Table table, TextWriter streamWriter)
        {
            var className = AppUtility.FormatClassName(table.Name);
            var variableName = "a" + className;

            // Append the method header
            streamWriter.WriteLine("\t\t/// <summary>");
            streamWriter.WriteLine("\t\t/// Saves a record to the " + table.Name + " table.");
            streamWriter.WriteLine("\t\t/// returns True if value saved successfullyelse false");
            streamWriter.WriteLine("\t\t/// Throw exception with message value 'EXISTS' if the data is duplicate");
            streamWriter.WriteLine("\t\t/// </summary>");
            streamWriter.WriteLine("\t\tpublic bool Insert(" + className + " " + variableName + ")");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t var blResult = false;");
            streamWriter.WriteLine("\t\t\t using (var vConn = OpenConnection())");
            streamWriter.WriteLine("\t\t\t\t {");
            streamWriter.WriteLine("\t\t\t\t var vParams = new DynamicParameters();");
            foreach (var column in table.Columns)
            { streamWriter.WriteLine("\t\t\t\t\t vParams.Add(\"@" + column.Name + "\"," + variableName + "." + AppUtility.FormatPascal(column.Name) + ");"); }
            streamWriter.WriteLine("\t\t\t\t\t int iResult = vConn.Execute(\"" + table.Name + "Insert\", vParams, commandType: CommandType.StoredProcedure);");
            streamWriter.WriteLine("\t\t\t if (iResult == -1) blResult = true;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t return blResult;");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine();
        }

        /// <summary>
        /// Creates a string that represents the update functionality of the data access class.
        /// </summary>
        /// <param name="table">The Table instance that this method will be created for.</param>
        /// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
        private static void CreateUpdateMethod(Table table, TextWriter streamWriter)
        {
            if (table.PrimaryKeys.Count <= 0 || table.Columns.Count == table.PrimaryKeys.Count ||
                table.Columns.Count == table.ForeignKeys.Count) return;
            var className = AppUtility.FormatClassName(table.Name);
            var variableName = "a" + className;

            // Append the method header
            streamWriter.WriteLine("\t\t/// <summary>");
            streamWriter.WriteLine("\t\t/// Updates record to the " + table.Name + " table.");
            streamWriter.WriteLine("\t\t/// returns True if value saved successfullyelse false");
            streamWriter.WriteLine("\t\t/// Throw exception with message value 'EXISTS' if the data is duplicate");
            streamWriter.WriteLine("\t\t/// </summary>");
            streamWriter.WriteLine("\t\tpublic bool Update(" + className + " " + variableName + ")");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t var blResult = false;");
            streamWriter.WriteLine("\t\t\t using (var vConn = OpenConnection())");
            streamWriter.WriteLine("\t\t\t\t {");
            streamWriter.WriteLine("\t\t\t\t var vParams = new DynamicParameters();");
            foreach (var column in table.Columns)
            { streamWriter.WriteLine("\t\t\t\t\t vParams.Add(\"@" + column.Name + "\"," + variableName + "." + AppUtility.FormatPascal(column.Name) + ");"); }
            streamWriter.WriteLine("\t\t\t\t\t int iResult = vConn.Execute(\"" + table.Name + "Update\", vParams, commandType: CommandType.StoredProcedure);");
            streamWriter.WriteLine("\t\t\t\t if (iResult == -1) blResult = true;");
            streamWriter.WriteLine("\t\t\t\t }");
            streamWriter.WriteLine("\t\t\treturn blResult;");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine();
        }

        /// <summary>
        /// Creates a string that represents the "select" functionality of the data access class.
        /// </summary>
        /// <param name="table">The Table instance that this method will be created for.</param>
         /// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
        private static void CreateSelectMethod(Table table, TextWriter streamWriter)
        {
            if (table.PrimaryKeys.Count <= 0 || table.Columns.Count == table.PrimaryKeys.Count ||
                table.Columns.Count == table.ForeignKeys.Count) return;
            var className = AppUtility.FormatClassName(table.Name);
            var variableName = "a" + table.PrimaryKeys[0].Name;

            // Append the method header
            streamWriter.WriteLine("\t\t/// <summary>");
            streamWriter.WriteLine("\t\t/// Selects the Single object of " + table.Name + " table.");
            streamWriter.WriteLine("\t\t/// </summary>");
            streamWriter.WriteLine("\t\tpublic "+ className + " Get"+ className +"(" + AppUtility.GetCsType(table.PrimaryKeys[0]) + " " + variableName + ")");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\t using (var vConn = OpenConnection())");
            streamWriter.WriteLine("\t\t\t\t {");
            streamWriter.WriteLine("\t\t\t\t var vParams = new DynamicParameters();");
            streamWriter.WriteLine("\t\t\t\t\t vParams.Add(\"@" + table.PrimaryKeys[0].Name + "\"," + variableName + ");"); 
            streamWriter.WriteLine("\t\t\t\t\t return vConn.Query<"+ className + ">(\"" + table.Name + "Select\", vParams, commandType: CommandType.StoredProcedure);");
            streamWriter.WriteLine("\t\t\t\t }");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine();


        }

        /// <summary>
        /// Creates a string that represents the select functionality of the data access class.
        /// </summary>
        /// <param name="table">The Table instance that this method will be created for.</param>
        /// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
        private static void CreateSelectAllMethod(Table table, TextWriter streamWriter)
        {
            if (table.Columns.Count == table.PrimaryKeys.Count || table.Columns.Count == table.ForeignKeys.Count)
                return;
            var className = AppUtility.FormatClassName(table.Name);
            // Append the method header
            streamWriter.WriteLine("\t\t/// <summary>");
            streamWriter.WriteLine("\t\t/// Selects all records from the " + table.Name + " table.");
            streamWriter.WriteLine("\t\t/// </summary>");
            streamWriter.WriteLine("\t\t public IEnumerable<" + className + "> SelectAll()");
            streamWriter.WriteLine("\t\t{");
            // Append the stored procedure execution
            streamWriter.WriteLine("\t\t\t using (var vConn = OpenConnection())");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\t return vConn.Query<" + className + ">(\"" + table.Name + "SelectAll\", commandType: CommandType.StoredProcedure).ToList();");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t}");
        }

        /// <summary>
        /// Creates a string that represents the "select by" functionality of the data access class.
        /// </summary>
        /// <param name="table">The Table instance that this method will be created for.</param>
        /// <param name="storedProcedurePrefix">The prefix that is used on the stored procedure that this method will call.</param>
        /// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
        private static void CreateSelectAllByMethods(Table table, string storedProcedurePrefix, TextWriter streamWriter)
        {
            string className = AppUtility.FormatClassName(table.Name);
            string dtoVariableName = AppUtility.FormatCamel(className);

            // Create a stored procedure for each foreign key
            foreach (List<Column> compositeKeyList in table.ForeignKeys.Values)
            {
                // Create the stored procedure name
                StringBuilder stringBuilder = new StringBuilder(255);
                stringBuilder.Append("SelectAllBy");
                for (var i = 0; i < compositeKeyList.Count; i++)
                {
                    var column = compositeKeyList[i];

                    if (i > 0)
                    {
                        stringBuilder.Append("_" + AppUtility.FormatPascal(column.Name));
                    }
                    else
                    {
                        stringBuilder.Append(AppUtility.FormatPascal(column.Name));
                    }
                }
                string methodName = stringBuilder.ToString();
                string procedureName = storedProcedurePrefix + table.Name + methodName;

                // Create the select function based on keys
                // Append the method header
                streamWriter.WriteLine("\t\t/// <summary>");
                streamWriter.WriteLine("\t\t/// Selects all records from the " + table.Name + " table by a foreign key.");
                streamWriter.WriteLine("\t\t/// </summary>");

                streamWriter.Write("\t\tpublic List<" + className + "> " + methodName + "(");
                for (int i = 0; i < compositeKeyList.Count; i++)
                {
                    Column column = compositeKeyList[i];
                    streamWriter.Write(AppUtility.CreateMethodParameter(column));
                    if (i < (compositeKeyList.Count - 1))
                    {
                        streamWriter.Write(",");
                    }
                }
                streamWriter.WriteLine(")");
                streamWriter.WriteLine("\t\t{");

                streamWriter.WriteLine("\t\t\t using (var vConn = OpenConnection())");
                streamWriter.WriteLine("\t\t\t\t {");
                streamWriter.WriteLine("\t\t\t\t var vParams = new DynamicParameters();");
                for (var i = 0; i < compositeKeyList.Count; i++)
                {
                    var column = compositeKeyList[i];
                    streamWriter.WriteLine("\t\t\t\t\t vParams.Add(\"@" + column.Name + "\"," + AppUtility.FormatCamel(column.Name) + ");");
                }
                streamWriter.WriteLine("\t\t\t\t return vConn.Query<" + className + ">(\"" + table.Name + "SelectAll\", vParams, commandType: CommandType.StoredProcedure).ToList();");
                streamWriter.WriteLine("\t\t\t\t }");
                streamWriter.WriteLine("\t\t}");
                streamWriter.WriteLine();
            }
        }

    }
}
