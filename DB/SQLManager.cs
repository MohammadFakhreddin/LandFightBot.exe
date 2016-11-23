using System;
using UnityEngine;
using System.Collections;
using System.Data;
using Mono.Data.SqliteClient;
using System.IO;

namespace LandFightBotReborn.DB
{
    /// <summary>
    /// By M.Fakhreddin
    /// </summary>
    public class SQLManager
    {
        private const string DB_NAME = "landfight";

        //private string DB_LOCATION;

        private IDbConnection mConnection = null;
        private IDbCommand mCommand = null;
        private IDataReader mReader = null;
        private string mSQLString;

        //Initilizaing database
        public SQLManager()
        {
            onCreate();
        }

        private void onCreate()
        {
            string DBLocation = "";
            //TODO Fix db location
            DBLocation += Application.dataPath + Path.DirectorySeparatorChar + "StreamingAssets" + Path.DirectorySeparatorChar + "Database" + Path.DirectorySeparatorChar;
            DBLocation += DB_NAME + ".db";
            Console.WriteLine("Starting database at location " + DBLocation);
            Console.WriteLine("SQLiter - Opening SQLite Connection at " + DBLocation);
            if (!File.Exists(DBLocation))
            {
                File.Create(DBLocation);
            }
            mConnection = new SqliteConnection(("URI=file:" + DBLocation));
            mCommand = mConnection.CreateCommand();
        }

        /// <summary>
        /// It executes the orders directly
        /// </summary>
        public void execute(string commandText)
        {
            mConnection.Open();
            mCommand.CommandText = commandText;
            mCommand.ExecuteNonQuery();
            mConnection.Close();
        }

        public IDataReader executeReader(string commandText)
        {
            mConnection.Open();
            mCommand.CommandText = commandText;
            mReader = mCommand.ExecuteReader();
            mConnection.Close();
            return mReader;
        }

        /// <summary>
        /// Clean up everything for SQLite
        /// </summary>
        private void SQLiteClose()
        {
            if (mReader != null && !mReader.IsClosed)
                mReader.Close();
            mReader = null;

            if (mCommand != null)
                mCommand.Dispose();
            mCommand = null;

            if (mConnection != null && mConnection.State != ConnectionState.Closed)
                mConnection.Close();
            mConnection = null;
        }
    }
}