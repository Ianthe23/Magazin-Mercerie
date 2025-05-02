using System;

public class AngajatMagazin : User
{
    public TipAngajat Tip { get; set; }

    public AngajatMagazin()
    {

    }

    public AngajatMagazin(Guid id) : base(id)
    {

    }

    public AngajatMagazin(string nume, string email, string username, string password, string telefon, TipAngajat tip) : base(nume, email, username, password, telefon)
    {
        Tip = tip;
    }

    public override string ToString()
    {
        return $"AngajatMagazin: {Nume} - {Email} - {Username} - {Password} - {Telefon} - {Tip}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is AngajatMagazin angajat)
        {
            return Id == angajat.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
