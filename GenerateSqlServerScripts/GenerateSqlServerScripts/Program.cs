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
        const string FilePath = "D:\\Temp\\";
        const string SchemaFile = FilePath + "Schemas.sql";
        const string InitDataFile = FilePath + "Init.sql";

        static void Main(string[] args)
        {
            ServerConnection serverConnection = new ServerConnection(new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString));
            Server srv = new Server(serverConnection);
            //srv = new Server(new ServerConnection("phoenix", "sa", "sa"));
            //Reference the AdventureWorks database. 
            Database db = srv.Databases[serverConnection.DatabaseName];//("AdventureWorks"); 
            //Database db = srv.Databases["CRM_8.0.1"];
            //Define a Scripter object and set the required scripting options. 
            Scripter scrp = new Scripter(srv);

            scrp.Options = GetScripterOptions();

            //GenerateTables(db, scrp);

            //GenerateFunctions(db, scrp);
            //GenerateViews(db, scrp);
            //GenerateStoredProcedures(db, scrp);

            GenerateInitData(db, scrp);
        }

        private static void GenerateFunctions(Database db, Scripter scrp)
        {
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
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }
                File.AppendAllText(SchemaFile, strSql.ToString(), Encoding.Unicode);
            }
        }

        private static void GenerateViews(Database db, Scripter scrp)
        {
            Console.WriteLine("/**===== Start GenerateViews =====**/");
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
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }
                File.AppendAllText(SchemaFile, strSql.ToString(), Encoding.Unicode);
            }
        }

        private static void GenerateStoredProcedures(Database db, Scripter scrp)
        {
            Console.WriteLine("/**===== Start GenerateStoredProcedures =====**/");
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
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { item.Urn });
                foreach (string st in sc)
                {
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }
                File.AppendAllText(SchemaFile, strSql.ToString(), Encoding.Unicode);
            }
        }

        private static void GenerateTables(Database db, Scripter scrp)
        {
            Console.WriteLine("/**===== Start GenerateTables =====**/");
            // Iterate through the tables in database and script each one. Display the script.   
            foreach (Table tb in db.Tables)
            {
                // check if the table is not a system table
                if (tb.IsSystemObject == true)
                {
                    continue;
                }
                StringBuilder strSql = new StringBuilder();
                scrp.Options.ScriptDrops = true;
                // Generating script for table tb
                System.Collections.Specialized.StringCollection sc = scrp.Script(new Urn[] { tb.Urn });
                foreach (string st in sc)
                {
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }

                scrp.Options.ScriptDrops = false;
                sc = scrp.Script(new Urn[] { tb.Urn });
                foreach (string st in sc)
                {
                    strSql.Append(st);
                    strSql.Append(Environment.NewLine);
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }
                File.AppendAllText(SchemaFile, strSql.ToString(), Encoding.Unicode);
            }
        }

        private static void GenerateInitData(Database db, Scripter scrp)
        {
            List<string> listTablesNeedData = File.ReadAllLines("TablesNeedInitData.txt").ToList<string>();

            Console.WriteLine("/**===== Start GenerateInitData =====**/");
            scrp.Options.ScriptData = true;
            scrp.Options.ScriptSchema = false;
            //scrp.Options.IncludeHeaders = true;
            foreach (Table item in db.Tables)
            {
                // check if the table is not a system table
                if (item.IsSystemObject == true)
                {
                    continue;
                }

                if (!listTablesNeedData.Any(x => x.ToLower() == item.Name.ToLower()))
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
                    strSql.Append(st);
                    if (!st.EndsWith("\r\n"))
                    {
                        strSql.Append(Environment.NewLine);
                    }
                    strSql.Append("GO");
                    strSql.Append(Environment.NewLine);
                }

                File.AppendAllText(InitDataFile, strSql.ToString(), Encoding.Unicode);
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
            options.WithDependencies = true;
            options.DriChecks = true;
            options.DriForeignKeys = true;
            options.DriPrimaryKey = true;
            options.DriUniqueKeys = true;
            options.Indexes = true;
            options.DriAllConstraints = true;

            return options;
        }
    }
}
