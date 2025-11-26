using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using Range = SemanticVersioning.Range;

namespace AKRailExtension;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.norvinskevangelion.akrailextension";
    public override string Name { get; init; } = "AK Rail Extension";
    public override string Author { get; init; } = "NORVINSK_EVANGELION";
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override Range SptVersion { get; init; } = new("~4.0.0");
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = true;
    public override Dictionary<string, Range>? ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new Range("~2.0.0") }
    };
    public override string? Url { get; init; }
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class AKRailExtension(
    ISptLogger<AKRailExtension> logger,
    WTTServerCommonLib.WTTServerCommonLib wttCommon,
    DatabaseService databaseService,
    ItemHelper itemHelper
) : IOnLoad
{
    public static DatabaseService DatabaseService;
    public static Dictionary<MongoId, TemplateItem> TemplateItems;
    
    public async Task OnLoad()
    {
        List<string> addUpper = ["5649af094bdc2df8348b4586", //AK-74 dust cover
                                "59d6507c86f7741b846413a2", //AKM dust cover
                                "59e6449086f7746c9f75e822", //Molot Arms AKM-type dust cover
                                "5ac50da15acfc4001718d287" ]; //AK-74M dust cover (6P34 0-1)
        
        List<string> addAk = ["59d6088586f774275f37482f", //akm
                            "5a0ec13bfcdbcb00165aa685", //akmn
                            "5bf3e03b0db834001d2c4a9c", //ak74
                            "5644bd2b4bdc2d3b4c8b4572", //ak74n
                            "628a60ae6b1d481ff772e9c8"]; //rd-704

        List<string> addAk100 = ["5ac4cd105acfc40016339859", //74m
                                "5ac66cb05acfc40198510a10", //101
                                "5ac66d015acfc400180ae6e4", //102
                                "5ac66d2e5acfc43b321d4b53", //103
                                "5ac66d725acfc43b321d4b60", //104
                                "5ac66d9b5acfc4001633997a", //105
                                "5ab8e9fcd8ce870019439434", //aks74n
                                "583990e32459771419544dd2", //74un
                                "5839a40f24597726f856b511"]; //74ub
        
        DatabaseService = databaseService;
        TemplateItems = databaseService.GetItems();
        
        // Get your current assembly
        var assembly = Assembly.GetExecutingAssembly();
        
        // Use WTT-CommonLib services
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomLocaleService.CreateCustomLocales(assembly);

        foreach (string s in addUpper)
        {
            AddToFilter(s);
        }

        foreach (string s in addAk)
        {
            AddToModRear(s);
            AddToModStock(s);
        }
        
        foreach (string s in addAk100)
        {
            AddToModStockAk100(s);
        }

        TemplateItems["628a60ae6b1d481ff772e9c8"].
            Properties!.Slots!.First().Properties!.Filters!.First().Filter!
            .Add("628a83c29179c324ed2695AB"); // mountsagmk3id

        //if (itemHelper.IsValidItem("5a01ad4786f77450561fda02"))
        //{
            TemplateItems["5a01ad4786f77450561fda02"].Properties!.Slots!.First().Properties!.Filters!.First().Filter!
                .Add("628a83c29179c324ed26955A"); // mountaksagmk3id
            TemplateItems["5a01ad4786f77450561fda02"].Properties!.Slots!.First().Properties!.Filters!.First().Filter!
                .Add("628a83c29179c324ed64955A"); // handguardsagmk2o1stdid
            TemplateItems["5a01ad4786f77450561fda02"].Properties!.Slots!.First().Properties!.Filters!.First().Filter!
                .Add("628a83c29179c324ed64943D"); // handguardsagmk2o1slimid
        //}
        
        TemplateItems["59d36a0086f7747e673f3946"].
            Properties!.Slots!.First().Properties!.Filters!.First().Filter!
            .Add("57ffa9f424597772857Ee844"); // handguardb11id
        
        logger.Success("[AK Rail Extension] Mod loaded successfully.");
        
        await Task.CompletedTask;
    }

    private static void AddToFilter(string itemId)
    {
        TemplateItem templateItem = TemplateItems[itemId];
        
        if (templateItem.Properties == null)
            return;
        
        TemplateItemProperties templateItemProperties = templateItem.Properties;
        
        templateItemProperties.Slots.First().Properties.Filters.First().Filter
            .Add("5c064c400db834001d234f68"); // tula10krailid
    }
    
    private static void AddToModRear(string itemId)
    {
        TemplateItem templateItem = TemplateItems[itemId];
        
        if (templateItem.Properties == null)
            return;
        
        TemplateItemProperties templateItemProperties = templateItem.Properties;

        foreach (Slot slot in templateItemProperties.Slots)
        {
            if (slot.Name == "mod_sight_rear")
                slot.Properties.Filters.First().Filter.Add("5d2c33Cc48f0355d95672c25"); // twsdlberylid
        }
    }

    private static void AddToModStock(string itemId)
    {
        TemplateItem templateItem = TemplateItems[itemId];
        
        if (templateItem.Properties == null)
            return;
        
        TemplateItemProperties templateItemProperties = templateItem.Properties;
        
        foreach (Slot slot in templateItemProperties.Slots)
        {
            if (slot.Name != "mod_stock" &&
                slot.Name != "mod_stock_000")
                continue;
            
            slot.Properties.Filters.First().Filter.Add("5649b2314bd2Fd79388b4576"); // popcakmadtid
        }
    }
    
    private static void AddToModStockAk100 (string itemId)
    {
        TemplateItem templateItem = TemplateItems[itemId];
        
        if (templateItem.Properties == null)
            return;
        
        TemplateItemProperties templateItemProperties = templateItem.Properties;
        
        foreach (Slot slot in templateItemProperties.Slots)
        {
            if (slot.Name != "mod_stock" &&
                slot.Name != "mod_stock_000")
                continue;
            
            slot.Properties.Filters.First().Filter.Add("566Cb2314bdc2d79388b4576"); // popcak100adtid
        }
    }
}