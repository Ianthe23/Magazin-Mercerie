using System;
using System.Collections.Generic;

public class Comanda : Entity<Guid>
{
    public Client Client { get; set; }
    public Guid ClientId { get; set; }
    public Angajat Angajat { get; set; }
    public Guid AngajatId { get; set; }
    public Status Status { get; set; }
    public List<Produs> Produse { get; set; }

    public Comanda()
    {

    }

    public Comanda(Guid id) : base(id)
    {

    }

    public Comanda(Client client, Guid clientId, Angajat angajat, Guid angajatId, Status status, List<Produs> produse) : base(Guid.NewGuid())
    {
        Client = client;
        ClientId = clientId;
        Angajat = angajat;
        AngajatId = angajatId;
        Status = status;
        Produse = produse;
    }

    public override string ToString()
    {
        return $"Comanda: {Id} - {Client} - {Angajat} - {Status} - {Produse}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Comanda comanda)
        {
            return Id == comanda.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    } 
}
