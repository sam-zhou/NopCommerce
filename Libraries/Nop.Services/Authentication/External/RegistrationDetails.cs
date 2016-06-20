//Contributor:  Nicholas Mayne


namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Registration details
    /// </summary>
    public struct RegistrationDetails
    {
        public RegistrationDetails(OpenAuthenticationParameters parameters)
            : this()
        {
            if (parameters.UserClaims != null)
                foreach (var claim in parameters.UserClaims)
                {
                    //email, username
                    if (string.IsNullOrEmpty(EmailAddress))
                    {
                        if (claim.Contact != null)
                        {
                            EmailAddress = claim.Contact.Email;
                            UserName = claim.Contact.Email;
                        }
                    }
                    //first name
                    if (string.IsNullOrEmpty(FirstName))
                        if (claim.Name != null)
                            FirstName = claim.Name.First;
                    //last name
                    if (string.IsNullOrEmpty(LastName))
                        if (claim.Name != null)
                            LastName = claim.Name.Last;

                    if (string.IsNullOrWhiteSpace(AvatarUrl))
                    {
                        if (claim.Media != null && claim.Media.Images != null)
                            AvatarUrl = claim.Media.Images.Default;
                    }


                    //email, username
                    if (string.IsNullOrEmpty(Password))
                    {
                        if (claim.Password != null)
                        {
                            Password = claim.Password.Password;
                        }
                    }
                }
        }

        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string AvatarUrl { get; set; }


        public string Password { get; set; }
    }
}