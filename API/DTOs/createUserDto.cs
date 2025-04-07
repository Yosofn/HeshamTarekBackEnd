namespace API.DTOs
{
    public class createUserDto
    {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Name { get; set; }
            public string? Phone1 { get; set; }
            public string? Phone2 { get; set; }
            public int Government { get; set; }
            public string? Address { get; set; }
            public string? SchoolName { get; set; }
            public int UserType { get; set; }
            public string? NationalId { get; set; }
            public int Balance { get; set; }
            public int Status { get; set; }
            public DateTime RegisterDate { get; set; }
            public string? Imei { get; set; }
            public string? Country { get; set; }
            public string? StudentId { get; set; }
            public DateTime ExpireDate { get; set; }
            public int Points { get; set; }
            public int UserStatus { get; set; }
            public string? ImeiDesktop { get; set; }
            public string? ImeiDesktopOnline { get; set; }
            public string? FilePath { get; set; }
        }

    
}
