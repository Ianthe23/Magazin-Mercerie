using System.Formats.Asn1;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using log4net;

public class Service : IService
{
    private readonly IAngajatRepository _angajatRepository;
    private readonly IPatronRepository _patronRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProdusRepository _produsRepository;
    private readonly IComandaRepository _comandaRepository;
    private readonly ILog _logger;

    // ------- CONSTRUCTOR ---------
    public Service(IAngajatRepository angajatRepository, IClientRepository clientRepository, IProdusRepository produsRepository, IComandaRepository comandaRepository, IPatronRepository patronRepository)
    {
        _angajatRepository = angajatRepository;
        _clientRepository = clientRepository;
        _produsRepository = produsRepository;
        _comandaRepository = comandaRepository;
        _patronRepository = patronRepository;
        _logger = LogManager.GetLogger(typeof(Service));
        _logger.Info("Service initialized");
    }

    // ------- LOGIN & REGISTER ---------
    public async Task<User?> LoginClient(string username, string password)
    {
        _logger.Debug($"Attempting client login for username: {username}");
        var client = await _clientRepository.GetByUsernameAsync(username);
        if (client == null || client.Password != password)
        {
            _logger.Warn($"Invalid login attempt for client username: {username}");
            return null;
        }
        _logger.Info($"Successful client login: {username} (ID: {client.Id})");
        return client;
    }

    public async Task<User?> LoginAngajat(string username, string password)
    {
        _logger.Debug($"Attempting employee login for username: {username}");
        
        // First try to find a regular employee
        var angajat = await _angajatRepository.GetByUsernameAsync(username);
        if (angajat != null && angajat.Password == password)
        {
            _logger.Info($"Successful employee login: {username} (ID: {angajat.Id})");
            return angajat;
        }

        // If not found or invalid password, try to find a patron
        var patron = await _patronRepository.GetByUsernameAsync(username);
        if (patron != null && patron.Password == password)
        {
            _logger.Info($"Successful patron login: {username} (ID: {patron.Id})");
            return patron;
        }

        // No matching employee found or invalid password
        _logger.Warn($"Invalid login attempt for employee username: {username}");
        return null;
    }

