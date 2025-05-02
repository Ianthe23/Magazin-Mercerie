using System;

public class Patron : AngajatMagazin
{
    public Patron()
    {

    }

    public Patron(Guid id) : base(id)
    {

    }

    public Patron(string nume, string email, string username, string password, string telefon) : base(nume, email, username, password, telefon, TipAngajat.Patron)
    {

    }

    public override string ToString()
    {
        return $"Patron: {Nume} - {Email} - {Username} - {Password} - {Telefon}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Patron patron)
        {
            return Id == patron.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}
