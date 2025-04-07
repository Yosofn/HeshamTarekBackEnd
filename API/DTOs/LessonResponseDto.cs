namespace API.DTOs
{
    public class LessonResponseDto
    {

        public int Id { get; set; }
        public int MaterialId { get; set; }
        public MaterialResponse? Material { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? EncryptionCode { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int LessonPriority { get; set; }
        public bool IsBought { get; set; }
        public string? TrainerName { get; set; }
        public int StudentsNo { get; set; }
        public List<LessonApiDto> LessonApiDto { get; set; } = new List<LessonApiDto>();
    }
    public class MaterialResponse
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public string? Name { get; set; }
        public UserResponse? User { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
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
        public decimal Balance { get; set; }
        public int Status { get; set; }
        public DateTime RegisterDate { get; set; }
        public string? IMEI { get; set; }
        public string? IMEIDesktop { get; set; }
        public string? IMEIDesktopOnline { get; set; }
        public string? Country { get; set; }
        public string? StudentID { get; set; }
        public DateTime ExpireDate { get; set; }
        public int Points { get; set; }
        public int UserStatus { get; set; }
    }

}
