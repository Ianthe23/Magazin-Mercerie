using System;
using System.Collections.Generic;

namespace magazin_mercerie.Service
{
    // Event args for employee status changes
    public class EmployeeStatusChangedEventArgs : EventArgs
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public bool IsOnline { get; set; }
        
        public EmployeeStatusChangedEventArgs(Guid employeeId, string employeeName, bool isOnline)
        {
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            IsOnline = isOnline;
        }
    }

    public interface IUserSessionService
    {
        User? CurrentUser { get; }
        bool IsLoggedIn { get; }
        
        void SetCurrentUser(User user);
        void ClearCurrentUser();
        
        Guid? GetCurrentUserId();
        Client? GetCurrentClient();
        Angajat? GetCurrentEmployee();
        
        // NEW: Support for multiple concurrent users
        void SetWindowUser(string windowId, User user);
        void ClearWindowUser(string windowId);
        User? GetWindowUser(string windowId);
        Client? GetClientForWindow(string windowId);
        Angajat? GetEmployeeForWindow(string windowId);
        
        // Helper method to get the most recent client (for backward compatibility)
        Client? GetMostRecentClient();
        
        // NEW: Employee online status methods
        List<Angajat> GetAllLoggedInEmployees();
        bool IsEmployeeOnline(Guid employeeId);
        bool IsEmployeeOnline(string username);
        
        // Employee-specific window management
        Angajat? GetEmployeeForAngajatWindow(Guid employeeId);
        void SetEmployeeForAngajatWindow(Guid employeeId, Angajat employee);
        void ClearEmployeeForAngajatWindow(Guid employeeId);
        
        // Employee status change notifications
        event EventHandler<EmployeeStatusChangedEventArgs>? EmployeeStatusChanged;
    }
} 