using System.Threading.Tasks;
using System;
using System.Collections.Generic;
public interface IService
{
    // LOGIN & REGISTER CLIENT
    Task<User?> LoginClient(string username, string password);
    Task<User?> RegisterClient(string nume, string email, string username, string password, string telefon);

    // LOGIN & REGISTER ANGAJAT
    Task<User?> LoginAngajat(string username, string password);
    Task<User?> RegisterAngajat(string nume, string email, string username, string password, string telefon, TipAngajat tipAngajat);

    // CRUD ANGAJAT
    Task<User?> AdaugareAngajat(string nume, string email, string username, string password, string telefon, int salariu);
    Task<bool> StergereAngajat(Guid id);
    Task<User?> ModificareAngajat(Guid id, string nume, string email, string username, string password, string telefon, int salariu);

    // CRUD CLIENT
    Task<User?> AdaugareClient(string nume, string email, string username, string password, string telefon);
    Task<bool> StergereClient(Guid id);
    Task<User?> ModificareClient(Guid id, string nume, string email, string username, string password, string telefon);

    // CRUD PRODUS
    Task<Produs?> AdaugareProdus(string nume, TipProdus tip, decimal pret, decimal cantitate);
    Task<bool> StergereProdus(Guid id);
    Task<Produs?> ModificareProdus(Guid id, string nume, TipProdus tip, decimal pret, decimal cantitate);

    // PLASARE COMANDA
    Task<Comanda?> PlasareComanda(Guid clientId, Guid angajatId, List<Guid> produseId, Status status);
    Task<Comanda?> PlasareComandaWithQuantities(Guid clientId, Guid angajatId, Dictionary<Guid, decimal> productQuantities, Status status);
    Task<Comanda?> ReturnareProdus(Guid clientId, Guid angajatId, Guid comandaId, Guid produsId);

    // AFISARE COMENZI
    Task<List<Comanda>> AfisareComenziClient(Guid clientId);
    Task<List<Comanda>> AfisareComenziAngajat(Guid angajatId);
    Task<(List<Comanda> orders, int totalCount)> AfisareComenziAngajat(Guid angajatId, int pageNumber, int pageSize);
    Task<List<Comanda>> AfisareComenzi();

    // EMPLOYEE ORDER MANAGEMENT
    Task<Comanda?> ActualizareStatusComanda(Guid comandaId, Status newStatus);
    Task<bool> ActualizareCantitateProduseComanda(Guid comandaId, Dictionary<Guid, decimal> productQuantities);

    // AFISARE INFORMATII
    Task<List<Angajat>> AfisareAngajati();
    Task<List<Client>> AfisareClienti();
    Task<List<Produs>> AfisareProduse();
    
    // LOAD BALANCING
    Task<Angajat?> GetEmployeeWithLeastOrders();
}
