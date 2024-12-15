using ppm_fe.Models;

namespace ppm_fe.Controls;

public partial class FlyoutHeaderControl : StackLayout
{

    string roleText = string.Empty; // Default value because mapping fails
                                    //private string _fullname = {App.UserDetails.Firstname} ++ {App.UserDetails.Lastname};
    public FlyoutHeaderControl()
    {
        InitializeComponent();

        BindingContext = App.UserDetails;

        if (App.UserDetails.Role_ID.HasValue)
        {
            if (Enum.TryParse(App.UserDetails.Role_ID.Value.ToString(), out UserRole parsedRole))
            {
                roleText = parsedRole.ToString();
            }
            else
            {
                Console.WriteLine($"Error: Invalid Role_ID value: {App.UserDetails.Role_ID.Value}");
                roleText = "Unknown Role";
            }
        }
        else
        {
            Console.WriteLine("Error: Role_ID is null");
        }

        if (App.UserDetails != null)
        {
            lblUserName.Text = App.UserDetails.Firstname;
            //lblUserEmail.Text = App.UserDetails.Email;
            lblUserEmail.Text = MaskEmail(App.UserDetails.Email);
            lblUserRole.Text = roleText;
            App.UserDetails.Approved = App.UserDetails.Approved;
        }
    }
    private string MaskEmail_(string email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        if (email.Length <= 4)
            return email;

        return email.Substring(0, 4) + new string('*', email.Length - 4);
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex == -1) // If there's no '@', just mask everything after the first 4 characters
        {
            return email.Length <= 4 ? email : email.Substring(0, 4) + new string('*', email.Length - 4);
        }

        var localPart = email.Substring(0, atIndex);
        var domainPart = email.Substring(atIndex + 1);

        // Mask local part
        var maskedLocalPart = localPart.Length <= 4
            ? localPart
            : localPart.Substring(0, 4) + new string('*', localPart.Length - 4);

        // Mask domain part
        var domainParts = domainPart.Split('.');
        var maskedDomainPart = string.Join(".", domainParts.Select(part =>
            part.Length <= 1 ? part : part[0] + new string('*', part.Length - 1)));

        return $"{maskedLocalPart}@{maskedDomainPart}";
    }
}