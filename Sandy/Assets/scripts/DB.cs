using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Terr01
{
    class DB
    {
        
        //MySql.Data.MySqlClient.MySqlConnection conn;
        private MySqlConnection conn;
        private string server;
        private string database;
        private string uid;
        private string password;
        private string myConnectionString;

        public DB()
        {
            init();
        }
        ~DB()
        {
            this.CloseConnection();
        }


        public string init()
        {
            myConnectionString = "server=127.0.0.1;uid=root;" +
        "pwd=;database=test;";
            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "";

        }


        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                //return ex.Message;
                return false;
            }


            return true;
        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                //todo return ex.Message;
                return false;
            }
        }

        //Insert statement
        public void Insert()
        {
            string query = "INSERT INTO devices0 (did, name, dtype, x,y,z, room, netid) VALUES(01, 'bob01', 1,  50, 50, 1, 101, 001)";


            // todo catch

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, conn);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Update statement
        public void Update()
        {
            string query = "UPDATE devices0 SET x=100, y=100 WHERE name='bob01'";

            // todo catch

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = conn;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete()
        {
            string query = "DELETE FROM devices0 WHERE name='bob01'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        //Select statement
        public ArrayList Select()
        {
            ArrayList list = new ArrayList();
            ArrayList line = new ArrayList();

            string query = "SELECT * FROM devices0";
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, conn);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    // line=dataReader.
                    line = new ArrayList();
                    line.Add(dataReader["name"]);

                    list.Add(line);

                    // list[1].Add(dataReader["name"] + "");
                    // list[2].Add(dataReader["age"] + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
            return null;
        }
        //Select statement
        public List<string>[] Select(string s)
        {

            return null;
        }

        //Count statement
        public int Count()
        {

            return 0;
        }





    }
}
