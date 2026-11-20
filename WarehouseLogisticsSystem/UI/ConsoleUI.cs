using System;
using System.Collections.Generic;
using System.Text;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;
using WarehouseLogisticsSystem.Users;

namespace WarehouseLogisticsSystem.UI
{
    public class ConsoleUI
    {
        private const int MenuWidth = 80;
        private readonly WarehouseManagementSystem _system;

        public ConsoleUI(WarehouseManagementSystem system)
        {
            _system = system;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                var products = _system.Database.ListOfProducts;
                int lowStockCount = products.Count(p => p.Status == StockStatus.NiskiStan || p.Status == StockStatus.Wyczerpany);
                int qaHoldCount = products.Count(p => p.Status == StockStatus.ZablokowanyQA);

                PrintTopBorder();
                PrintCenteredText("SYSTEM ZARZĄDZANIA MAGAZYNEM I LOGISTYKĄ B2B (WMS)");
                PrintMiddleBorder();
                PrintCenteredText($"Pozycje SKU: {products.Count} | Alerty niskiego stanu: {lowStockCount} | Blokady QA: {qaHoldCount}");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Zaloguj się do systemu");
                PrintLeftText("2 - Zarejestruj nowego pracownika (Logistyk / Inspektor QA)");
                PrintLeftText("3 - Dziennik operacji logistycznych i alarmów magazynowych");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Zamknij system WMS");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                if (choice == "0") break;
                else if (choice == "1") HandleLogin();
                else if (choice == "2") HandleStaffRegistration();
                else if (choice == "3") ShowSystemLogs();
            }
        }
        private void HandleLogin()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("AUTORYZACJA UŻYTKOWNIKA");
            PrintMiddleBorder();
            PrintEmptyLine();
            PrintCenteredText("Konta demonstracyjne:");
            PrintEmptyLine();

            foreach (var m in _system.Managers)
                PrintLeftText($"Kierownik WMS | ID: {m.ID,-6} | Hasło: {m.DemoPlainPassword}");

            foreach (var u in _system.StaffMembers)
            {
                string shortRole = u is LogisticsCoordinator ? "Logistyk B2B " : "Inspektor QA ";
                PrintLeftText($"{shortRole} | ID: {u.ID,-6} | Hasło: {u.DemoPlainPassword}");
            }

            PrintEmptyLine();
            PrintBottomBorder();

            Console.Write("\nPodaj ID pracownika (liczba): ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("\nBłąd: Identyfikator musi być liczbą całkowitą!");
                Console.ReadKey(intercept: true);
                return;
            }

            Console.Write("Hasło: ");
            string password = Console.ReadLine()?.Trim() ?? "";

