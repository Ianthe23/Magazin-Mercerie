using System;

public class ComandaProdus
{
    public Guid ComandaId { get; set; }
    public Comanda Comanda { get; set; }
    
    public Guid ProdusId { get; set; }
    public Produs Produs { get; set; }
    
    public decimal CantitateComanda { get; set; } // This is the ordered quantity
    public decimal PretLaComanda { get; set; } // Price at the time of order (to handle price changes)
    
    public ComandaProdus()
    {
    }
    
    public ComandaProdus(Guid comandaId, Guid produsId, decimal cantitateComanda, decimal pretLaComanda)
    {
        ComandaId = comandaId;
        ProdusId = produsId;
        CantitateComanda = cantitateComanda;
        PretLaComanda = pretLaComanda;
    }
} 