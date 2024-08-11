namespace UserManagementAPI.Models
{
    public class UserModel
    {
        public class ChangePasswordModel
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}