            if (_system.LoginManager(id, password))
                ShowManagerMenu();
            else if (_system.LoginStaff(id, password))
                ShowStaffMenu();
            else
            {
                Console.WriteLine("\nOdmowa dostępu: Nieprawidłowe ID lub hasło!");
                Console.ReadKey(intercept: true);
            }
        }

        private void HandleStaffRegistration()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTRACJA NOWEGO PRACOWNIKA MAGAZYNU");
            PrintBottomBorder();

            Console.Write("\nImię i nazwisko: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Identyfikator (liczba, np. 102): ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Stanowisko (1 - Koordynator Logistyki, 2 - Inspektor Kontroli Jakości QA): ");
                string role = Console.ReadLine()?.Trim() ?? "1";

                Console.Write("Przypisany sektor magazynowy: ");
                string sector = Console.ReadLine() ?? "Strefa Główna";

                Console.Write("Ustaw hasło (zostanie zaszyfrowane algorytmem SHA-256): ");
                string pass = Console.ReadLine()?.Trim() ?? "";

                try
                {
                    _system.RegisterStaffMember(name, id, pass, role, sector);
                    Console.WriteLine("\nKonto pracownika zostało utworzone i zabezpieczone skrótem SHA-256!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nBłąd rejestracji: {ex.Message}");
                }
            }
            else Console.WriteLine("\nBłąd: ID musi być liczbą!");

            Console.ReadKey(intercept: true);
        }

        private void ShowManagerMenu()
        {
            var manager = _system.CurrentManager!;
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL KIEROWNIKA MAGAZYNU ({manager.Name})");
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE KATALOGIEM SKU, ANALITYKA SQL I EKSPORT RAPORTÓW");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl pełny rejestr asortymentu magazynowego");
                PrintLeftText("2 - Wyszukaj towar (po kodzie SKU, nazwie, regale lub statusie)");
                PrintLeftText("3 - Dodaj nowy produkt (SKU) do katalogu magazynowego");
                PrintLeftText("4 - Edytuj cenę jednostkową lub lokalizację regałową towaru");
                PrintLeftText("5 - Usuń wycofany indeks SKU z bazy danych");
                PrintLeftText("6 - Raport Analityczny Wyceny Magazynu (SQL Server GROUP BY)");
                PrintLeftText("7 - Eksportuj raport magazynowo-finansowy do pliku CSV (Excel)");
                PrintLeftText("8 - Wyświetl statystyki wydajności personelu (KPI)");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyloguj się");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                if (choice == "0")
                {
                    _system.Logout();
                    break;
                }

                Console.WriteLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            ViewAllProducts();
                            break;
                        case "2":
                            Console.Write("Wpisz szukany kod SKU, nazwę lub regał: ");
                            var found = manager.SearchCatalog(_system.Database, Console.ReadLine() ?? "").ToList();
                            Console.WriteLine($"\nZnaleziono pozycji: {found.Count}");
                            found.ForEach(p => Console.WriteLine($"-> {p}"));
                            break;
                        case "3":
                            Console.Write("Kod SKU (np. ALU-PRF-500): "); string sku = Console.ReadLine() ?? "";
                            Console.Write("Nazwa towaru: "); string name = Console.ReadLine() ?? "";
                            Console.WriteLine("Kategoria (0-ProfileAluminiowe, 1-KomponentyElektroniczne, 2-ChemiaPrzemyslowa, 3-MaterialyOpakowaniowe): ");
                            int.TryParse(Console.ReadLine(), out int catIdx);
                            var category = Enum.IsDefined(typeof(ProductCategory), catIdx) ? (ProductCategory)catIdx : ProductCategory.ProfileAluminiowe;

                            Console.Write("Lokalizacja regałowa (np. A-02-01): "); string rack = Console.ReadLine() ?? "A-01-01";
                            Console.Write("Ilość początkowa (szt.): "); int.TryParse(Console.ReadLine(), out int qty);
                            Console.Write("Próg alarmowy niskiego stanu (szt.): "); int.TryParse(Console.ReadLine(), out int minTh);
                            Console.Write("Cena jednostkowa netto (PLN): "); decimal.TryParse(Console.ReadLine(), out decimal price);

                            var newProduct = new Product(sku, name, category, rack, qty, minTh, price);
                            newProduct.AddAuditNote(manager.Name, "Utworzono kartotekę towarową w systemie WMS.");
                            manager.RegisterNewProduct(_system.Database, newProduct);
                            Console.WriteLine($"\nZarejestrowano nowy indeks SKU '{newProduct.Sku}' w bazie SQL!");
                            break;
                        case "4":
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU produktu do edycji: ");
                            var toEdit = _system.Database.GetBySku(Console.ReadLine() ?? "");
                            if (toEdit != null)
                            {
                                Console.WriteLine($"Edytujesz: {toEdit.Name} | Regał: {toEdit.RackLocation} | Cena: {toEdit.UnitNetPrice:C}");
                                Console.Write("Nowa lokalizacja regałowa (Enter by pominąć): ");
                                string newRack = Console.ReadLine() ?? "";
                                if (!string.IsNullOrWhiteSpace(newRack)) toEdit.RackLocation = newRack.Trim().ToUpperInvariant();

                                Console.Write("Nowa cena netto PLN (Enter by pominąć): ");
                                string priceStr = Console.ReadLine() ?? "";
                                if (decimal.TryParse(priceStr, out decimal newPrice) && newPrice > 0)
                                    toEdit.UnitNetPrice = newPrice;

                                toEdit.AddAuditNote(manager.Name, $"Zaktualizowano parametry magazynowe (Regał: {toEdit.RackLocation}, Cena netto: {toEdit.UnitNetPrice:C}).");
                                _system.Database.Update(toEdit);
                                Console.WriteLine("\nZapisano zmiany w bazie SQL!");
                            }
                            else Console.WriteLine("Nie znaleziono produktu o podanym SKU.");
                            break;
                        case "5":
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU produktu do trwałego usunięcia: ");
                            string delSku = Console.ReadLine() ?? "";
                            manager.DeleteProductFromCatalog(_system.Database, delSku);
                            Console.WriteLine("\nPomyślnie usunięto produkt oraz jego historię audytową z bazy SQL.");
                            break;
                        case "6":
                            ShowSqlAnalyticsReport();
                            break;
                        case "7":
                            var products = _system.Database.ListOfProducts;
                            var summaries = _system.Database.GetCategoryAnalytics();
                            string path = CsvReportExporter.ExportInventoryReport(products, summaries);
                            _system.Database.AddLog($"[RAPORT CSV] {manager.Name} wyeksportował raport magazynowy do pliku.");
                            Console.WriteLine("\nSukces! Wygenerowano profesjonalny raport CSV dla programu Excel:");
                            Console.WriteLine($"Ścieżka pliku: {path}");
                            break;
                        case "8":
                            ShowStaffKpiDashboard();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[BŁĄD OPERACJI]: {ex.Message}");
                }

                Console.WriteLine("\nNaciśnij dowolny klawisz, aby kontynuować...");
                Console.ReadKey(intercept: true);
            }
        }

        private void ShowStaffMenu()
        {
            var user = _system.CurrentUser!;
            bool isLogistics = user is LogisticsCoordinator;

            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL OPERACYJNY: {user.RoleName.ToUpper()} ({user.Name})");
                PrintMiddleBorder();
                PrintCenteredText($"Sektor: {user.AssignedSector} | Przeładowane jedn.: {user.Account.TotalUnitsHandled} szt.");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl stany magazynowe i karty audytowe towarów");
                PrintLeftText("2 - Wyszukaj towar (po SKU, nazwie lub lokalizacji)");
                PrintLeftText("3 - Dodaj notatkę służbową do karty produktu");

                if (isLogistics)
                {
                    PrintLeftText("4 - Przyjmij dostawę zewnętrzną na magazyn (Dokument PZ)");
                    PrintLeftText("5 - Wydaj towar dla klienta B2B ze strategią spedycji (Dokument WZ)");
                }
                else
                {
                    PrintLeftText("4 - Nałóż blokadę Kontroli Jakości na partię towaru (Blokada QA)");
                    PrintLeftText("5 - Zwolnij partię towaru z blokady jakościowej (Zwolnienie QA)");
                }

                PrintEmptyLine();
                PrintMiddleBorder();
                PrintLeftText("01 - Moje statystyki wydajności operacyjnej (KPI)");
                PrintLeftText("0  - Wyloguj się");
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                if (choice == "0")
                {
                    _system.Logout();
                    break;
                }

                Console.WriteLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            ViewAllProducts();
                            break;
                        case "2":
                            Console.Write("Szukana fraza (SKU / nazwa / regał): ");
                            var results = _system.Database.Search(Console.ReadLine() ?? "").ToList();
                            results.ForEach(p => Console.WriteLine($"-> {p}"));
                            break;
                        case "3":
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU produktu: ");
                            var prodForNote = _system.Database.GetBySku(Console.ReadLine() ?? "");
                            if (prodForNote != null)
                            {
                                Console.Write("Treść adnotacji magazynowej: ");
                                prodForNote.AddAuditNote(user.Name, Console.ReadLine() ?? "");
                                _system.Database.Update(prodForNote);
                                Console.WriteLine("Zapisano adnotację w bazie SQL!");
                            }
                            else Console.WriteLine("Nie znaleziono produktu.");
                            break;
                        case "4" when user is LogisticsCoordinator lcForInbound:
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU przyjmowanego towaru (PZ): ");
                            var inboundProd = _system.Database.GetBySku(Console.ReadLine() ?? "");
                            if (inboundProd != null)
                            {
                                Console.Write("Ilość przyjmowana z dostawy (szt.): ");
                                int.TryParse(Console.ReadLine(), out int addQty);
                                Console.Write("Nazwa dostawcy / producenta: ");
                                string supplier = Console.ReadLine() ?? "Dostawca Przemysłowy";

                                Console.WriteLine("\n" + lcForInbound.ProcessInboundDelivery(inboundProd, addQty, supplier, _system.Database));
                            }
                            else Console.WriteLine("Nie znaleziono towaru o podanym kodzie SKU.");
                            break;
                        case "5" when user is LogisticsCoordinator lcForOutbound:
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU wydawanego towaru (WZ): ");
                            var outboundProd = _system.Database.GetBySku(Console.ReadLine() ?? "");
                            if (outboundProd != null)
                            {
                                Console.Write("Ilość zamawiana przez klienta (szt.): ");
                                int.TryParse(Console.ReadLine(), out int reqQty);
                                Console.Write("Nazwa firmy odbiorcy B2B: ");
                                string client = Console.ReadLine() ?? "Kontrahent B2B";

                                Console.WriteLine("\nWybierz strategię spedycyjną i taryfową (Strategy Pattern):");
                                Console.WriteLine("1 - Standardowa Spedycja Paletowa (+140 PLN stałej opłaty paletowej)");
                                Console.WriteLine("2 - Ekspres Kurier Przemysłowy 24h (+5% priorytet, +220 PLN + 1.50 PLN/szt.)");
                                Console.WriteLine("3 - Kontrakt Hurtowy B2B (8% rabatu na towar, darmowy transport od 50 szt.)");
                                Console.Write("Twój wybór: ");
                                string stratChoice = Console.ReadLine()?.Trim() ?? "1";

                                IShippingStrategy strategy = stratChoice switch
                                {
                                    "2" => new ExpressIndustrialStrategy(),
                                    "3" => new BulkContractB2BStrategy(),
                                    _ => new StandardPalletStrategy()
                                };

                                Console.WriteLine("\n" + lcForOutbound.ProcessOutboundDispatch(outboundProd, reqQty, client, strategy, _system.Database));
                            }
                            else Console.WriteLine("Nie znaleziono towaru o podanym kodzie SKU.");
                            break;
                        case "4" when user is QualityAuditor qaForHold:
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU partii do zablokowania (Blokada QA): ");
                            var holdProd = _system.Database.GetBySku(Console.ReadLine() ?? "");
                            if (holdProd != null)
                            {
                                Console.Write("Podaj przyczynę wstrzymania partii (np. wada powłoki, uszkodzenie): ");
                                string reason = Console.ReadLine() ?? "Weryfikacja technologiczna";
                                Console.WriteLine("\n" + qaForHold.PlaceQualityHold(holdProd, reason, _system.Database));
                            }
                            else Console.WriteLine("Nie znaleziono produktu.");
                            break;
                        case "5" when user is QualityAuditor qaForRelease:
                            ViewAllProducts();
                            Console.Write("\nPodaj kod SKU partii do odblokowania (Zwolnienie QA): ");
                            var relProd = _system.Database.GetBySku(Console.ReadLine() ?? "");
                            if (relProd != null)
                            {
                                Console.Write("Uwagi po kontroli dopuszczającej: ");
                                string notes = Console.ReadLine() ?? "Partia zgodna ze specyfikacją jakościową.";
                                Console.WriteLine("\n" + qaForRelease.ReleaseQualityHold(relProd, notes, _system.Database));
                            }
                            else Console.WriteLine("Nie znaleziono produktu.");
                            break;
                        case "01":
                            Console.WriteLine($"Pracownik: {user.Name} [{user.RoleName}]");
                            Console.WriteLine($"Przypisany sektor: {user.AssignedSector}");
                            Console.WriteLine($"Obsłużone przyjęcia dostaw (PZ): {user.Account.ProcessedInboundDeliveries}");
                            Console.WriteLine($"Obsłużone wydania B2B (WZ): {user.Account.ProcessedOutboundOrders}");
                            Console.WriteLine($"Łącznie przeładowane jednostki: {user.Account.TotalUnitsHandled} szt.");
                            Console.WriteLine($"Przeprowadzone inspekcje QA: {user.Account.QualityAuditsPerformed}");
                            Console.WriteLine($"Łączna wartość netto wydanego towaru: {user.Account.TotalDispatchedNetValue:C}");
                            break;
                    }
                }
                catch (QualityHoldException qhEx)
                {
                    Console.WriteLine($"\n[ODMOWA SYSTEMU QA]: {qhEx.Message}");
                }
                catch (InsufficientStockException stockEx)
                {
                    Console.WriteLine($"\n[BRAK MAGAZYNOWY]: {stockEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[BŁĄD]: {ex.Message}");
                }

                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić...");
                Console.ReadKey(intercept: true);
            }
        }

        private void ShowSqlAnalyticsReport()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("RAPORT ANALITYCZNY WYCENY MAGAZYNU (SQL GROUP BY & SUM)");
            PrintMiddleBorder();
            PrintEmptyLine();

            var summaries = _system.Database.GetCategoryAnalytics();
            decimal grandTotalNet = 0m;
            decimal grandTotalGross = 0m;

            foreach (var s in summaries)
            {
                grandTotalNet += s.TotalNetValuation;
                grandTotalGross += s.TotalGrossValuation;

                PrintLeftText($"Kategoria: {s.Category}");
                PrintLeftText($"   Indeksy SKU: {s.DistinctProductsCount} | Łączna ilość: {s.TotalUnitsInStock} szt.");
                PrintLeftText($"   Wartość Netto: {s.TotalNetValuation:C} | Wartość Brutto (23% VAT): {s.TotalGrossValuation:C}");
                PrintEmptyLine();
            }

            PrintMiddleBorder();
            PrintCenteredText($"SUMA CAŁKOWITA NETTO: {grandTotalNet:C} | BRUTTO: {grandTotalGross:C}");
            PrintBottomBorder();
        }

        private void ShowStaffKpiDashboard()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("STATYSTYKI WYDAJNOŚCI OPERACYJNEJ ZESPOŁU (KPI)");
            PrintMiddleBorder();
            PrintEmptyLine();

            foreach (var u in _system.StaffMembers)
            {
                PrintLeftText($"{u.Name} [ID: {u.ID}] - {u.RoleName}");
                PrintLeftText($"   Sektor: {u.AssignedSector}");
                PrintLeftText($"   Dostawy PZ: {u.Account.ProcessedInboundDeliveries} | Wysyłki WZ: {u.Account.ProcessedOutboundOrders} | Audyty QA: {u.Account.QualityAuditsPerformed}");
                PrintLeftText($"   Przeładowane szt.: {u.Account.TotalUnitsHandled} | Obrót WZ Netto: {u.Account.TotalDispatchedNetValue:C}");
                PrintEmptyLine();
            }
            PrintBottomBorder();
        }

        private void ViewAllProducts()
        {
            var products = _system.Database.ListOfProducts;
            if (products.Count == 0)
            {
                Console.WriteLine("Brak zarejestrowanych towarów w bazie WMS.");
                return;
            }

            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTR STANÓW MAGAZYNOWYCH I KART AUDYTOWYCH");
            PrintMiddleBorder();
            PrintEmptyLine();

            foreach (var p in products)
            {
                PrintLeftText($"[{p.Sku}] {p.Name}");
                PrintLeftText($"   Kategoria: {p.Category} | Regał: {p.RackLocation} | Status: {p.Status}");
                PrintLeftText($"   Stan: {p.Quantity} szt. (Próg min.: {p.MinStockThreshold} szt.) | Cena netto: {p.UnitNetPrice:C} | Wartość: {p.TotalNetValue:C}");
                foreach (var log in p.AuditLogs)
                {
                    PrintLeftText($"   > {log}");
                }
                PrintEmptyLine();
            }
            PrintBottomBorder();
        }

        private void ShowSystemLogs()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("DZIENNIK ZDARZEŃ I ALARMÓW MAGAZYNOWYCH (SQL)");
            PrintMiddleBorder();
            PrintEmptyLine();

            var logs = _system.Database.GetRecentLogs(15);
            if (logs.Count == 0) PrintCenteredText("Brak wpisów w dzienniku systemowym.");
            else logs.ForEach(PrintLeftText);

            PrintEmptyLine();
            PrintBottomBorder();
            Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
            Console.ReadKey(intercept: true);
        }

        private static string GetLeftMargin()
        {
            int margin = (Console.WindowWidth - MenuWidth) / 2;
            return new string(' ', Math.Max(0, margin));
        }

        private static void PrintTopBorder() => Console.WriteLine(GetLeftMargin() + "╔" + new string('═', MenuWidth - 2) + "╗");
        private static void PrintMiddleBorder() => Console.WriteLine(GetLeftMargin() + "╠" + new string('═', MenuWidth - 2) + "╣");
        private static void PrintBottomBorder() => Console.WriteLine(GetLeftMargin() + "╚" + new string('═', MenuWidth - 2) + "╝");
        private static void PrintEmptyLine() => Console.WriteLine(GetLeftMargin() + "║" + new string(' ', MenuWidth - 2) + "║");

        private static void PrintCenteredText(string text)
        {
            int totalSpaces = MenuWidth - 2 - text.Length;
            if (totalSpaces < 0)
            {
                text = text.Substring(0, MenuWidth - 5) + "...";
                totalSpaces = 0;
            }
            int leftSpaces = totalSpaces / 2;
            int rightSpaces = totalSpaces - leftSpaces;
            Console.WriteLine(GetLeftMargin() + "║" + new string(' ', leftSpaces) + text + new string(' ', rightSpaces) + "║");
        }

        private static void PrintLeftText(string text)
        {
            text = text.Replace("\t", "    ");
            string prefix = "  ";
            int rightSpaces = MenuWidth - 2 - text.Length - prefix.Length;
            if (rightSpaces < 0)
            {
                text = text.Substring(0, MenuWidth - 5 - prefix.Length) + "...";
                rightSpaces = 0;
            }
            Console.WriteLine(GetLeftMargin() + "║" + prefix + text + new string(' ', rightSpaces) + "║");
        }
    }
}