    public async Task<User?> RegisterClient(string nume, string email, string username, string password, string telefon)
    {
        _logger.Debug($"Attempting to register new client: {username}, {email}");
        try
        {
            var client = new Client(nume, email, username, password, telefon, new List<Comanda>());
            await ((IRepository<Client>)_clientRepository).AddAsync(client);
            _logger.Info($"Successfully registered new client: {username} (ID: {client.Id})");
            return client;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error registering client {username}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<User?> RegisterAngajat(string nume, string email, string username, string password, string telefon)
    {
        _logger.Debug($"Attempting to register new employee: {username}, {email}");
        try
        {
            var angajat = new Angajat(nume, email, username, password, telefon, 0, new List<Comanda>());
            await ((IRepository<Angajat>)_angajatRepository).AddAsync(angajat);
            _logger.Info($"Successfully registered new employee: {username} (ID: {angajat.Id})");
            return angajat;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error registering employee {username}: {ex.Message}", ex);
            throw;
        }
    }

    // ------- CRUD ANGAJAT ---------
    public async Task<User?> AdaugareAngajat(string nume, string email, string username, string password, string telefon, int salariu)
    {
        _logger.Debug($"Adding new employee: {username}, salary: {salariu}");
        try
        {
            var angajat = new Angajat(nume, email, username, password, telefon, salariu, new List<Comanda>());
            await ((IRepository<Angajat>)_angajatRepository).AddAsync(angajat);
            _logger.Info($"Successfully added new employee: {username} (ID: {angajat.Id})");
            return angajat;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error adding employee {username}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<bool> StergereAngajat(Guid id)
    {
        _logger.Debug($"Attempting to delete employee with ID: {id}");
        try
        {
            var result = await _angajatRepository.DeleteAsync(id);
            if (result)
            {
                _logger.Info($"Successfully deleted employee with ID: {id}");
            }
            else
            {
                _logger.Warn($"Failed to delete employee with ID: {id}");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error deleting employee with ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<User?> ModificareAngajat(Guid id, string nume, string email, string username, string password, string telefon, int salariu)
    {
        var angajat = await ((IRepository<Angajat>)_angajatRepository).GetByIdAsync(id);
        if (angajat == null)
        {
            return null;
        }
        angajat.Nume = nume;
        angajat.Email = email;
        angajat.Username = username;
        angajat.Password = password;
        angajat.Telefon = telefon;
        angajat.Salariu = salariu;
        await ((IRepository<Angajat>)_angajatRepository).UpdateAsync(angajat);
        return angajat;
    }

    // ------- CRUD CLIENT ---------
    public async Task<User?> AdaugareClient(string nume, string email, string username, string password, string telefon)
    {
        var client = new Client(nume, email, username, password, telefon, new List<Comanda>());
        await ((IRepository<Client>)_clientRepository).AddAsync(client);
        return client;
    }

    public async Task<bool> StergereClient(Guid id)
    {
        return await _clientRepository.DeleteAsync(id);
    }

    public async Task<User?> ModificareClient(Guid id, string nume, string email, string username, string password, string telefon)
    {
        var client = await ((IRepository<Client>)_clientRepository).GetByIdAsync(id);
        if (client == null)
        {
            return null;
        }
        client.Nume = nume;
        client.Email = email;
        client.Username = username;
        client.Password = password;
        client.Telefon = telefon;
        await ((IRepository<Client>)_clientRepository).UpdateAsync(client);
        return client;
    }

    // ------- CRUD PRODUS ---------
    public async Task<Produs?> AdaugareProdus(string nume, TipProdus tip, int pret, int cantitate)
    {
        var produs = new Produs(nume, tip, pret, cantitate);
        await ((IRepository<Produs>)_produsRepository).AddAsync(produs);
        return produs;
    }

    public async Task<bool> StergereProdus(Guid id)
    {
        return await _produsRepository.DeleteAsync(id);
    }

    public async Task<Produs?> ModificareProdus(Guid id, string nume, TipProdus tip, int pret, int cantitate)
    {
        var produs = await ((IRepository<Produs>)_produsRepository).GetByIdAsync(id);
        if (produs == null)
        {
            return null;
        }
        produs.Nume = nume;
        produs.Tip = tip;
        produs.Pret = pret;
        produs.Cantitate = cantitate;
        await ((IRepository<Produs>)_produsRepository).UpdateAsync(produs);
        return produs;
    }

    // ------- PLASARE COMANDA ---------
    public async Task<Comanda?> PlasareComanda(Guid clientId, Guid angajatId, List<Guid> produseId, Status status)
    {
        var client = await ((IRepository<Client>)_clientRepository).GetByIdAsync(clientId);
        var angajat = await ((IRepository<Angajat>)_angajatRepository).GetByIdAsync(angajatId);
        var produse = await _produsRepository.GetByIdsAsync(produseId);
        var comanda = new Comanda(client, clientId, angajat, angajatId, status, produse);
        await ((IRepository<Comanda>)_comandaRepository).AddAsync(comanda);
        return comanda;
    }

    // ------- RETURNARE PRODUS ---------
    public async Task<Comanda?> ReturnareProdus(Guid clientId, Guid angajatId, Guid comandaId, Guid produsId)
    {
        var comanda = await ((IRepository<Comanda>)_comandaRepository).GetByIdAsync(comandaId);
        if (comanda == null)
        {
            return null;
        }
        var produs = await _produsRepository.GetByIdAsync(produsId);
        if (produs == null)
        {
            return null;
        }
        comanda.Produse.Remove(produs);
        await ((IRepository<Comanda>)_comandaRepository).UpdateAsync(comanda);
        return comanda;
    }

    // ------- AFISARE COMENZI CLIENT ---------
    public async Task<List<Comanda>> AfisareComenziClient(Guid clientId)
    {
        return await _comandaRepository.GetByClientIdAsync(clientId);
    }

    // ------- AFISARE COMENZI ANGAJAT ---------
    public async Task<List<Comanda>> AfisareComenziAngajat(Guid angajatId)
    {
        return await _comandaRepository.GetByAngajatIdAsync(angajatId);
    }

    // ------- AFISARE COMENZI ---------
    public async Task<List<Comanda>> AfisareComenzi()
    {
        return await _comandaRepository.GetAllAsync();
    }

    // ------- AFISARE ANGAJATI ---------
    public async Task<List<Angajat>> AfisareAngajati()
    {
        return await ((IRepository<Angajat>)_angajatRepository).GetAllAsync();
    }

    // ------- AFISARE CLIENTI ---------
    public async Task<List<Client>> AfisareClienti()
    {
        return await ((IRepository<Client>)_clientRepository).GetAllAsync();
    }

    // ------- AFISARE PRODUSE ---------
    public async Task<List<Produs>> AfisareProduse()
    {
        return await ((IRepository<Produs>)_produsRepository).GetAllAsync();
    }
    
}
