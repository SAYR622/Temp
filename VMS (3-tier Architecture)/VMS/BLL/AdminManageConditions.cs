using System;
using System.Collections.Generic;
using VMS.DAL;
namespace VMS.BLL

{
    public class AdminManageConditions
    {
        private AdminManageDA _dal = new AdminManageDA();

        public List<AdminUser> LoadStandardAdmins()
        {
            return _dal.GetStandardAdmins();
        }

        public bool CreateNewAdmin(string name, string gender, string contactStr, string username, string password)
        {
            ValidateInputs(name, contactStr, username, password);

            AdminUser newAdmin = new AdminUser
            {
                Name = name.Trim(),
                Gender = gender,
                Contact = Convert.ToInt32(contactStr),
                Username = username.Trim(),
                Password = password
            };

            return _dal.InsertAdmin(newAdmin);
        }

        public bool UpdateExistingAdmin(long adminId, string name, string gender, string contactStr, string username, string password)
        {
            ValidateInputs(name, contactStr, username, password);

            AdminUser updatedAdmin = new AdminUser
            {
                AdminID = adminId,
                Name = name.Trim(),
                Gender = gender,
                Contact = Convert.ToInt32(contactStr),
                Username = username.Trim(),
                Password = password
            };

            return _dal.UpdateAdmin(updatedAdmin);
        }

        public bool DeleteAdmin(long adminId)
        {
            if (adminId <= 0) throw new Exception("Invalid Admin ID selected.");
            return _dal.DeleteAdmin(adminId);
        }

        private void ValidateInputs(string name, string contact, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Name, Username, and Password are required.");

            if (!int.TryParse(contact, out _))
                throw new Exception("Contact must be a valid number (without leading zeros for int4 storage).");

            if (password.Length < 6)
                throw new Exception("Password must be at least 6 characters.");
        }

        // Add inside AdminManageConditions.cs (your BLL)
        public List<PendingRequest> LoadPendingRequests()
        {
            return _dal.GetPendingRequests();
        }

        public bool ApproveRequest(PendingRequest req)
        {
            // 1. Try to create the new standard admin
            bool isCreated = CreateNewAdmin(req.FullName, req.Gender, req.Contact.ToString(), req.Username, req.Password);

            // 2. If successfully created in 'admins', remove it from 'admin_req'
            if (isCreated)
            {
                return _dal.DeleteRequest(req.Id);
            }
            return false;
        }

        public bool RejectRequest(long reqId)
        {
            return _dal.DeleteRequest(reqId);
        }
    }
}