using System.Formats.Asn1;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using log4net;
using magazin_mercerie.Service;
using System.Linq;

public class Service : IService
{
    private readonly IAngajatRepository _angajatRepository;
    private readonly IPatronRepository _patronRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProdusRepository _produsRepository;
    private readonly IComandaRepository _comandaRepository;
    private readonly IPasswordService _passwordService;
    private readonly IProductUpdateNotificationService _notificationService;
    private readonly ILog _logger;

    // ------- CONSTRUCTOR ---------
    public Service(IAngajatRepository angajatRepository, IClientRepository clientRepository, IProdusRepository produsRepository, IComandaRepository comandaRepository, IPatronRepository patronRepository, IPasswordService passwordService, IProductUpdateNotificationService notificationService)
    {
        _angajatRepository = angajatRepository;
        _clientRepository = clientRepository;
        _produsRepository = produsRepository;
        _comandaRepository = comandaRepository;
        _patronRepository = patronRepository;
        _passwordService = passwordService;
        _notificationService = notificationService;
        _logger = LogManager.GetLogger(typeof(Service));
        _logger.Info("Service initialized");
        
        // DEBUG: Log service instance information for tracking
        _logger.Info($"🔍 SERVICE INSTANCE TRACKING:");
        _logger.Info($"   Service HashCode: {this.GetHashCode()}");
        _logger.Info($"   NotificationService HashCode: {_notificationService?.GetHashCode()}");
    }

