using System;

public class User : Entity<Guid>
{
    public string Nume { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Telefon { get; set; }

    public User()
    {

    }

    public User(Guid id) : base(id)
    {

    }

    public User(string nume, string email, string username, string password, string telefon) : base(Guid.NewGuid())
    {
        Nume = nume;
        Email = email;
        Username = username;
        Password = password;
        Telefon = telefon;
    }

    public override string ToString()
    {
        return $"User: {Nume} - {Email} - {Username} - {Password} - {Telefon}";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is User user)
        {
            return Id == user.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}