using Microsoft.Extensions.Configuration;
using SSO.DataModels;
using SSO.ViewModels;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SSO.Services
{
    public class AccountService : IAccountService
    {
        private readonly string connectionString;
        private readonly int commandTimeout;

        public AccountService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            commandTimeout = int.TryParse(configuration.GetSection("CommandTimeout").Value, out int timeout) ? timeout : 20000;
        }

        public UserDataModel UserLogin(LoginViewModel login)
        {
            UserDataModel userDataModel = new UserDataModel();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Open();
                }
                string command = $"SELECT UserId, Username, FirstName, LastName, Password FROM tblUserLogin WHERE Username = '{login.Username}' AND Password = '{login.Password}'";
                SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = commandTimeout;
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        while (sqlDataReader.Read())
                        {
                            userDataModel.UserId = Convert.ToInt64(sqlDataReader["UserId"].ToString());
                            userDataModel.UserName = sqlDataReader["Username"].ToString();
                            userDataModel.FirstName = sqlDataReader["FirstName"].ToString();
                            userDataModel.LastName = sqlDataReader["LastName"].ToString();
                            userDataModel.Password = sqlDataReader["Password"].ToString();
                        }
                        userDataModel.IsSuccess = true;
                        userDataModel.Message = "Success";
                    }
                    catch (Exception ex)
                    {
                        userDataModel.IsSuccess = false;
                        userDataModel.Message = ex.Message;
                        if (sqlConnection.State != ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                    }
                }
            }
            return userDataModel;
        }

        public UserDataModel FindUserByUserName(string userName)
        {
            UserDataModel userDataModel = new UserDataModel();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Open();
                }
                string command = $"SELECT UserId, Username, FirstName, LastName, Password FROM tblUserLogin WHERE Username = '{userName}'";
                SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = commandTimeout;
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        while (sqlDataReader.Read())
                        {
                            userDataModel.UserId = Convert.ToInt64(sqlDataReader["UserId"].ToString());
                            userDataModel.UserName = sqlDataReader["Username"].ToString();
                            userDataModel.FirstName = sqlDataReader["FirstName"].ToString();
                            userDataModel.LastName = sqlDataReader["LastName"].ToString();
                        }
                        userDataModel.IsSuccess = true;
                        userDataModel.Message = "Success";
                    }
                    catch (Exception ex)
                    {
                        userDataModel.IsSuccess = false;
                        userDataModel.Message = ex.Message;
                        if (sqlConnection.State != ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                    }
                }
            }
            return userDataModel;
        }
    }
}
