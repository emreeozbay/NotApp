namespace NotApp.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "no-reply@ozal.edu.tr";
        public string FromName { get; set; } = "MTÜ Not/Ödev Platformu";
    }
}
