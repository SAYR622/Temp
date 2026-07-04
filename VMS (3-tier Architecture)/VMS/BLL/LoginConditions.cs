using VMS.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMS.BLL
{
    class LoginConditions
    {
        private LoginDA _LoginDA = new LoginDA();

        [Obsolete]
        public bool Login(string username, string password)
        {

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Username and password cannot be empty.");
            }

            if (password.Length < 6)
            {
                throw new Exception("Password must be at least 6 characters long.");
            }


            return _LoginDA.ValidateUserInDatabase(username, password);
        }
    }
}
