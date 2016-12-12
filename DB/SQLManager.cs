using LandFightBotReborn.Utils;
using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;
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

        private SQLiteConnection mConnection = null;
        private SQLiteCommand mCommand = null;
        private SQLiteDataReader mReader = null;
        private string mSQLString;

        //Initilizaing database
        public SQLManager()
        {
            onCreate();
        }

        private void onCreate()
        {
            string DBLocation = "";
            DBLocation += ".." + Path.DirectorySeparatorChar.ToString() + ".." +Path.DirectorySeparatorChar.ToString()+".."+Path.DirectorySeparatorChar.ToString()
                          + "Database" + Path.DirectorySeparatorChar.ToString();
            DBLocation += DB_NAME + ".db";
            Logger.info("Starting database at location " + DBLocation);
            Logger.info("SQLiter - Opening SQLite Connection at " + DBLocation);
            if (!File.Exists(DBLocation))
            {
                File.Create(DBLocation);
            }
            mConnection = new SQLiteConnection(("URI=file:" + DBLocation));
            Logger.info("Creating sqllite connection successful");
            mCommand = new SQLiteCommand(mConnection);
            mConnection.Open();
        }

        /// <summary>
        /// It executes the orders directly
        /// </summary>
        public void execute(string commandText)
        {
            if (mReader != null && !mReader.IsClosed) mReader.Close();
            if (mCommand != null) mCommand.Dispose();
            if (mConnection != null && mConnection.State != ConnectionState.Closed) mConnection.Close();
            mConnection.Open();
            mCommand = new SQLiteCommand(mConnection);
            mCommand.CommandText = commandText;
            mCommand.ExecuteNonQuery();
        }

        public SQLiteDataReader executeReader(string commandText)
        {
            if (mReader != null && !mReader.IsClosed) mReader.Close();
            if (mCommand != null) mCommand.Dispose();
            if (mConnection != null && mConnection.State != ConnectionState.Closed) mConnection.Close();
            mConnection.Open();
            mCommand = new SQLiteCommand(mConnection);
            mCommand.CommandText = commandText;
            mReader = mCommand.ExecuteReader();
            //mConnection.Close();
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