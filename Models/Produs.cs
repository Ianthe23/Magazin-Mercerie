using System;

public class Produs : Entity<Guid>
{
    public string Nume { get; set; }
    public TipProdus Tip { get; set; }
    public decimal Pret { get; set; }
    public decimal Cantitate { get; set; }

    public Produs()
    {

    }

    public Produs(Guid id) : base(id)
    {

    }

    public Produs(string nume, TipProdus tip, decimal pret, decimal cantitate) : base(Guid.NewGuid())
    {
        Nume = nume;
        Tip = tip;
        Pret = pret;
        Cantitate = cantitate;
    }

    public override string ToString()
    {
        return $"Produs: {Nume} - {Tip} - {Pret} - {Cantitate}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Produs produs)
        {
            return Id == produs.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
