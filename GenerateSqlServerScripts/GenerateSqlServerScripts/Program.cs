using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace ConsoleApplication1
{
    class Program
    {
        public static string FilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["OutputFilePath"];
            }
        }
        public static int GenerateMode
        {
            get
            {
                int tmpMode = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["GenerateMode"], out tmpMode))
                {
                    tmpMode = (int)GenerateModeEnum.SchemaAndData;
                }
                return tmpMode;
            }
        }
        public static string SchemaFile
        {
            get
            {
                return FilePath + "Schema.sql";
            }
        }
        public static string InitDataFile
        {
            get
            {
                return FilePath + "Init.sql";
            }
        }

        static void Main(string[] args)
        {
            if (File.Exists(SchemaFile))
            {
                File.Delete(SchemaFile);
            }
            if (File.Exists(InitDataFile))
            {
                File.Delete(InitDataFile);
            }

            ServerConnection serverConnection = new ServerConnection(new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString));
            Server srv = new Server(serverConnection);
            //srv = new Server(new ServerConnection("phoenix", "sa", "sa"));
            //Reference the AdventureWorks database. 
            Database db = srv.Databases[serverConnection.DatabaseName];//("AdventureWorks"); 
            //Database db = srv.Databases["CRM_8.0.1"];
            //Define a Scripter object and set the required scripting options. 
            Scripter scrp = new Scripter(srv);
            srv.ConnectionContext.SqlExecutionModes = SqlExecutionModes.CaptureSql;

            scrp.Options = GetScripterOptions();

            if (GenerateMode == (int)GenerateModeEnum.SchemaAndData || GenerateMode == (int)GenerateModeEnum.OnlySchema)
            {
                GenerateTables(db, scrp);
                GenerateFunctions(db, scrp);
                GenerateViews(db, scrp);
                GenerateStoredProcedures(db, scrp);
            }

            if (GenerateMode == (int)GenerateModeEnum.SchemaAndData || GenerateMode == (int)GenerateModeEnum.OnlyData)
            {
                GenerateInitData(db, scrp);
            }
        }

        private static void GenerateTables(Database db, Scripter scrp)
        {
            Console.WriteLine("/**===== Start GenerateTables =====**/");
            // Iterate through the tables in database and script each one. Display the script.   

            string strDefaultContraints = "IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND parent_object_id = OBJECT_ID(N'[dbo].[{1}]'))";

            List<string> list = new List<string>();
            foreach (Table tb in db.Tables)
            {
                // check if the table is not a system table
                if (tb.IsSystemObject == true)
                {
                    continue;
                }

                StringBuilder strSql = new StringBuilder();
                scrp.Options.ScriptDrops = true;
                scrp.Options.DriIncludeSystemNames = true;


                foreach (Column col in tb.Columns)
                {
                    if (col.DefaultConstraint != null)
                    {
                        StringCollection sc = col.DefaultConstraint.Script(scrp.Options);
                        foreach (string st in sc)
                        {
                            string dropConstraint = st;

                            strSql.AppendFormat(strDefaultContraints, col.DefaultConstraint.Name, tb.Name);
                            strSql.AppendFormat(Environment.NewLine + "BEGIN" + Environment.NewLine);
                            strSql.AppendFormat(st);
                            strSql.AppendFormat(Environment.NewLine + "END" + Environment.NewLine);
                            strSql.AppendFormat("GO" + Environment.NewLine);
                        }
                    }
                }

                // Generating script for table tb
                StringCollection scDrop = scrp.Script(new Urn[] { tb.Urn });
                foreach (string st in scDrop)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                StringCollection scCreate = scrp.Script(new Urn[] { tb.Urn });
                foreach (string st in scCreate)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }

                list.Add(strSql.ToString());
            }
            foreach (var item in list)
            {
                File.AppendAllText(SchemaFile, item, Encoding.Unicode);
            }

        }

        private static void GenerateFunctions(Database db, Scripter scrp)
        {
            List<string> list = new List<string>();
            foreach (UserDefinedFunction item in db.UserDefinedFunctions)
            {
                // check if the table is not a system table
                if (item.IsSystemObject == true)
                {
                    continue;
                }
                StringBuilder strSql = new StringBuilder();
                scrp.Options.ScriptDrops = true;
                // Generating script for table tb
                System.Collections.Specialized.StringCollection sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }

                list.Add(strSql.ToString());
            }

            foreach (var item in list)
            {
                File.AppendAllText(SchemaFile, item, Encoding.Unicode);
            }
        }

        private static void GenerateViews(Database db, Scripter scrp)
        {
            Console.WriteLine("/**===== Start GenerateViews =====**/");
            List<string> list = new List<string>();

            foreach (View item in db.Views)
            {
                // check if the table is not a system table
                if (item.IsSystemObject == true)
                {
                    continue;
                }

                StringBuilder strSql = new StringBuilder();

                scrp.Options.ScriptDrops = true;
                // Generating script for table tb
                System.Collections.Specialized.StringCollection sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }
                list.Add(strSql.ToString());
            }

            foreach (var item in list)
            {
                File.AppendAllText(SchemaFile, item, Encoding.Unicode);
            }
        }

        private static void GenerateStoredProcedures(Database db, Scripter scrp)
        {
            Console.WriteLine("/**===== Start GenerateStoredProcedures =====**/");

            List<string> list = new List<string>();
            foreach (StoredProcedure item in db.StoredProcedures)
            {
                // check if the table is not a system table
                if (item.IsSystemObject == true)
                {
                    continue;
                }
                StringBuilder strSql = new StringBuilder();
                scrp.Options.ScriptDrops = true;
                // Generating script for table tb
                System.Collections.Specialized.StringCollection sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine + "GO" + Environment.NewLine);
                }
                list.Add(strSql.ToString());
            }

            foreach (var item in list)
            {
                File.AppendAllText(SchemaFile, item, Encoding.Unicode);
            }
        }

        private static void GenerateInitData(Database db, Scripter scrp)
        {
            List<string> listTablesNeedData = File.ReadAllLines("TablesNeedInitData.txt").ToList<string>();
            List<string> list = new List<string>();

            Console.WriteLine("/**===== Start GenerateInitData =====**/");
            scrp.Options.ScriptData = true;
            scrp.Options.ScriptSchema = false;
            //scrp.Options.WithDependencies = true;
            //scrp.Options.IncludeHeaders = true;
            foreach (Table item in db.Tables)
            {
                // check if the table is not a system table
                if (item.IsSystemObject == true)
                {
                    continue;
                }

                if (!listTablesNeedData.Any(x => x.Trim().ToLower() == item.Name.ToLower()))
                {
                    continue;
                }

                StringBuilder strSql = new StringBuilder();

                // Generating script for table tb
                IEnumerable<string> sc = scrp.EnumScript(new Urn[] { item.Urn });

                if (sc.Count() > 0)
                {
                    strSql.AppendFormat("/****** Object:  Table {0}    Script Date: {1} ******/{2}", item.ToString(), DateTime.Now, Environment.NewLine);
                }
                foreach (string st in sc)
                {
                    Console.WriteLine(st);
                    strSql.Append(st);
                    if (!st.EndsWith("\r\n"))
                    {
                        strSql.Append(Environment.NewLine);
                    }
                }
                strSql.Append("GO" + Environment.NewLine);

                File.AppendAllText(InitDataFile, strSql.ToString(), Encoding.Unicode);
                //list.Add(strSql.ToString());
            }

            foreach (var item in list)
            {

            }
        }


        private static ScriptingOptions GetScripterOptions()
        {
            ScriptingOptions options = new ScriptingOptions();
            //General
            options.AnsiPadding = true;
            options.IncludeHeaders = true;
            options.IncludeIfNotExists = true;
            //Schema qualify object names.
            options.SchemaQualify = true;
            options.Default = true;
            options.ExtendedProperties = true;
            options.TargetServerVersion = SqlServerVersion.Version100;

            options.NoCollation = true;
            options.ScriptSchema = true;
            //options.WithDependencies = true;
            options.DriChecks = true;
            options.DriForeignKeys = true;
            options.DriPrimaryKey = true;
            options.DriUniqueKeys = true;
            options.Indexes = true;
            options.DriAllConstraints = true;
            options.DriAll = true;
            return options;
        }

        private enum GenerateModeEnum
        {
            SchemaAndData = 1,
            OnlySchema = 2,
            OnlyData = 3
        }
    }
}
