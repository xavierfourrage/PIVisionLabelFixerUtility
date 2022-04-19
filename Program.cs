using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PIVisionLabelFixerUtility
{
    class Program
    {
        static StreamWriter file = new StreamWriter("PIVisionLabelFixerUtility_output.txt", append: false);
        static void Main(string[] args)
        {
            file.AutoFlush = true;
            SQLdata visiondata = new SQLdata();
            Utilities util = new Utilities();

            string sqlInstance = visiondata.ValidatingSQLConnection();
            util.WriteInGreen("Connection to the PIVision SQL database successful");
            util.WriteInGreen("Retrieving records...");

            DataTable dt = new DataTable();           
            dt = pullDataFromSQL(dt, sqlInstance, "PIVision");
            
            util.WriteInYellow("This utility will scan all Value symbols that contain TypeLabels = Description, Partial, Full, or A, and then remove the CustomName field if present");
            util.WriteInRed("Make sure you have taken a backup of your PIVision SQL database");

            bool confirm = util.Confirm("Do you want to proceed with bulk edits?");
            if (confirm)
            {
                editDataTable(dt);
                util.WriteInBlue("Updating the SQL database... do not close the window.");
                publishToSQL(dt, sqlInstance);
                util.WriteInGreen("Output has been saved under: PIVisionLabelFixerUtility_output.txt");
                util.PressEnterToExit();
            }
           else
            {
                util.PressEnterToExit();
            }           
        }

        static public DataTable pullDataFromSQL(DataTable dataTable, string sqlserver, string visionDatabase)
        {
            string connString = $@"Server={sqlserver};Database={visionDatabase};Integrated Security=true;MultipleActiveResultSets=true"; /*---> using integrated security*/
            string query = @"SELECT [DisplayID],[Name], EditorDisplay FROM[dbo].[View_Displays]WHERE EditorDisplay Like '%""NameType"":""[ADFP]"",""CustomName"":""[^""]%'";

            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();

            SqlDataAdapter da = new SqlDataAdapter(cmd);

            da.Fill(dataTable);
            conn.Close();
            da.Dispose();

            return dataTable;
        }

        static public void editDataTable(DataTable dt)
        {
            Utilities util = new Utilities();
            foreach(DataRow dr in dt.Rows)
            {
                util.WriteInBlue("Display ID: " + dr["DisplayID"].ToString()+" ("+ dr["Name"].ToString()+")");
                file.WriteLine("Display ID: " + dr["DisplayID"].ToString() + " (" + dr["Name"].ToString() + ")");
                JObject json = JObject.Parse(dr["EditorDisplay"].ToString());
                /*util.WriteInYellow(json.ToString());*/

                foreach (var item in json["Symbols"])
                {
                    if(item["SymbolType"].ToString() == "collection")
                    {
                        var StencilSymbols = item["StencilSymbols"];
                        foreach (var obj in StencilSymbols)
                        {
                            /*util.WriteInYellow(obj.ToString());*/
                            var config = obj["Configuration"];
                            if ((string)config["NameType"] != "C" && config["CustomName"] != null && obj["SymbolType"].ToString() == "value")
                            {
                                util.WriteInWrite(obj["Name"].ToString());
                                file.WriteLine(obj["Name"].ToString());

                                util.WriteInYellow("NameType: " + config["NameType"].ToString() + ", removing-> CustomName: \"" + config["CustomName"].ToString() + "\"");
                                file.WriteLine("NameType: " + config["NameType"].ToString() + ", removing-> CustomName: \"" + config["CustomName"].ToString() + "\"");
                                config["CustomName"].Parent.Remove();
                            }
                        }
                    }
                    else
                    {
                        var config = item["Configuration"];

                        if ((string)config["NameType"] != "C" && config["CustomName"] != null && item["SymbolType"].ToString() == "value")
                        {
                            util.WriteInWrite(item["Name"].ToString());
                            file.WriteLine(item["Name"].ToString());

                            util.WriteInYellow("NameType: " + config["NameType"].ToString() + ", removing-> CustomName: \"" + config["CustomName"].ToString() + "\"");
                            file.WriteLine("NameType: " + config["NameType"].ToString() + ", removing-> CustomName: \"" + config["CustomName"].ToString() + "\"");
                            config["CustomName"].Parent.Remove();
                        }
                    }                 
                }
                    dr["EditorDisplay"] = JsonConvert.SerializeObject(json);
            }
        }

        static public void publishToSQL(DataTable dt, string sqlserver)
        {
            Utilities util = new Utilities();
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    UpdateSQL(sqlserver, row["EditorDisplay"].ToString(), row["DisplayID"].ToString(),"PIVision");
                }
            }
            catch (SqlException ex)
            {
                StringBuilder errorMessages = new StringBuilder();

                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }

                util.WriteInRed("Failed to write to the [PIVision].[dbo].[View_Displays] table");
                file.WriteLine("Failed to write to the [PIVision].[dbo].[View_Displays] table");
                util.WriteInRed(errorMessages.ToString());
                file.WriteLine(errorMessages.ToString());
            }
        }

        static public void UpdateSQL(string sqlserver, string newEditorDisplayValue, string id, string pivisionDatabase)
        {
            string connString = $@"Server={sqlserver};Database={pivisionDatabase};Integrated Security=true;MultipleActiveResultSets=true"; /*---> using integrated security*/
            string query = $@"UPDATE [{pivisionDatabase}].[dbo].[View_Displays] SET [EditorDisplay]=@newString WHERE DisplayID=@Id";

            using (SqlConnection con = new SqlConnection(connString))
            {
                SqlCommand command = new SqlCommand(query, con);
                command.Parameters.Add("@newString", SqlDbType.VarChar).Value = newEditorDisplayValue;
                command.Parameters.Add("@Id", SqlDbType.NVarChar).Value = id;
                con.Open();
                command.ExecuteNonQuery();
                con.Close();
            }
        }
    }
}
