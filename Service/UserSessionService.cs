using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace magazin_mercerie.Service
{
    public class UserSessionService : IUserSessionService
    {
        private readonly ILog _logger;
        private User? _currentUser;
        private readonly Dictionary<string, User> _windowUsers = new();

        public User? CurrentUser => _currentUser;
        
        public bool IsLoggedIn => _currentUser != null;

        // Event for employee status changes
        public event EventHandler<EmployeeStatusChangedEventArgs>? EmployeeStatusChanged;

        public UserSessionService()
        {
            _logger = LogManager.GetLogger(typeof(UserSessionService));
            _logger.Info($"UserSessionService initialized - Instance HashCode: {this.GetHashCode()}");
        }

        public void SetCurrentUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _currentUser = user;
            _logger.Info($"Current user set: {user.Username} (ID: {user.Id}) on instance {this.GetHashCode()}");
        }

        public void ClearCurrentUser()
        {
            if (_currentUser != null)
            {
                _logger.Info($"Clearing current user: {_currentUser.Username} on instance {this.GetHashCode()}");
                _currentUser = null;
            }
        }

        public Guid? GetCurrentUserId()
        {
            _logger.Debug($"GetCurrentUserId called on instance {this.GetHashCode()}, current user: {(_currentUser?.Username ?? "null")}");
            return _currentUser?.Id;
        }

        public Client? GetCurrentClient()
        {
            _logger.Debug($"GetCurrentClient called on instance {this.GetHashCode()}, current user: {(_currentUser?.Username ?? "null")}");
            
            // First try the current user
            var client = _currentUser as Client;
            if (client != null)
            {
                _logger.Debug($"Returning current user as client: {client.Username}");
                return client;
            }
            
            // If current user is not a client, find the most recent client from window users
            var mostRecentClient = GetMostRecentClient();
            _logger.Debug($"Returning most recent client: {(mostRecentClient?.Username ?? "null")}");
            return mostRecentClient;
        }

        public Angajat? GetCurrentEmployee()
        {
            return _currentUser as Angajat;
        }

        // NEW: Multi-user support methods
        public void SetWindowUser(string windowId, User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _windowUsers[windowId] = user;
            _logger.Info($"Window user set: {user.Username} for window {windowId}");
        }

        public void ClearWindowUser(string windowId)
        {
            if (_windowUsers.ContainsKey(windowId))
            {
                var user = _windowUsers[windowId];
                _windowUsers.Remove(windowId);
                _logger.Info($"Cleared window user: {user.Username} for window {windowId}");
            }
        }

        public User? GetWindowUser(string windowId)
        {
            return _windowUsers.TryGetValue(windowId, out var user) ? user : null;
        }

        public Client? GetClientForWindow(string windowId)
        {
            return GetWindowUser(windowId) as Client;
        }

        public Angajat? GetEmployeeForWindow(string windowId)
        {
            return GetWindowUser(windowId) as Angajat;
        }

        public Client? GetMostRecentClient()
        {
            // Get all clients from window users and current user
            var allClients = new List<Client>();
            
            // Add current user if it's a client
            if (_currentUser is Client currentClient)
            {
                allClients.Add(currentClient);
            }
            
            // Add all window users that are clients
            foreach (var windowUser in _windowUsers.Values)
            {
                if (windowUser is Client client)
                {
                    allClients.Add(client);
                }
            }
            
            // Return the first client found (most recent)
            var result = allClients.FirstOrDefault();
            _logger.Debug($"GetMostRecentClient found {allClients.Count} clients, returning: {(result?.Username ?? "null")}");
            return result;
        }

        // NEW: Employee online status methods
        public List<Angajat> GetAllLoggedInEmployees()
        {
            var loggedInEmployees = new List<Angajat>();
            
            // Add current user if it's an employee
            if (_currentUser is Angajat currentEmployee)
            {
                loggedInEmployees.Add(currentEmployee);
            }
            
            // Add all window users that are employees
            foreach (var windowUser in _windowUsers.Values)
            {
                if (windowUser is Angajat employee && !loggedInEmployees.Any(e => e.Id == employee.Id))
                {
                    loggedInEmployees.Add(employee);
                }
            }
            
            _logger.Debug($"Found {loggedInEmployees.Count} logged-in employees: {string.Join(", ", loggedInEmployees.Select(e => e.Username))}");
            return loggedInEmployees;
        }
        
        // NEW: Employee-specific window management
        public Angajat? GetEmployeeForAngajatWindow(Guid employeeId)
        {
            var windowKey = $"AngajatWindow_{employeeId}";
            return GetWindowUser(windowKey) as Angajat;
        }
        
        public void SetEmployeeForAngajatWindow(Guid employeeId, Angajat employee)
        {
            var windowKey = $"AngajatWindow_{employeeId}";
            SetWindowUser(windowKey, employee);
            _logger.Info($"Set employee {employee.Username} for AngajatWindow with key: {windowKey}");
            
            // Trigger employee status changed event
            EmployeeStatusChanged?.Invoke(this, new EmployeeStatusChangedEventArgs(employeeId, employee.Nume, true));
            _logger.Debug($"Triggered EmployeeStatusChanged event: {employee.Nume} is now ONLINE");
        }
        
        public void ClearEmployeeForAngajatWindow(Guid employeeId)
        {
            var windowKey = $"AngajatWindow_{employeeId}";
            
            // Get employee info before clearing (for event notification)
            var employee = GetWindowUser(windowKey) as Angajat;
            string employeeName = employee?.Nume ?? "Unknown";
            
            ClearWindowUser(windowKey);
            _logger.Info($"Cleared employee for AngajatWindow with key: {windowKey}");
            
            // CRITICAL FIX: Also clear global current user if it's the same employee
            if (_currentUser is Angajat currentEmployee && currentEmployee.Id == employeeId)
            {
                _logger.Info($"Also clearing global current user since {employeeName} (ID: {employeeId}) was the current user");
                _currentUser = null;
            }
            
            // Trigger employee status changed event
            EmployeeStatusChanged?.Invoke(this, new EmployeeStatusChangedEventArgs(employeeId, employeeName, false));
            _logger.Debug($"Triggered EmployeeStatusChanged event: {employeeName} is now OFFLINE");
        }

        public bool IsEmployeeOnline(Guid employeeId)
        {
            // Check current user
            if (_currentUser is Angajat currentEmployee && currentEmployee.Id == employeeId)
            {
                _logger.Debug($"Employee {employeeId} is online (current user)");
                return true;
            }
            
            // Check window users
            foreach (var windowUser in _windowUsers.Values)
            {
                if (windowUser is Angajat employee && employee.Id == employeeId)
                {
                    _logger.Debug($"Employee {employeeId} is online (window user)");
                    return true;
                }
            }
            
            _logger.Debug($"Employee {employeeId} is offline");
            return false;
        }

        public bool IsEmployeeOnline(string username)
        {
            // Check current user
            if (_currentUser is Angajat currentEmployee && 
                string.Equals(currentEmployee.Username, username, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug($"Employee {username} is online (current user)");
                return true;
            }
            
            // Check window users
            foreach (var windowUser in _windowUsers.Values)
            {
                if (windowUser is Angajat employee && 
                    string.Equals(employee.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Debug($"Employee {username} is online (window user)");
                    return true;
                }
            }
            
            _logger.Debug($"Employee {username} is offline");
            return false;
        }
    }
} 