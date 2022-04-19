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
        public void TestingSQLConnection(string sqlserver)
        {
            string connString = $@"Server={sqlserver};Database=PIVision;Integrated Security=true;MultipleActiveResultSets=true"; /*---> using integrated security*/

            SqlConnection connection = new SqlConnection(connString);
            connection.Open();
        }

        public string ValidatingSQLConnection()
        {
            Utilities util = new Utilities();
            util.WriteInGreen("Enter the SQL Database instance hosting the PIVision database:");
            bool repeat = true;
            string sqlInstance = "";


            while (repeat)
            {
                Console.ForegroundColor = ConsoleColor.White;
                sqlInstance = Console.ReadLine();

                try
                {
                    TestingSQLConnection(sqlInstance);
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
                    util.WriteInGreen("Something went wrong. Enter the SQL Database instance hosting the PIVision database:");
                    repeat = true;
                }
            }
            return sqlInstance;
        }
    }


}
