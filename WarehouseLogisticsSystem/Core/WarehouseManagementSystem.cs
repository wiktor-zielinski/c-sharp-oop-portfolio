using System;
using System.Collections.Generic;
using System.Linq;
using WarehouseLogisticsSystem.Models;
using WarehouseLogisticsSystem.Users;

namespace WarehouseLogisticsSystem.Core
{
    public class WarehouseManagementSystem
    {
        public IWarehouseDatabase Database { get; }
        public List<WarehouseManager> Managers { get; } = new();
        public List<User> StaffMembers { get; } = new();

        public WarehouseManager? CurrentManager { get; private set; }
        public User? CurrentUser { get; private set; }

        public WarehouseManagementSystem(IWarehouseDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Database.OnStockAlert += msg => Database.AddLog($"[ALARM MAGAZYNOWY] {msg}");
            SeedInitialData();
        }

        private void SeedInitialData()
        {
            // Domyślny kierownik magazynu (hasło haszowane SHA-256)
            Managers.Add(new WarehouseManager(1, "inż. Marek Nowicki", "admin123"));

            // Domyślny zespół operacyjny
            StaffMembers.Add(new LogisticsCoordinator(101, "Damian Kowalski", "log123", "Strefa Składowania Wysokiego A-B"));
            StaffMembers.Add(new QualityAuditor(201, "mgr inż. Natalia Wójcik", "qa123", "Laboratorium Kontroli Jakości i Powłok"));

            // Inicjalizacja towarów w bazie SQL tylko wtedy, gdy tabela jest pusta
            if (Database.ListOfProducts.Count == 0)
            {
                var p1 = new Product("ALU-PRF-200", "Profil Aluminiowy Konstrukcyjny 40x40 (Anodowany)",
                    ProductCategory.ProfileAluminiowe, "A-01-02", 120, 25, 85.50m);
                p1.AddAuditNote("System WMS", "Inicjalizacja partii startowej po kontroli wymiarowej.");

                var p2 = new Product("POW-RAL-7016", "Farba Proszkowa Poliestrowa RAL 7016 (Antracyt 20kg)",
                    ProductCategory.ChemiaPrzemyslowa, "C-04-01", 12, 15, 340.00m);
                p2.AddAuditNote("inż. Marek Nowicki", "Stan poniżej progu minimalnego - wymagane zamówienie uzupełniające.");

                var p3 = new Product("PLC-MOD-S7", "Sterownik Przemysłowy PLC Siemens S7-1200",
                    ProductCategory.KomponentyElektroniczne, "B-02-05", 18, 5, 1150.00m);
                p3.AddAuditNote("mgr inż. Natalia Wójcik", "Testy napięciowe zaliczone pomyślnie.");

                var p4 = new Product("PAL-EUR-EPAL", "Paleta Przemysłowa Drewniana Certyfikowana EPAL",
                    ProductCategory.MaterialyOpakowaniowe, "D-01-01", 250, 40, 48.00m);

                Database.Add(p1);
                Database.Add(p2);
                Database.Add(p3);
                Database.Add(p4);
                Database.AddLog("Zainicjowano bazę danych WarehouseDB domyślnym asortymentem przemysłowym.");
            }
        }

        public bool LoginManager(int id, string password)
        {
            var manager = Managers.FirstOrDefault(m => m.VerifyCredentials(id, password));
            if (manager != null)
            {
                CurrentManager = manager;
                CurrentUser = null;
                return true;
            }
            return false;
        }

        public bool LoginStaff(int id, string password)
        {
            var user = StaffMembers.FirstOrDefault(u => u.VerifyCredentials(id, password));
            if (user != null)
            {
                CurrentUser = user;
                CurrentManager = null;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentManager = null;
            CurrentUser = null;
        }

        public void RegisterStaffMember(string name, int id, string password, string roleOption, string sector)
        {
            if (Managers.Any(m => m.ID == id) || StaffMembers.Any(u => u.ID == id))
                throw new InvalidOperationException($"Konto o identyfikatorze #{id} już istnieje w systemie.");

            if (roleOption == "1")
                StaffMembers.Add(new LogisticsCoordinator(id, name, password, sector));
            else if (roleOption == "2")
                StaffMembers.Add(new QualityAuditor(id, name, password, sector));
            else
                throw new ArgumentException("Nieprawidłowy wybór stanowiska.");
        }

        public bool RemoveStaffMember(int id)
        {
            var staff = StaffMembers.FirstOrDefault(u => u.ID == id);
            return staff != null && StaffMembers.Remove(staff);
        }
    }
}