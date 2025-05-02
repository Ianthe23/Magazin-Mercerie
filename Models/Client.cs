using System;
using System.Collections.Generic;
public class Client : User
{
    public List<Comanda> Comenzi { get; set; } = new List<Comanda>();
    public Client()
    {

    }

    public Client(Guid id) : base(id)
    {

    }

    public Client(string nume, string email, string username, string password, string telefon, List<Comanda> comenzi) : base(nume, email, username, password, telefon)
    {
        Comenzi = comenzi;
    }

    public override string ToString()
    {
        return $"Client: {Nume} - {Email} - {Username} - {Password} - {Telefon}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Client client)
        {
            return Id == client.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
