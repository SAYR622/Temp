using System;
using System.Text.RegularExpressions;
using VMS.DAL;

namespace VMS.BLL
{
    public class AdminRequestConditions
    {
        private AdminRequestDA _dal = new AdminRequestDA();

        public bool ProcessAccountRequest(string fullName, string username, string contactStr, string genderStr, string password)
        {
            // 1. Basic Validations
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(contactStr) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("All input fields are mandatory. Please fill them completely.");
            }

            if (Regex.IsMatch(fullName, @"[0-9]"))
            {
                throw new Exception("Full Name cannot contain numbers.");
            }

            if (!Regex.IsMatch(contactStr, @"^[0-9]{10}$"))
            {
                throw new Exception("Please enter a valid 10-digit Contact Number.");
            }

            if (password.Length < 6)
            {
                throw new Exception("Password must be at least 6 characters long for security purposes.");
            }

            // ---> NEW LOGIC: Check if the username is already in the database <---
            if (_dal.IsUsernameTaken(username))
            {
                throw new Exception("This username is already taken. Please choose another one.");
            }

            // 2. Data Conversion
            long contactNumber = Convert.ToInt64(contactStr);
            bool isMale = (genderStr == "Male");

            // 3. Pack data into the Model
            AdminRequest newRequest = new AdminRequest
            {
                FullName = fullName,
                Username = username,
                Contact = contactNumber,
                Gender = isMale,
                Password = password
            };

            // 4. Send to DAL to save
            return _dal.InsertAdminRequest(newRequest);
        }
    }
}