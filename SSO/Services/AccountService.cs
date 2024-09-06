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
            UserDataModel userData = new UserDataModel();
            userData.IsSuccess = false;
            userData.Message = "Invalid Username or Password, Please try again.";
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Open();
                }
                string command = $"SELECT UserId, Username, FirstName, LastName, Password FROM tblUserLogin WHERE Username = '{login.Username}' AND Password = '{login.Password}' AND IsActive = 1";
                SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = commandTimeout;
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        if (sqlDataReader.HasRows)
                        {
                            while (sqlDataReader.Read())
                            {
                                userData.UserId = Convert.ToInt64(sqlDataReader["UserId"].ToString());
                                userData.UserName = sqlDataReader["Username"].ToString();
                                userData.FirstName = sqlDataReader["FirstName"].ToString();
                                userData.LastName = sqlDataReader["LastName"].ToString();
                                userData.Password = sqlDataReader["Password"].ToString();
                            }

                            if (userData.UserId > 0)
                            {
                                userData.IsSuccess = true;
                                UpdateLastLogInDateTime(userData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        userData.IsSuccess = false;
                        userData.Message = ex.Message;
                        if (sqlConnection.State != ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                    }
                }
            }
            return userData;
        }

        private void UpdateLastLogInDateTime(UserDataModel user)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Open();
                }
                string command = $"UPDATE tblUserLogIn SET LastLogInDateTime = GETDATE() WHERE UserId = {user.UserId}";
                SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = commandTimeout;
                sqlCommand.ExecuteNonQuery();
            }
        }

        public UserDataModel FindUserByUserName(string userName)
        {
            UserDataModel userData = new UserDataModel();
            userData.IsSuccess = false;
            userData.Message = "Invalid Credentials";
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
                        if (sqlDataReader.HasRows)
                        {
                            while (sqlDataReader.Read())
                            {
                                userData.UserId = Convert.ToInt64(sqlDataReader["UserId"].ToString());
                                userData.UserName = sqlDataReader["Username"].ToString();
                                userData.FirstName = sqlDataReader["FirstName"].ToString();
                                userData.LastName = sqlDataReader["LastName"].ToString();
                            }

                            if (userData.UserId > 0)
                            {
                                userData.IsSuccess = true;
                                UpdateLastLogInDateTime(userData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        userData.IsSuccess = false;
                        userData.Message = ex.Message;
                        if (sqlConnection.State != ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                    }
                }
            }
            return userData;
        }
    }
}
