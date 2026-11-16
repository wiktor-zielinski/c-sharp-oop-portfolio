using System.Security.Principal;
using System.Xml.Linq;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Users
{
    // Inspektor Kontroli Jakości (QA) uprawniony do blokowania wadliwych partii oraz prowadzenia audytów
    public class QualityAuditor : User
    {
        public override string RoleName => "Inspektor Kontroli Jakości (QA)";

        public QualityAuditor(int id, string name, string plainPassword, string assignedSector): base(id, name, plainPassword, assignedSector)
        {
        }

        // Nakłada blokadę jakościową (np. uszkodzenia mechaniczne, odpryski lakieru)
        public string PlaceQualityHold(Product product, string reason, IWarehouseDatabase db)
        {
            product.SetQualityHold(true, Name, reason);
            Account.RecordQualityAudit();

            db.Update(product);
            db.TriggerAlert($"BLOKADA QA! Inspektor {Name} wstrzymał obrót towarem SKU {product.Sku} ({product.Name}). Powód: {reason}");

            return $"Nałożono blokadę Kontroli Jakości (ZablokowanyQA) na produkt '{product.Name}' [{product.Sku}].";
        }

        // Zdejmuje blokadę jakościową po pomyślnej weryfikacji lub naprawie partii
        public string ReleaseQualityHold(Product product, string clearanceNote, IWarehouseDatabase db)
        {
            product.SetQualityHold(false, Name, clearanceNote);
            Account.RecordQualityAudit();

            db.Update(product);
            db.AddLog($"[ZWOLNIENIE QA] Inspektor {Name} odblokował partię SKU {product.Sku}. Status: {product.Status}.");

            return $"Zwolniono blokadę QA dla '{product.Name}' [{product.Sku}]. Aktualny status: {product.Status}.";
        }
    }
}