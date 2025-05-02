using System;
using System.Collections.Generic;

public class Angajat : AngajatMagazin
{
    public int Salariu { get; set; }
    public List<Comanda> Comenzi { get; set; } = new List<Comanda>();
    public Angajat()
    {

    }

    public Angajat(Guid id) : base(id)
    {

    }

    public Angajat(string nume, string email, string username, string password, string telefon, int salariu, List<Comanda> comenzi) : base(nume, email, username, password, telefon, TipAngajat.Angajat)
    {
        Salariu = salariu;
        Comenzi = comenzi;
    }

    public override string ToString()
    {
        return $"Angajat: {Nume} - {Email} - {Username} - {Password} - {Telefon} - {Salariu} - {Comenzi}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Angajat angajat)
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
