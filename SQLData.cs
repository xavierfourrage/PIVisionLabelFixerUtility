using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace PIVisionLabelFixerUtility
{
    class SQLdata
    {
        public void TestingSQLConnection(string sqlserver, string piVisionSQLdBName)
        {
            string connString = $@"Server={sqlserver};Database={piVisionSQLdBName};Integrated Security=true;MultipleActiveResultSets=true"; /*---> using integrated security*/

            SqlConnection connection = new SqlConnection(connString);
            connection.Open();
        }

        public (string,string) ValidatingPIVisionSQLdBName()
        {
            Utilities util = new Utilities();       
            bool repeat = true;
            string sqlInstance = "";
            string piVisionSQLdBName = "";

            while (repeat)
            {
                util.WriteInGreen("Enter the SQL Server instance name:");
                Console.ForegroundColor = ConsoleColor.White;
                sqlInstance = Console.ReadLine();

                util.WriteInGreen("Enter the PI Vision SQL database name:");              
                Console.ForegroundColor = ConsoleColor.White;
                piVisionSQLdBName = Console.ReadLine();

                try
                {
                    util.WriteInYellow("Validating connection to the PIVision SQL database...");
                    TestingSQLConnection(sqlInstance,piVisionSQLdBName);
                    repeat = false;
                }
                catch (SqlException ex)
                {
                    StringBuilder errorMessages = new StringBuilder();
                    util.WriteInRed("Could not connect to your PI Vision SQL database.");
                    for (int i = 0; i < ex.Errors.Count; i++)
                    {
                        errorMessages.Append("Index #" + i + "\n" +
                            "Message: " + ex.Errors[i].Message + "\n" +
                            "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                            "Source: " + ex.Errors[i].Source + "\n" +
                            "Procedure: " + ex.Errors[i].Procedure + "\n");
                    }
                    util.WriteInRed(errorMessages.ToString());
                    util.WriteInGreen("Something went wrong.");
                    repeat = true;
                }
            }
            return (sqlInstance,piVisionSQLdBName);
        }
    }


}
