namespace Ewallet.Mvc.Models;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; } 
    public string Email { get; private set; } 
    public string Photo { get; private set; } 
    public string FullName { get; private set; }
    public string Phone { get; private set; }
    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public DateTime UpdatedAt {get;set;} = DateTime.Now;

    public User(int id,string username, string passwordHash, string email, string photo, string fullName, string phone)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username tidak boleh kosong.", nameof(username));

        Id=id;
        Username = username;
        PasswordHash=passwordHash;
        Email = email;
        Photo=photo;
        FullName=fullName;
        Phone=phone;
    }

    public string GetInformation()
    {
        return $"Full Name: {FullName}\nEmail: {Email}\nPhone: {Phone}\nPhoto: {Photo}\nUsername: {Username}";
    }


}


