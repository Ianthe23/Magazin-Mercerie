using System;
using System.Collections.Generic;
using System.Linq;

public class Comanda : Entity<Guid>
{
    public Client Client { get; set; }
    public Guid ClientId { get; set; }
    public Angajat Angajat { get; set; }
    public Guid AngajatId { get; set; }
    public Status Status { get; set; }
    
    // Use junction table instead of direct many-to-many
    public List<ComandaProdus> ComandaProduse { get; set; } = new List<ComandaProdus>();
    
    // Convenience property to get products (for backward compatibility)
    public List<Produs> Produse => ComandaProduse?.Select(cp => cp.Produs).ToList() ?? new List<Produs>();
    
    // Calculated property for total amount using ordered quantities and prices
    public decimal TotalAmount => ComandaProduse?.Sum(cp => cp.CantitateComanda * cp.PretLaComanda) ?? 0;

    public Comanda()
    {
        ComandaProduse = new List<ComandaProdus>();
    }

    public Comanda(Guid id) : base(id)
    {
        ComandaProduse = new List<ComandaProdus>();
    }

    public Comanda(Client client, Guid clientId, Angajat angajat, Guid angajatId, Status status) : base(Guid.NewGuid())
    {
        Client = client;
        ClientId = clientId;
        Angajat = angajat;
        AngajatId = angajatId;
        Status = status;
        ComandaProduse = new List<ComandaProdus>();
    }

    public override string ToString()
    {
        return $"Comanda: {Id} - {Client} - {Angajat} - {Status} - {ComandaProduse?.Count ?? 0} products";
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