    // ------- LOGIN & REGISTER ---------
    public async Task<User?> LoginClient(string username, string password)
    {
        _logger.Debug($"Attempting client login for username: {username}");
        var client = await _clientRepository.GetByUsernameAsync(username);
        if (client == null || !_passwordService.VerifyPassword(password, client.Password))
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
        
        try
        {
            // First try to find a regular employee
            _logger.Debug($"🔍 Looking for employee: {username}");
            var angajat = await _angajatRepository.GetByUsernameAsync(username);
            
            if (angajat != null)
            {
                _logger.Debug($"✅ Found employee: {username} (ID: {angajat.Id})");
                _logger.Debug($"🔐 Verifying password for employee: {username}");
                
                var passwordValid = _passwordService.VerifyPassword(password, angajat.Password);
                _logger.Debug($"🔐 Password verification result for {username}: {passwordValid}");
                
                if (passwordValid)
                {
                    _logger.Info($"Successful employee login: {username} (ID: {angajat.Id})");
                    return angajat;
                }
                else
                {
                    _logger.Warn($"❌ Invalid password for employee: {username}");
                }
            }
            else
            {
                _logger.Debug($"❌ Employee not found: {username}, trying patron...");
            }

            // If not found or invalid password, try to find a patron
            _logger.Debug($"🔍 Looking for patron: {username}");
            var patron = await _patronRepository.GetByUsernameAsync(username);
            
            if (patron != null)
            {
                _logger.Debug($"✅ Found patron: {username} (ID: {patron.Id})");
                _logger.Debug($"🔐 Verifying password for patron: {username}");
                
                var passwordValid = _passwordService.VerifyPassword(password, patron.Password);
                _logger.Debug($"🔐 Password verification result for patron {username}: {passwordValid}");
                
                if (passwordValid)
                {
                    _logger.Info($"Successful patron login: {username} (ID: {patron.Id})");
                    return patron;
                }
                else
                {
                    _logger.Warn($"❌ Invalid password for patron: {username}");
                }
            }
            else
            {
                _logger.Debug($"❌ Patron not found: {username}");
            }

            // No matching employee/patron found or invalid password
            _logger.Warn($"❌ FINAL RESULT: No valid employee/patron found for username: {username}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error($"💥 Exception during employee login for {username}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<User?> RegisterClient(string nume, string email, string username, string password, string telefon)
    {
        _logger.Debug($"Attempting to register new client: {username}, {email}");
        try
        {
            var hashedPassword = _passwordService.HashPassword(password);
            var client = new Client(nume, email, username, hashedPassword, telefon, new List<Comanda>());
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

    public async Task<User?> RegisterAngajat(string nume, string email, string username, string password, string telefon, TipAngajat tipAngajat)
    {
        _logger.Debug($"Attempting to register new employee: {username}, {email}");
        try
        {
            var hashedPassword = _passwordService.HashPassword(password);
            if (tipAngajat == TipAngajat.Patron)
            {
                var patron = new Patron(nume, email, username, hashedPassword, telefon);   
                await ((IRepository<Patron>)_patronRepository).AddAsync(patron);
                _logger.Info($"Successfully registered new patron: {username} (ID: {patron.Id})");
                return patron;
            } else {
                var angajat = new Angajat(nume, email, username, hashedPassword, telefon, 0, new List<Comanda>());
                await ((IRepository<Angajat>)_angajatRepository).AddAsync(angajat);
                _logger.Info($"Successfully registered new employee: {username} (ID: {angajat.Id})");
                return angajat;
            }
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
            var hashedPassword = _passwordService.HashPassword(password);
            var angajat = new Angajat(nume, email, username, hashedPassword, telefon, salariu, new List<Comanda>());
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
        
        // Only hash password if it's not already a bcrypt hash (starts with $2a$, $2b$, $2y$)
        if (!password.StartsWith("$2") || password.Length < 50)
        {
            angajat.Password = _passwordService.HashPassword(password);
        }
        else
        {
            // Password is already hashed, keep it as is
            angajat.Password = password;
        }
        
        angajat.Telefon = telefon;
        angajat.Salariu = salariu;
        await ((IRepository<Angajat>)_angajatRepository).UpdateAsync(angajat);
        return angajat;
    }

    // ------- CRUD CLIENT ---------
    public async Task<User?> AdaugareClient(string nume, string email, string username, string password, string telefon)
    {
        var hashedPassword = _passwordService.HashPassword(password);
        var client = new Client(nume, email, username, hashedPassword, telefon, new List<Comanda>());
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
        
        // Only hash password if it's not already a bcrypt hash (starts with $2a$, $2b$, $2y$)
        if (!password.StartsWith("$2") || password.Length < 50)
        {
            client.Password = _passwordService.HashPassword(password);
        }
        else
        {
            // Password is already hashed, keep it as is
            client.Password = password;
        }
        
        client.Telefon = telefon;
        await ((IRepository<Client>)_clientRepository).UpdateAsync(client);
        return client;
    }

    // ------- CRUD PRODUS ---------
    public async Task<Produs?> AdaugareProdus(string nume, TipProdus tip, decimal pret, decimal cantitate)
    {
        var produs = new Produs(nume, tip, pret, cantitate);
        await ((IRepository<Produs>)_produsRepository).AddAsync(produs);
        
        // Notify all views that products have changed
        _notificationService?.NotifyProductsChanged();
        
        return produs;
    }

    public async Task<bool> StergereProdus(Guid id)
    {
        var result = await _produsRepository.DeleteAsync(id);
        
        if (result)
        {
            // Notify all views that products have changed
            _notificationService?.NotifyProductsChanged();
        }
        
        return result;
    }

    public async Task<Produs?> ModificareProdus(Guid id, string nume, TipProdus tip, decimal pret, decimal cantitate)
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
        
        // Notify all views that this specific product quantity has changed
        _notificationService?.NotifyProductQuantityUpdated(id, cantitate);
        
        return produs;
    }

    // ------- PLASARE COMANDA ---------
    public async Task<Comanda?> PlasareComanda(Guid clientId, Guid angajatId, List<Guid> produseId, Status status)
    {
        // Convert to quantities dictionary with default quantity of 1
        var productQuantities = new Dictionary<Guid, decimal>();
        foreach (var productId in produseId)
        {
            if (productQuantities.ContainsKey(productId))
            {
                productQuantities[productId] += 1; // If product appears multiple times, increase quantity
            }
            else
            {
                productQuantities[productId] = 1;
            }
        }
        
        return await PlasareComandaWithQuantities(clientId, angajatId, productQuantities, status);
    }

    public async Task<Comanda?> PlasareComandaWithQuantities(Guid clientId, Guid angajatId, Dictionary<Guid, decimal> productQuantities, Status status)
    {
        try
        {
            var client = await ((IRepository<Client>)_clientRepository).GetByIdAsync(clientId);
            var angajat = await ((IRepository<Angajat>)_angajatRepository).GetByIdAsync(angajatId);
            
            if (client == null || angajat == null)
            {
                _logger.Warn($"Client or employee not found. ClientId: {clientId}, AngajatId: {angajatId}");
                return null;
            }
            
            // Create the order object (without ComandaProduse)
            var comanda = new Comanda(client, clientId, angajat, angajatId, status);
            
            // Use the specialized repository method to handle the complex creation
            var createdOrder = await _comandaRepository.CreateOrderWithProductsAsync(comanda, productQuantities);
            
            _logger.Info($"Successfully created order {createdOrder.Id} with {productQuantities.Count} product types");
            
            // CRITICAL FIX: Notify all views that products may have changed due to order placement
            // This ensures employee views get updated when stock changes due to orders
            _logger.Info($"🔔 TRIGGERING PRODUCT UPDATES after order placement for {productQuantities.Count} products");
            foreach (var kvp in productQuantities)
            {
                var productId = kvp.Key;
                // Get the current product to find its updated stock
                var product = await _produsRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    _logger.Info($"🔔 Notifying views: Product {product.Nume} (ID: {productId}) may have updated stock: {product.Cantitate}");
                    _notificationService?.NotifyProductQuantityUpdated(productId, product.Cantitate);
                }
            }
            
            return createdOrder;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error creating order: {ex.Message}", ex);
            throw;
        }
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
        
        // Remove the product from the order using the junction table
        var comandaProdus = comanda.ComandaProduse.FirstOrDefault(cp => cp.ProdusId == produsId);
        if (comandaProdus != null)
        {
            comanda.ComandaProduse.Remove(comandaProdus);
        }
        
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

    // ------- AFISARE COMENZI ANGAJAT (PAGINATED) ---------
    public async Task<(List<Comanda> orders, int totalCount)> AfisareComenziAngajat(Guid angajatId, int pageNumber, int pageSize)
    {
        try
        {
            // Get all orders for the employee to count total
            var allOrders = await _comandaRepository.GetByAngajatIdAsync(angajatId);
            var totalCount = allOrders.Count;
            
            // Calculate pagination
            var skip = (pageNumber - 1) * pageSize;
            var paginatedOrders = allOrders
                .OrderByDescending(o => o.Id) // Order by most recent first
                .Skip(skip)
                .Take(pageSize)
                .ToList();
            
            _logger.Debug($"Retrieved page {pageNumber} of orders for employee {angajatId}: {paginatedOrders.Count} of {totalCount} total");
            
            return (paginatedOrders, totalCount);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error getting paginated orders for employee {angajatId}: {ex.Message}", ex);
            throw;
        }
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

    // ------- LOAD BALANCING ---------
    public async Task<Angajat?> GetEmployeeWithLeastOrders()
    {
        try
        {
            _logger?.Debug("Finding employee with least orders for load balancing");
            
            // Get all employees (exclude patrons)
            var allEmployees = await ((IRepository<Angajat>)_angajatRepository).GetAllAsync();
            var regularEmployees = allEmployees.Where(e => !(e is Patron)).ToList();
            
            if (!regularEmployees.Any())
            {
                _logger?.Warn("No regular employees found for order assignment");
                return null;
            }
            
            // Find employee with minimum orders
            Angajat? bestEmployee = null;
            int minOrderCount = int.MaxValue;
            
            foreach (var employee in regularEmployees)
            {
                var orders = await _comandaRepository.GetByAngajatIdAsync(employee.Id);
                // Count only active orders (not completed)
                var activeOrders = orders.Where(o => o.Status != Status.Finalizat).ToList();
                var orderCount = activeOrders.Count;
                
                _logger?.Debug($"Employee {employee.Nume} ({employee.Id}) has {orderCount} active orders");
                
                if (orderCount < minOrderCount)
                {
                    minOrderCount = orderCount;
                    bestEmployee = employee;
                }
            }
            
            if (bestEmployee != null)
            {
                _logger?.Info($"Selected employee {bestEmployee.Nume} with {minOrderCount} active orders for load balancing");
            }
            else
            {
                _logger?.Warn("Could not find best employee for load balancing");
            }
            
            return bestEmployee;
        }
        catch (Exception ex)
        {
            _logger?.Error($"Error finding employee with least orders: {ex.Message}", ex);
            throw;
        }
    }

    // ------- EMPLOYEE ORDER MANAGEMENT ---------
    public async Task<Comanda?> ActualizareStatusComanda(Guid comandaId, Status newStatus)
    {
        _logger.Debug($"Updating order status: {comandaId} to {newStatus}");
        try
        {
            var comanda = await ((IRepository<Comanda>)_comandaRepository).GetByIdAsync(comandaId);
            if (comanda == null)
            {
                _logger.Warn($"Order not found: {comandaId}");
                return null;
            }

            comanda.Status = newStatus;
            await ((IRepository<Comanda>)_comandaRepository).UpdateAsync(comanda);
            _logger.Info($"Successfully updated order {comandaId} status to {newStatus}");
            
            // CRITICAL: Notify client about order status change
            _logger.Info($"🔔 TRIGGERING ORDER STATUS NOTIFICATION for order {comandaId}");
            _notificationService?.NotifyOrderStatusUpdated(
                comandaId, 
                comanda.ClientId, 
                newStatus, 
                comanda.Client?.Nume ?? "Unknown Client"
            );
            
            return comanda;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error updating order status {comandaId}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<bool> ActualizareCantitateProduseComanda(Guid comandaId, Dictionary<Guid, decimal> productQuantities)
    {
        _logger.Debug($"Updating product quantities for order: {comandaId}");
        try
        {
            var comanda = await ((IRepository<Comanda>)_comandaRepository).GetByIdAsync(comandaId);
            if (comanda == null)
            {
                _logger.Warn($"Order not found: {comandaId}");
                return false;
            }

            // Update stock quantities for products in the order
            foreach (var kvp in productQuantities)
            {
                var productId = kvp.Key;
                var newQuantity = kvp.Value;
                
                var product = await _produsRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    product.Cantitate = newQuantity;
                    await _produsRepository.UpdateAsync(product);
                    _logger.Debug($"Updated product {productId} quantity to {newQuantity}");
                    
                    // Notify all views about the quantity update
                    _logger.Debug($"🔔 Service {this.GetHashCode()} triggering notification via NotificationService {_notificationService?.GetHashCode()}");
                    _notificationService?.NotifyProductQuantityUpdated(productId, newQuantity);
                }
            }

            _logger.Info($"Successfully updated quantities for order {comandaId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error updating product quantities for order {comandaId}: {ex.Message}", ex);
            throw;
        }
    }
}
