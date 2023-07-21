////////////////////////////////////////////////////////////////
//  Pet Intensity Evaluator - Designed originally for use
//  on UO Alive.
//
//  Any feedback/suggestions, contact mukkel on Discord!
//
//  Instructions:
//  1. Add script to RE under the C# tab.
//  2. Run the script.  It'll ask you for a target to lore.
//     or to close an existing lore window.
//
//  Known Issues:
//  - Bug in RE with Gumps.GetLineList().  Until that's fixed
//    we have to use Gumps.LastGumpGetLineList().
//  - MobileIDs are still a work in progress.  If you find an
//    ID that doesn't match what you lore'd.  Let me know!
//
//  NOTE: I recommend you assign it to a hotkey for ease of use.
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Engine = Assistant.Engine;
using RazorEnhanced;

namespace RazorEnhanced
{
    public class IntensityInspect
    {
        public void Run()
        {
            try {
                Gumps.ResetGump();
                string gumpIDList = string.Join(",", Gumps.AllGumpIDs());
                Handler.SendMessage(MessageType.Debug, $"GumpList: {gumpIDList}");
                if (gumpIDList.Contains("3644314075"))
                {
                    Player.HeadMessage(52, "Please close the Animal Lore gump");
                    return;
                }
                {
                    Player.UseSkill("Animal Lore");
                    Gumps.WaitForGump(3644314075,50000);
                }

                // Assemble the mobile ID based off of the last target.
                var targetID = Mobiles.FindBySerial(Target.GetLast()).Body;
                string hexBreed = "0x" + targetID.ToString("X4"); 

                if (!petDefinitions.ContainsKey(hexBreed))
                {
                    string _message = "Target Animal (" + hexBreed + ") could not be found.  Let mukkel know!";
                    Player.HeadMessage(52, _message);
                    return;
                }

                // Pull in the gump text from the last gump opened.  NOTE: Change this to GetLineList() whenever it gets fixed.
                string gumpTextString = string.Join(",", Gumps.LastGumpGetLineList());
                string gumpTextProcess = gumpTextString.Substring(gumpTextString.IndexOf('<'));

                // Detect whether the target is a pet with training in progress.  Needed since the server response is different.
                // UO Alive: Last <div align=right> is the training progress.
                int startIndex      = gumpTextString.LastIndexOf("<div align=right>") + "<div align=right>".Length;
                int endIndex        = gumpTextString.LastIndexOf("%")+1;
                string capturedText = gumpTextString.Substring(startIndex, endIndex - startIndex);
                bool trainedPet     = capturedText.Contains("%");

                // Remove all the unnecessary text from the string.
                //string regexPattern = @"<[^>]+>|%|";
                string regexPattern = @"(<[^>]+>|%|[a-zA-Z])";
                string outputString = Regex.Replace(gumpTextProcess, regexPattern, "").Replace("---", "0").Replace("-", "/")
                                            .Replace("=>", "/").Replace("~1_~", "").Replace(" / ", "/").Replace(", ", ",")
                                            .Replace(" ,", ",").Replace("  (. ", ",").Replace("),", ",").Replace("&", "");
                outputString = string.Join(",", outputString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                // Populate the dictionary values with the data from the gump.
                var parseDict = Parse(outputString, trainedPet);

                // Define all values / properties for the pet.
                Pet myPet = new Pet(parseDict, rarityColours, petDefinitions, hexBreed);
                
                // Show the GUI.
                CreateMenuInstance(myPet, parseDict, petDefinitions);
            }
            catch (Exception e)
            {
                Player.HeadMessage(52, "Error: Check the log.");
                Handler.SendMessage(MessageType.Debug, e.Message);
                return;
            }
        }

        public static Dictionary<string, string> Parse(string input, bool trainedPet)
        {
            var values = input.Split(',');
            var keys   = new List<string>();

            // Determine whether the pet is trained/tame or not.
            if (trainedPet)
            {
            keys = new List<string> {"strength", "dexterity","intelligence", "hits", "stamina", "mana",  "resistphysical"
                                        , "resistfire", "resistcold", "resistpoison", "resistenergy", "damagephysical"
                                        , "damagefire", "damagecold", "damagepoison", "damageenergy", "wrestling", "tactics"
                                        , "resistingspells", "anatomy", "healing", "poisoning", "detectinghidden", "hiding"
                                        , "parrying", "magery", "evalintelligence","meditation", "necromancy", "spiritspeak"
                                        , "mysticism", "focus", "spellweaving","discordance", "bushido", "ninjitsu", "chivalry"
                                        , "hpregen", "staminaregen", "manaregen", "damage", "bardingdifficulty", "slots", "progress" };
            }
            else
            {
            keys = new List<string> {"strength", "dexterity","intelligence", "hits", "stamina", "mana",  "resistphysical"
                                        , "resistfire", "resistcold", "resistpoison", "resistenergy", "damagephysical"
                                        , "damagefire", "damagecold", "damagepoison", "damageenergy", "wrestling", "tactics"
                                        , "resistingspells", "anatomy", "healing", "poisoning", "detectinghidden", "hiding"
                                        , "parrying", "magery", "evalintelligence","meditation", "necromancy", "spiritspeak"
                                        , "mysticism", "focus", "spellweaving","discordance", "bushido", "ninjitsu", "chivalry"
                                        , "hpregen", "staminaregen", "manaregen", "damage", "bardingdifficulty", "slots" };

            }
            
            var dict = new Dictionary<string, string>();
            

            for (int i = 0; i < keys.Count && i < values.Length; i++)            
            //First value is the hex value of the pet. Skip it.
            {
                dict.Add(keys[i], values[i+1]);
            }

            return dict;
        }

        public static Dictionary<string, string> rarityColours = new Dictionary<string, string>()
        {
            { "Exotic", "#1BF2E7" },
            { "Legendary", "#FF0068" },
            { "Exquisite", "#00C438" },
            { "Rare", "#9B00FE" },
            { "Common", "#808080" }
        };

        public static Dictionary<string, string[]> petDefinitions = new Dictionary<string, string[]>()
        {
            { "Unknown7", new string[] { "Greater Dragon, TRUE,500-1000,1000-2000,40-74,80-148,475-675,475-675,512-700,1025-1400,40-74,80-148,475-675,475-675,60-85,65-90,40-55,40-60,50-75"}},
            { "Unknown6", new string[] { "Frost Drake, FALSE,240-260,,150-150,,301-360,,400-430,,160-182,,301-360,,45-50,40-50,50-60,20-30,30-40"}},
            { "Unknown5", new string[] { "Frost Dragon, TRUE,750-1000,1500-2000,44-71,88-142,490-688,490-688,515-642,1030-1285,44-71,88-142,495-688,495-688,70-90,55-60,80-90,65-70,55-75"}},
            { "Unknown4", new string[] { "Drake, FALSE,241-258,,133-150,,101-140,,401-430,,133-152,,101-140,,45-50,50-60,40-50,20-30,30-40"}},
            { "Unknown3", new string[] { "Nightmare, FALSE,298-315,,86-106,,86-125,,250-525,,86-106,,86-125,,55-65,30-40,30-40,30-40,20-30"}},
            { "Unknown2", new string[] { "Nightmare, FALSE,298-315,,86-106,,86-125,,250-525,,86-106,,86-125,,55-65,30-40,30-40,30-40,20-30"}},
            { "Unknown1", new string[] { "Toothless, ,,,,,,,,,,,,,,,,,"}},
            { "0xA2D8", new string[] { "Triton, FALSE,651-700,,150-150,,101-120,,101-250,,151-220,,101-120,,45-55,50-60,45-55,35-45,85-90"}},
            { "0x3EBD", new string[] { "Bane Dragon, FALSE,551-650,,88-125,,88-165,,495-555,,88-125,,88-165,,60-70,40-50,35-45,50-60,20-40"}},
            { "0x3EA7", new string[] { "Dread Warhorse, FALSE,555-650,,89-125,,100-165,,501-555,,89-125,,100-165,,65-75,20-40,20-40,50-60,40-50"}},
            { "0x3E94", new string[] { "Hiryu, TRUE,450-550,900-1100,85-135,170-270,60-60,60-60,600-705,1200-1410,85-135,170-270,300-325,300-325,55-70,70-90,15-25,40-50,40-50"}},
            { "0x0590", new string[] { "Frost Mite, TRUE,400-500,800-1000,75-85,150-170,250-290,250-290,500-513,1000-1026,75-85,150-170,250-290,250-290,50-65,15-25,85-95,50-65,40-55"}},
            { "0x058C", new string[] { "Crimson Drake, FALSE,241-258,,130-150,,100-140,,400-430,,130-155,,100-140,,30-50,30-50,30-50,30-50,30-50"}},
            { "0x0589", new string[] { "Platinum Drake, FALSE,241-258,,130-150,,100-140,,400-430,,130-155,,100-140,,30-50,30-50,30-50,30-50,30-50"}},
            { "0x0587", new string[] { "Triceratops, TRUE,503-600,1006-1200,75-85,150-170,277-319,277-319,551-658,1102-1316,75-85,150-170,277-319,277-319,60-85,30-45,30-55,35-50,35-55"}},
            { "0x0582", new string[] { "Windrunner, FALSE,240-240,,125-125,,0-0,,400-400,,125-125,,51-55,,40-50,30-40,30-40,30-40,30-40"}},
            { "0x057F", new string[] { "Charlie, ,,,,,,,,,,,,,,,,,"}},
            { "0x0509", new string[] { "Najasaurus, FALSE,737-854,,150-150,,21-40,,162-346,,151-218,,21-40,,45-55,50-60,45-55,100-100,35-45"}},
            { "0x0506", new string[] { "Gallusaurus, FALSE,751-790,,150-150,,144-288,,473-514,,151-170,,144-288,,50-60,20-30,20-30,60-70,20-30"}},
            { "0x0505", new string[] { "Dimetrosaur, TRUE,686-750,5488-6000,82-92,165-185,361-435,361-435,261-307,522-615,82-92,165-185,361-435,361-435,80-90,60-70,60-70,65-75,65-75"}},
            { "0x0340", new string[] { "Phoenix, FALSE,340-380,,150-150,,532-672,,551-650,,220-300,,532-672,,45-55,60-70,0-0,25-35,40-50"}},
            { "0x0317", new string[] { "Giant Beetle, TRUE,100-100,200-200,50-50,100-100,500-500,500-500,150-150,300-300,50-50,100-100,500-500,500-500,30-40,20-30,20-30,20-30,20-30"}},
            { "0x02E1", new string[] { "a spider, FALSE,737-854,,150-150,,21-40,,162-346,,151-218,,21-40,,45-55,50-60,45-55,100-100,35-45"}},
            { "0x02CF", new string[] { "Dragon Wolf, FALSE,776-852,,67-77,,54-77,,785-915,,67-77,,54-77,,45-55,30-40,30-40,40-50,40-50"}},
            { "0x02CB", new string[] { "High Plains Boura, FALSE,574-675,,85-103,,25-30,,370-532,,85-103,,25-30,,55-65,30-40,50-60,40-50,30-40"}},
            { "0x0124", new string[] { "Pack Llama, FALSE,50-50,,86-105,,0-0,,52-80,,36-55,,16-30,,25-35,10-15,10-15,10-15,10-15"}},
            { "0x0123", new string[] { "Pack Horse, FALSE,61-80,,81-100,,0-0,,44-120,,36-55,,6-10,,20-25,10-15,20-25,10-15,10-15"}},
            { "0x0115", new string[] { "Cu Sidhe, TRUE,500-600,1000-1200,75-85,150-170,250-290,250-290,600-612,1200-1225,75-85,150-170,250-290,250-290,50-65,25-45,70-85,30-50,70-85"}},
            { "0x0114", new string[] { "Reptalon, TRUE,406-463,812-926,75-85,150-170,250-290,250-290,500-512,1000-1025,75-85,150-170,250-290,250-290,50-65,35-45,35-45,50-65,70-85"}},
            { "0x00F6", new string[] { "Bake Kitsune, FALSE,301-350,,125-145,,375-425,,170-220,,125-145,,375-425,,40-60,70-90,40-60,40-60,40-60"}},
            { "0x00F4", new string[] { "Rune Beetle, FALSE,305-360,,121-150,,375-450,,400-465,,121-170,,375-450,,40-65,35-50,35-50,75-95,40-60"}},
            { "0x00D5", new string[] { "Polar Bear, FALSE,70-84,,80-105,,30-40,,116-140,,81-105,,26-50,,25-35,0-0,60-80,15-25,10-15"}},
            { "0x00BE", new string[] { "Fire Steed, FALSE,226-240,,91-120,,291-300,,376-400,,91-120,,291-300,,30-40,70-80,20-30,30-40,30-40"}},
            { "0x00B4", new string[] { "White Wyrm, FALSE,433-456,,101-130,,386-425,,359-760,,101-130,,386-425,,55-70,15-25,80-90,40-50,40-50"}},
            { "0x00B3", new string[] { "Nightmare, FALSE,298-315,,86-106,,86-125,,250-525,,86-106,,86-125,,55-65,30-40,30-40,30-40,20-30"}},
            { "0x00B2", new string[] { "Nightmare, FALSE,298-315,,86-106,,86-125,,250-525,,86-106,,86-125,,55-65,30-40,30-40,30-40,20-30"}},
            { "0x00B1", new string[] { "Nightmare, FALSE,298-315,,86-106,,86-125,,250-525,,86-106,,86-125,,55-65,30-40,30-40,30-40,20-30"}},
            { "0x00A9", new string[] { "Fire Beetle, TRUE,100-100,200-200,50-50,100-100,500-500,500-500,150-150,300-300,50-50,100-100,500-500,500-500,40-40,70-75,10-10,30-30,30-30"}},
            { "0x0084", new string[] { "Ki-Rin, FALSE,191-210,,96-115,,186-225,,296-325,,96-115,,186-225,,55-65,35-45,25-35,25-35,25-35"}},
            { "0x007A", new string[] { "Unicorn, FALSE,191-210,,96-115,,186-225,,296-325,,96-115,,186-225,,55-65,25-40,25-40,55-65,25-40"}},
            { "0x0074", new string[] { "Nightmare?, FALSE,298-315,,86-106,,86-125,,250-525,,86-106,,86-125,,55-65,30-40,30-40,30-40,20-30"}},
            { "0x006A", new string[] { "Shadow Wyrm, FALSE,561-600,,44-100,,488-620,,898-1030,,44-100,,488-620,,65-75,50-60,45-55,20-30,50-60"}},
            { "0x0069", new string[] { "Ancient Wyrm? 69, TRUE,500-1000,1000-2000,40-74,80-148,475-675,475-675,512-700,1025-1400,40-74,80-148,475-675,475-675,60-85,65-90,40-55,40-60,50-75"}},
            { "0x0067", new string[] { "Serpentine Dragon, FALSE,480-480,,150-150,,1001-1040,,111-140,,201-220,,1001-1040,,35-40,25-35,25-35,25-35,25-35"}},
            { "0x0062", new string[] { "Hell Hound, FALSE,102-210,,81-105,,36-60,,102-150,,81-105,,36-60,,25-35,30-40,0-0,10-20,10-20"}},
            { "0x0061", new string[] { "Hell Hound, FALSE,102-210,,81-105,,36-60,,102-150,,81-105,,36-60,,25-35,30-40,0-0,10-20,10-20"}},
            { "0x003D", new string[] { "Drake, FALSE,241-258,,133-150,,101-140,,401-430,,133-152,,101-140,,45-50,50-60,40-50,20-30,30-40"}},
            { "0x003C", new string[] { "Drake, FALSE,241-258,,133-150,,101-140,,401-430,,133-152,,101-140,,45-50,50-60,40-50,20-30,30-40"}},
            { "0x003B", new string[] { "Dragon, FALSE,478-495,,86-105,,436-475,,796-825,,86-105,,436-475,,55-65,60-70,30-40,25-35,35-45"}},
            { "0x0031", new string[] { "White Wyrm ? 31, FALSE,433-456,,101-130,,386-425,,359-760,,101-130,,386-425,,55-70,15-25,80-90,40-50,40-50"}},
            { "0x0017", new string[] { "Dire Wolf, FALSE,46-72,,81-105,,36-60,,96-120,,81-105,,36-60,,20-25,10-20,5-10,5-10,10-15"}},
            { "0x0014", new string[] { "Frost Spider? 14, FALSE,46-60,,126-144,,0-0,,76-100,,126-144,,36-60,,25-30,5-10,40-50,20-30,10-20"}},
            { "0x0013", new string[] { "Dread Spider? 13, FALSE,115-147,,146-150,,236-324,,231-279,,146-165,,285-321,,45-55,35-45,35-45,100-100,35-45"}},
            { "0x000C", new string[] { "Dragon? 0C, FALSE,478-495,,86-105,,436-475,,796-825,,86-105,,436-475,,55-65,60-70,30-40,25-35,35-45"}},
            { "0x000B", new string[] { "Dread Spider, FALSE,115-147,,146-150,,236-324,,231-279,,146-165,,285-321,,45-55,35-45,35-45,100-100,35-45"}},
            { "0x00F3", new string[] { "Lesser Hiryu, TRUE,200-300,400-600,85-135,170-270,60-60,60-60,150-205,300-410,85-135,170-270,300-325,300-325,45-70,60-80,5-15,30-40,30-40"}},
        };

        public void CreateMenuInstance(Pet myPet, Dictionary<string, string> parseDict, Dictionary<string, string[]> petDict)
        {
            var gump = Gumps.CreateGump(true, true, true, true);
            gump.gumpId = 879941;
            gump.serial = (uint)Player.Serial;

            Gumps.AddPage(ref gump, 0);
            Gumps.AddBackground(ref gump, 0, 0, 530, 497, 5054);
            
            //////////// TITLE //////////////
            Gumps.AddImageTiled(ref gump, 10, 10, 490, 25, 2624);
        
            //////////// INTENSITY RATING //////////////
            Gumps.AddImageTiled(ref gump, 10, 40, 510, 25, 2624);
            Gumps.AddImageTiled(ref gump, 10, 70, 510, 120, 2624);
            
            //////////// RESISTANCES //////////////
            Gumps.AddImageTiled(ref gump, 10, 195, 250, 25, 2624);
            Gumps.AddImageTiled(ref gump, 270, 195, 250, 25, 2624);
            
            //////////// SKILLS //////////////
            Gumps.AddImageTiled(ref gump, 10, 222, 250, 130, 2624);
            Gumps.AddImageTiled(ref gump, 270, 222, 250, 130, 2624);
            
             //////////// SKILLS //////////////
            Gumps.AddImageTiled(ref gump, 10, 357, 510, 25, 2624);
            Gumps.AddImageTiled(ref gump, 10, 385, 510, 100, 2624);

            //////////// TITLE //////////////
            Gumps.AddAlphaRegion(ref gump, 10, 10, 480, 477);
            Gumps.AddAlphaRegion(ref gump, 490, 34, 30, 453);

            //////////// TITLE //////////////
            Gumps.AddHtml(ref gump, 10, 12, 512, 20, $"<CENTER><BASEFONT COLOR={myPet.RarityColour}>{myPet.Name} ({myPet.Breed})</BASEFONT></CENTER>", false, false);    
            Gumps.AddButton(ref gump, 490, 10, 0xFB1, 0xFB2, 1, 0, 0 );

            //////////// INTENSITY RATING //////////////
            Gumps.AddHtml(ref gump, 10, 43, 512, 20, $"<CENTER><BASEFONT COLOR=#42a5ff>INTENSITY SUMMARY</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 15, 70, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Stats :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 140, 70, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.TotalStats} ({myPet.TotalStatsValue})</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 15, 93, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Hit Points :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 140, 93, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.TotalHits} ({myPet.TotalHitsValue})</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 15, 116, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Resistances :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 140, 116, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.TotalResists} ({myPet.TotalResistsValue})</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 15, 139, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Skills :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 140, 139, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.TotalSkills} ({myPet.TotalSkillsValue})</BASEFONT></LEFT>", false, false);

            Gumps.AddHtml(ref gump, 15, 169, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Total :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 140, 169, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.TotalAll} ({myPet.TotalAllValue})</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 242, 70, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Weight :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 367, 70, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.StatsWeight} (5%)</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 242, 93, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Weight :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 367, 93, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.HitsWeight} (25%)</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 242, 116, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Weight :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 367, 116, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.ResistsWeight} (65%)</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 242, 139, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Weight :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 367, 139, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.SkillsWeight} (5%)</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 242, 169, 125, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Total :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 367, 169, 125, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.TotalWeight} (100%)</BASEFONT></LEFT>", false, false);
            //////////// STATS & ATTRIBUTES //////////////
            Gumps.AddHtml(ref gump, 10, 198, 250, 20, $"<CENTER><BASEFONT COLOR=#42a5ff>STATS & ATTRIBUTES</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 10, 226, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Strength :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 226, 100, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.Strength}</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 10, 246, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Dexterity :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 246, 100, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.Dexterity}</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 10, 266, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Intelligence :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 266, 100, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.Intelligence}</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 10, 286, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Hit Points :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 286, 100, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.HitsMin} / {myPet.HitsMax}</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 10, 306, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Stamina :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 306, 100, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.StaminaMin} / {myPet.StaminaMax}</BASEFONT></LEFT>", false, false);
            Gumps.AddHtml(ref gump, 10, 326, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Mana :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 326, 100, 20, $"<LEFT><BASEFONT COLOR=#ffffff>{myPet.ManaMin} / {myPet.ManaMax}</BASEFONT></LEFT>", false, false);

            //////////// RESISTANCES //////////////
            Gumps.AddHtml(ref gump, 270, 198, 250, 20, $"<CENTER><BASEFONT COLOR=#42a5ff>RESISTANCES</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 226, 100, 20, $"<CENTER><BASEFONT COLOR=#B2B2B2>Physical :</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 246, 100, 20, $"<CENTER><BASEFONT COLOR=#FF0000>Fire :</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 266, 100, 20, $"<CENTER><BASEFONT COLOR=#20C3d0>Cold :</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 286, 100, 20, $"<CENTER><BASEFONT COLOR=#008000>Poison :</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 306, 100, 20, $"<CENTER><BASEFONT COLOR=#BF80FF>Energy :</BASEFONT></CENTER>", false, false);

            Gumps.AddHtml(ref gump, 370, 226, 100, 20, $"<CENTER><BASEFONT COLOR=#B2B2B2>{myPet.ResistPhysical}% ({myPet.ResistPhysicalMaxRange}%)</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 370, 246, 100, 20, $"<CENTER><BASEFONT COLOR=#FF0000>{myPet.ResistFire}% ({myPet.ResistFireMaxRange}%)</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 370, 266, 100, 20, $"<CENTER><BASEFONT COLOR=#20C3d0>{myPet.ResistCold}% ({myPet.ResistColdMaxRange}%)</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 370, 286, 100, 20, $"<CENTER><BASEFONT COLOR=#008000>{myPet.ResistPoison}% ({myPet.ResistPoisonMaxRange}%)</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 370, 306, 100, 20, $"<CENTER><BASEFONT COLOR=#BF80FF>{myPet.ResistEnergy}% ({myPet.ResistEnergyMaxRange}%)</BASEFONT></CENTER>", false, false);
            
             //////////// DAMAGE //////////////
            /*Gumps.AddHtml(ref gump, 10, 400, 512, 20, $"<CENTER><BASEFONT COLOR=#42a5ff>DAMAGE</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 24, 430, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Physical</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 122, 430, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Fire</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 220, 430, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Cold</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 318, 430, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Poison</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 416, 430, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Energy</BASEFONT></CENTER>", false, false);               
            Gumps.AddHtml(ref gump, 24, 462, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{(myPet.DamagePhysical == "---" ? "---" : $"{myPet.DamagePhysical}%")}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 122, 462, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{(myPet.DamageFire == "---" ? "---" : $"{myPet.DamageFire}%")}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 220, 462, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{(myPet.DamageCold == "---" ? "---" : $"{myPet.DamageCold}%")}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 318, 462, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{(myPet.DamagePoison == "---" ? "---" : $"{myPet.DamagePoison}%")}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 416, 462, 88, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{(myPet.DamageEnergy == "---" ? "---" : $"{myPet.DamageEnergy}%")}</BASEFONT></CENTER>", false, false);
*/
             //////////// SKILLS //////////////
            Gumps.AddHtml(ref gump, 10, 360, 500, 20, $"<CENTER><BASEFONT COLOR=#42a5ff>SKILLS</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 10, 391, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Wrestling : </BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 391, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.WrestlingMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 10, 411, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Tactics :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 411, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.TacticsMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 10, 431, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Anatomy :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 431, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.AnatomyMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 10, 451, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Resist Spells :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 110, 451, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.ResistingSpellsMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 391, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Magery :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 370, 391, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.MageryMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 411, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Eval Int. :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 370, 411, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.EvalIntelligenceMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 431, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Meditation :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 370, 431, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.MeditationMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 270, 451, 100, 20, $"<RIGHT><BASEFONT COLOR=#ffffff>Poisoning :</BASEFONT></RIGHT>", false, false);
            Gumps.AddHtml(ref gump, 370, 451, 100, 20, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.PoisoningMin}</BASEFONT></CENTER>", false, false);

            /*Gumps.AddHtml(ref gump, 142, 552, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Focus</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 229, 552, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.FocusMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 269, 552, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Spirit Speak</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 356, 552, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.SpiritSpeakMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 396, 552, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Spellweaving</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 483, 552, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.SpellweavingMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 142, 582, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Healing</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 229, 582, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.HealingMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 269, 582, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Chivalry</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 356, 582, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.ChivalryMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 396, 582, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Discordance</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 483, 582, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.DiscordanceMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 269, 612, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Bushido</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 356, 612, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.BushidoMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 15, 642, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Parrying</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 102, 642, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.ParryingMin}</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 269, 642, 85, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Ninjitsu</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 356, 642, 35, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.NinjitsuMin}</BASEFONT></CENTER>", false, false);
/*
             //////////// MISC //////////////
            Gumps.AddHtml(ref gump, 10, 672, 512, 20, $"<CENTER><BASEFONT COLOR=#42a5ff>MISC</BASEFONT></CENTER>", false, false);

            Gumps.AddHtml(ref gump, 20, 702, 100, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Base Damage</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 125, 702, 60, 25, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.Damage}</BASEFONT></CENTER>", false, false);

            Gumps.AddHtml(ref gump, 194, 702, 100, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Barding Diff.</BASEFONT></CENTER>", false, false);
            Gumps.AddHtml(ref gump, 299, 702, 35, 40, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.BardingDifficulty}</BASEFONT></CENTER>", false, false);*/

            //Gumps.AddHtml(ref gump, 350, 702, 100, 25, $"<CENTER><BASEFONT COLOR=#ffffff>Slots</BASEFONT></CENTER>", false, false);
            //Gumps.AddHtml(ref gump, 455, 702, 55, 40, $"<CENTER><BASEFONT COLOR=#ffffff>{myPet.SlotsMin} / {myPet.SlotsMax}</BASEFONT></CENTER>", false, false);

            Gumps.SendGump(gump.gumpId, gump.serial, 0, 0, gump.gumpDefinition, gump.gumpStrings);
        }
    }
    
    public class Pet
    {
        //////////////////////
        /////  parseDict /////
        //////////////////////
        public string Name { get; set; }
        /////  Stats & Attributes /////
        public string HitsMin { get; set; } 
        public string HitsMax { get; set; } 
        public string StaminaMin { get; set; } 
        public string StaminaMax { get; set; } 
        public string ManaMin { get; set; } 
        public string ManaMax { get; set; }        
        public string Strength { get; set; } 
        public string Dexterity { get; set; } 
        public string Intelligence { get; set; } 
        /////  Regen /////
        public string HpRegen { get; set; } 
        public string StaminaRegen { get; set; } 
        public string ManaRegen { get; set; } 
        /////  Resists /////
        public string ResistPhysical { get; set; } 
        public string ResistFire { get; set; } 
        public string ResistCold { get; set; } 
        public string ResistPoison { get; set; } 
        public string ResistEnergy { get; set; } 
        /////  Damage /////
        public string DamagePhysical { get; set; } 
        public string DamageFire { get; set; } 
        public string DamageCold { get; set; } 
        public string DamagePoison { get; set; } 
        public string DamageEnergy { get; set; } 
        /////  Skills /////
        public string WrestlingMin { get; set; } 
        public string TacticsMin { get; set; } 
        public string ResistingSpellsMin { get; set; } 
        public string AnatomyMin { get; set; } 
        public string HealingMin { get; set; } 
        public string PoisoningMin { get; set; } 
        public string DetectingHiddenMin { get; set; } 
        public string HidingMin { get; set; } 
        public string ParryingMin { get; set; } 
        public string MageryMin { get; set; } 
        public string EvalIntelligenceMin { get; set; }
        public string MeditationMin { get; set; } 
        public string NecromancyMin { get; set; } 
        public string SpiritSpeakMin { get; set; } 
        public string MysticismMin { get; set; } 
        public string FocusMin { get; set; } 
        public string SpellweavingMin { get; set; } 
        public string DiscordanceMin { get; set; } 
        public string BushidoMin { get; set; } 
        public string NinjitsuMin { get; set; } 
        public string ChivalryMin { get; set; }

        public string WrestlingMax { get; set; } 
        public string TacticsMax { get; set; } 
        public string ResistingSpellsMax { get; set; } 
        public string AnatomyMax { get; set; } 
        public string HealingMax { get; set; } 
        public string PoisoningMax { get; set; } 
        public string DetectingHiddenMax { get; set; } 
        public string HidingMax { get; set; } 
        public string ParryingMax { get; set; } 
        public string MageryMax { get; set; } 
        public string EvalIntelligenceMax { get; set; }
        public string MeditationMax { get; set; } 
        public string NecromancyMax { get; set; } 
        public string SpiritSpeakMax { get; set; } 
        public string MysticismMax { get; set; } 
        public string FocusMax { get; set; }
        public string SpellweavingMax { get; set; }
        public string DiscordanceMax { get; set; }
        public string BushidoMax { get; set; }
        public string NinjitsuMax { get; set; }
        public string ChivalryMax { get; set; }
        /////  Misc /////
        public string BardingDifficulty { get; set; } 
        public string SlotsMin { get; set; }
        public string SlotsMax { get; set; }
        public string Damage { get; set; }
        public string Rarity { get; set; }
        public string RarityColour { get; set; }
        public string Status { get; set; }
        public string StatsWeight { get; set; }
        public string HitsWeight { get; set; }
        public string ResistsWeight { get; set; }
        public string SkillsWeight { get; set; }
        public string TotalWeight { get; set; }

        ////////////////////
        /////  petDict /////
        ////////////////////
        public string PetID { get; set; } 
        public string Breed { get; set; } 
        public bool Halved { get; set; } 
        public string HitsTamed { get; set; } 
        public string HitsWild { get; set; } 
        public string StaminaTamed { get; set; } 
        public string StaminaWild { get; set; } 
        public string ManaTamed { get; set; } 
        public string ManaWild { get; set; } 
        public string StrengthTamed { get; set; } 
        public string StrengthWild { get; set; }
        public string DexterityTamed { get; set; } 
        public string DexterityWild { get; set; } 
        public string IntelligenceTamed { get; set; } 
        public string IntelligenceWild { get; set; } 
        public string ResistPhysicalRange { get; set; } 
        public string ResistFireRange { get; set; } 
        public string ResistColdRange { get; set; } 
        public string ResistPoisonRange { get; set; } 
        public string ResistEnergyRange { get; set; } 

        public string HitsTamedMin { get; set; } 
        public string HitsWildMin { get; set; } 
        public string StaminaTamedMin { get; set; } 
        public string StaminaWildMin { get; set; } 
        public string ManaTamedMin { get; set; } 
        public string ManaWildMin { get; set; } 
        public string StrengthTamedMin { get; set; } 
        public string StrengthWildMin { get; set; }
        public string DexterityTamedMin { get; set; } 
        public string DexterityWildMin { get; set; } 
        public string IntelligenceTamedMin { get; set; } 
        public string IntelligenceWildMin { get; set; } 

        public string HitsTamedMax { get; set; } 
        public string HitsWildMax { get; set; } 
        public string StaminaTamedMax { get; set; } 
        public string StaminaWildMax { get; set; } 
        public string ManaTamedMax { get; set; } 
        public string ManaWildMax { get; set; } 
        public string StrengthTamedMax { get; set; } 
        public string StrengthWildMax { get; set; }
        public string DexterityTamedMax { get; set; } 
        public string DexterityWildMax { get; set; } 
        public string IntelligenceTamedMax { get; set; } 
        public string IntelligenceWildMax { get; set; } 

        public string ResistPhysicalMinRange { get; set; } 
        public string ResistFireMinRange { get; set; } 
        public string ResistColdMinRange { get; set; } 
        public string ResistPoisonMinRange { get; set; } 
        public string ResistEnergyMinRange { get; set; } 
        public string ResistPhysicalMaxRange { get; set; } 
        public string ResistFireMaxRange { get; set; } 
        public string ResistColdMaxRange { get; set; } 
        public string ResistPoisonMaxRange { get; set; } 
        public string ResistEnergyMaxRange { get; set; } 

        /////  Synthetic /////
        public string TotalStats { get; set; }
        public string TotalStatsValue { get; set; }
        public string TotalHits { get; set; }
        public string TotalHitsValue { get; set; }
        public string TotalResists { get; set; }
        public string TotalResistsValue { get; set; }
        public string TotalSkills { get; set; }
        public string TotalSkillsValue { get; set; }
        public string TotalAll { get; set; }
        public string TotalAllValue { get; set; }

        public Pet(Dictionary<string, string> parseDict, Dictionary<string, string> rarityColours, Dictionary<string, string[]> petDict, string hexBreed)
        {
            //if (parseDict.TryGetValue("name", out string name))
            //    Name = name;
            if (parseDict.TryGetValue("hits", out string hits))
            {
                var hitsArray = hits.Split('/');
                if (hitsArray.Length == 2)
                {
                    if (int.TryParse(hitsArray[0], out int minHits))
                    {
                        HitsMin = minHits.ToString();
                    }
                    if (int.TryParse(hitsArray[1], out int maxHits))
                    {
                        HitsMax = maxHits.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("stamina", out string stamina))
            {
                var staminaArray = stamina.Split('/');
                if (staminaArray.Length == 2)
                {
                    if (int.TryParse(staminaArray[0], out int minStamina))
                    {
                        StaminaMin = minStamina.ToString();
                    }
                    if (int.TryParse(staminaArray[1], out int maxStamina))
                    {
                        StaminaMax = maxStamina.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("mana", out string mana))
            {
                var manaArray = mana.Split('/');
                if (manaArray.Length == 2)
                {
                    if (int.TryParse(manaArray[0], out int minMana))
                    {
                        ManaMin = minMana.ToString();
                    }
                    if (int.TryParse(manaArray[1], out int maxMana))
                    {
                        ManaMax = maxMana.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("strength", out string strength))
                Strength = strength;

            if (parseDict.TryGetValue("dexterity", out string dexterity))
                Dexterity = dexterity;

            if (parseDict.TryGetValue("intelligence", out string intelligence))
                Intelligence = intelligence;

            if (parseDict.TryGetValue("bardingdifficulty", out string bardingDifficulty))
                BardingDifficulty = bardingDifficulty;

            if (parseDict.TryGetValue("hpregen", out string hpRegen))
                HpRegen = hpRegen;

            if (parseDict.TryGetValue("staminaregen", out string staminaRegen))
                StaminaRegen = staminaRegen;

            if (parseDict.TryGetValue("manaregen", out string manaRegen))
                ManaRegen = manaRegen;

            if (parseDict.TryGetValue("resistphysical", out string resistPhysical))
                ResistPhysical = resistPhysical;

            if (parseDict.TryGetValue("resistfire", out string resistFire))
                ResistFire = resistFire;

            if (parseDict.TryGetValue("resistcold", out string resistCold))
                ResistCold = resistCold;

            if (parseDict.TryGetValue("resistpoison", out string resistPoison))
                ResistPoison = resistPoison;

            if (parseDict.TryGetValue("resistenergy", out string resistEnergy))
                ResistEnergy = resistEnergy;

            if (parseDict.TryGetValue("damagephysical", out string damagePhysical))
                DamagePhysical = damagePhysical.Equals("0") ? "---" : int.TryParse(damagePhysical, out int physicalDamage) ? physicalDamage.ToString() : damagePhysical.Split('/')[1];

            if (parseDict.TryGetValue("damagefire", out string damageFire))
                DamageFire = damageFire.Equals("0") ? "---" : int.TryParse(damageFire, out int fireDamage) ? fireDamage.ToString() : damageFire.Split('/')[1];

            if (parseDict.TryGetValue("damagecold", out string damageCold))
                DamageCold = damageCold.Equals("0") ? "---" : int.TryParse(damageCold, out int coldDamage) ? coldDamage.ToString() : damageCold.Split('/')[1];

            if (parseDict.TryGetValue("damagepoison", out string damagePoison))
                DamagePoison = damagePoison.Equals("0") ? "---" : int.TryParse(damagePoison, out int poisonDamage) ? poisonDamage.ToString() : damagePoison.Split('/')[1];
            
            if (parseDict.TryGetValue("damageenergy", out string damageEnergy))
                DamageEnergy = damageEnergy.Equals("0") ? "---" : int.TryParse(damageEnergy, out int energyDamage) ? energyDamage.ToString() : damageEnergy.Split('/')[1];

            if (parseDict.TryGetValue("damage", out string damage))
                Damage = damage.Equals("0") ? "---" : damage.Substring(0, damage.IndexOf('/')) + " - " + damage.Substring(damage.IndexOf("/") + 1);

            if (parseDict.TryGetValue("wrestling", out string wrestling))
            {
                var wrestlingArray = wrestling.Split('/');
                if (wrestling == "0")
                {
                    WrestlingMin = "---";
                    WrestlingMax = "---";
                }
                else
                {
                    if (double.TryParse(wrestlingArray[0], out double minWrestling))
                    {
                        WrestlingMin = minWrestling.ToString("0.0");
                    }
                    if (int.TryParse(wrestlingArray[1], out int maxWrestling))
                    {
                        WrestlingMax = maxWrestling.ToString();
                    }
                }
            }
            
            if (parseDict.TryGetValue("tactics", out string tactics))
            {
                var tacticsArray = tactics.Split('/');
                if (tactics == "0")
                {
                    TacticsMin = "---";
                    TacticsMax = "---";
                }
                else
                {
                    if (double.TryParse(tacticsArray[0], out double minTactics))
                    {
                        TacticsMin = minTactics.ToString("0.0");
                    }
                    if (int.TryParse(tacticsArray[1], out int maxTactics))
                    {
                        TacticsMax = maxTactics.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("resistingspells", out string resistingspells))
            {
                var resistingspellsArray = resistingspells.Split('/');
                if (resistingspells == "0")
                {
                    ResistingSpellsMin = "---";
                    ResistingSpellsMax = "---";
                }
                else
                {
                    if (double.TryParse(resistingspellsArray[0], out double minResistingSpells))
                    {
                        ResistingSpellsMin = minResistingSpells.ToString("0.0");
                    }
                    if (int.TryParse(resistingspellsArray[1], out int maxResistingSpells))
                    {
                        ResistingSpellsMax = maxResistingSpells.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("anatomy", out string anatomy))
            {
                var anatomyArray = anatomy.Split('/');
                if (anatomy == "0")
                {
                    AnatomyMin = "---";
                    AnatomyMax = "---";
                }
                else
                {
                    if (double.TryParse(anatomyArray[0], out double minAnatomy))
                    {
                        AnatomyMin = minAnatomy.ToString("0.0");
                    }
                    if (int.TryParse(anatomyArray[1], out int maxAnatomy))
                    {
                        AnatomyMax = maxAnatomy.ToString();
                    }
                }
            }
            /*if (parseDict.TryGetValue("healing", out string healing))
            {
                var healingArray = healing.Split('/');
                if (healing == "0")
                {
                    HealingMin = "---";
                    HealingMax = "---";
                }
                else
                {
                    if (double.TryParse(healingArray[0], out double minHealing))
                    {
                        HealingMin = minHealing.ToString("0.0");
                    }
                    if (int.TryParse(healingArray[1], out int maxHealing))
                    {
                        HealingMax = maxHealing.ToString();
                    }
                }
            }*/

            if (parseDict.TryGetValue("poisoning", out string poisoning))
            {
                var poisoningArray = poisoning.Split('/');
                if (poisoning == "0")
                {
                    PoisoningMin = "---";
                    PoisoningMax = "---";
                }
                else
                {
                    if (double.TryParse(poisoningArray[0], out double minPoisoning))
                    {
                        PoisoningMin = minPoisoning.ToString("0.0");
                    }
                    if (int.TryParse(poisoningArray[1], out int maxPoisoning))
                    {
                        PoisoningMax = maxPoisoning.ToString();
                    }
                }
            }

            /*if (parseDict.TryGetValue("detectinghidden", out string detectinghidden))
            {
                var detectinghiddenArray = detectinghidden.Split('/');
                if (detectinghidden == "0")
                {
                    DetectingHiddenMin = "---";
                    DetectingHiddenMax = "---";
                }
                else
                {
                    if (double.TryParse(detectinghiddenArray[0], out double minDetectingHidden))
                    {
                        DetectingHiddenMin = minDetectingHidden.ToString("0.0");
                    }
                    if (int.TryParse(detectinghiddenArray[1], out int maxDetectingHidden))
                    {
                        DetectingHiddenMax = maxDetectingHidden.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("hiding", out string hiding))
            {
                var hidingArray = hiding.Split('/');
                if (hiding == "0")
                {
                    HidingMin = "---";
                    HidingMax = "---";
                }
                else
                {
                    if (double.TryParse(hidingArray[0], out double minHiding))
                    {
                        HidingMin = minHiding.ToString("0.0");
                    }
                    if (int.TryParse(hidingArray[1], out int maxHiding))
                    {
                        HidingMax = maxHiding.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("parrying", out string parrying))
            {
                var parryingArray = parrying.Split('/');
                if (parrying == "0")
                {
                    ParryingMin = "---";
                    ParryingMax = "---";
                }
                else
                {
                    if (double.TryParse(parryingArray[0], out double minParrying))
                    {
                        ParryingMin = minParrying.ToString("0.0");
                    }
                    if (int.TryParse(parryingArray[1], out int maxParrying))
                    {
                        ParryingMax = maxParrying.ToString();
                    }
                }
            }*/

            if (parseDict.TryGetValue("magery", out string magery))
            {
                var mageryArray = magery.Split('/');
                if (magery == "0")
                {
                    MageryMin = "---";
                    MageryMax = "---";
                }
                else
                {
                    if (double.TryParse(mageryArray[0], out double minMagery))
                    {
                        MageryMin = minMagery.ToString("0.0");
                    }
                    if (int.TryParse(mageryArray[1], out int maxMagery))
                    {
                        MageryMax = maxMagery.ToString();
                    }
                }

            }

            if (parseDict.TryGetValue("evalintelligence", out string evalintelligence))
            {
                var evalintelligenceArray = evalintelligence.Split('/');
                if (evalintelligence == "0")
                {
                    EvalIntelligenceMin = "---";
                    EvalIntelligenceMax = "---";
                }
                else
                {
                    if (double.TryParse(evalintelligenceArray[0], out double minEvalIntelligence))
                    {
                        EvalIntelligenceMin = minEvalIntelligence.ToString("0.0");
                    }
                    if (int.TryParse(evalintelligenceArray[1], out int maxEvalIntelligence))
                    {
                        EvalIntelligenceMax = maxEvalIntelligence.ToString();
                    }
                }

            }

            if (parseDict.TryGetValue("meditation", out string meditation))
            {
                var meditationArray = meditation.Split('/');
                if (meditation == "0")
                {
                    MeditationMin = "---";
                    MeditationMax = "---";
                }
                else
                {
                    if (double.TryParse(meditationArray[0], out double minMeditation))
                    {
                        MeditationMin = minMeditation.ToString("0.0");
                    }
                    if (int.TryParse(meditationArray[1], out int maxMeditation))
                    {
                        MeditationMax = maxMeditation.ToString();
                    }
                }
            }

            /*if (parseDict.TryGetValue("necromancy", out string necromancy))
            {
                var necromancyArray = necromancy.Split('/');
                if (necromancy == "0")
                {
                    NecromancyMin = "---";
                    NecromancyMax = "---";
                }
                else
                {
                    if (double.TryParse(necromancyArray[0], out double minNecromancy))
                    {
                        NecromancyMin = minNecromancy.ToString("0.0");
                    }
                    if (int.TryParse(necromancyArray[1], out int maxNecromancy))
                    {
                        NecromancyMax = maxNecromancy.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("spiritspeak", out string spiritspeak))
            {
                var spiritspeakArray = spiritspeak.Split('/');
                if (spiritspeak == "0")
                {
                    SpiritSpeakMin = "---";
                    SpiritSpeakMax = "---";
                }
                else
                {
                    if (double.TryParse(spiritspeakArray[0], out double minSpiritSpeak))
                    {
                        SpiritSpeakMin = minSpiritSpeak.ToString("0.0");
                    }
                    if (int.TryParse(spiritspeakArray[1], out int maxSpiritSpeak))
                    {
                        SpiritSpeakMax = maxSpiritSpeak.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("mysticism", out string mysticism))
            {
                var mysticismArray = mysticism.Split('/');
                if (mysticism == "0")
                {
                    MysticismMin = "---";
                    MysticismMax = "---";
                }
                else
                {
                    if (double.TryParse(mysticismArray[0], out double minMysticism))
                    {
                        MysticismMin = minMysticism.ToString("0.0");
                    }
                    if (int.TryParse(mysticismArray[1], out int maxMysticism))
                    {
                        MysticismMax = maxMysticism.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("focus", out string focus))
            {
                var focusArray = focus.Split('/');
                if (focus == "0")
                {
                    FocusMin = "---";
                    FocusMax = "---";
                }
                else
                {
                    if (double.TryParse(focusArray[0], out double minFocus))
                    {
                        FocusMin = minFocus.ToString("0.0");
                    }
                    if (int.TryParse(focusArray[1], out int maxFocus))
                    {
                        FocusMax = maxFocus.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("spellweaving", out string spellweaving))
            {
                var spellweavingArray = spellweaving.Split('/');
                if (spellweaving == "0")
                {
                    SpellweavingMin = "---";
                    SpellweavingMax = "---";
                }
                else
                {
                    if (double.TryParse(spellweavingArray[0], out double minSpellweaving))
                    {
                        SpellweavingMin = minSpellweaving.ToString("0.0");
                    }
                    if (int.TryParse(spellweavingArray[1], out int maxSpellweaving))
                    {
                        SpellweavingMax = maxSpellweaving.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("discordance", out string discordance))
            {
                var discordanceArray = discordance.Split('/');
                if (discordance == "0")
                {
                    DiscordanceMin = "---";
                    DiscordanceMax = "---";
                }
                else
                {
                    if (double.TryParse(discordanceArray[0], out double minDiscordance))
                    {
                        DiscordanceMin = minDiscordance.ToString("0.0");
                    }
                    if (int.TryParse(discordanceArray[1], out int maxDiscordance))
                    {
                        DiscordanceMax = maxDiscordance.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("bushido", out string bushido))
            {
                var bushidoArray = bushido.Split('/');
                if (bushido == "0")
                {
                    BushidoMin = "---";
                    BushidoMax = "---";
                }
                else
                {
                    if (double.TryParse(bushidoArray[0], out double minBushido))
                    {
                        BushidoMin = minBushido.ToString("0.0");
                    }
                    if (int.TryParse(bushidoArray[1], out int maxBushido))
                    {
                        BushidoMax = maxBushido.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("ninjitsu", out string ninjitsu))
            {
                var ninjitsuArray = ninjitsu.Split('/');
                if (ninjitsu == "0")
                {
                    NinjitsuMin = "---";
                    NinjitsuMax = "---";
                }
                else
                {
                    if (double.TryParse(ninjitsuArray[0], out double minNinjitsu))
                    {
                        NinjitsuMin = minNinjitsu.ToString("0.0");
                    }
                    if (int.TryParse(ninjitsuArray[1], out int maxNinjitsu))
                    {
                        NinjitsuMax = maxNinjitsu.ToString();
                    }
                }
            }

            if (parseDict.TryGetValue("chivalry", out string chivalry))
            {
                var chivalryArray = chivalry.Split('/');
                if (chivalry == "0")
                {
                    ChivalryMin = "---";
                    ChivalryMax = "---";
                }
                else
                {
                    if (double.TryParse(chivalryArray[0], out double minChivalry))
                    {
                        ChivalryMin = minChivalry.ToString("0.0");
                    }
                    if (int.TryParse(chivalryArray[1], out int maxChivalry))
                    {
                        ChivalryMax = maxChivalry.ToString();
                    }
                }
            }*/
            
            if (parseDict.TryGetValue("slots", out string slots))
            {
                var slotsArray = slots.Split('/');
                if (slotsArray.Length == 2)
                {
                    if (int.TryParse(slotsArray[0], out int minSlots))
                    {
                        SlotsMin = minSlots.ToString();
                    }
                    if (int.TryParse(slotsArray[1], out int maxSlots))
                    {
                        SlotsMax = maxSlots.ToString();
                    }
                }
            }

            if (petDict.TryGetValue(hexBreed, out string[] values))
            {
                string[] tokens = values[0].Split(',');
                Breed = tokens[0];
                Halved = bool.Parse(tokens[1]);
                HitsTamed = tokens[2];
                HitsTamedMin = HitsTamed == "" ? "" : HitsTamed.Split('-')[0];
                HitsTamedMax = HitsTamed == "" ? "" : HitsTamed.Split('-')[1];
                HitsWild = tokens[3];
                HitsWildMin = HitsWild == "" ? "" : HitsWild.Split('-')[0];
                HitsWildMax = HitsWild == "" ? "" : HitsWild.Split('-')[1];
                StaminaTamed = tokens[4];
                StaminaTamedMin = StaminaTamed == "" ? "" : StaminaTamed.Split('-')[0];
                StaminaTamedMax = StaminaTamed == "" ? "" : StaminaTamed.Split('-')[1];
                StaminaWild = tokens[5];
                StaminaWildMin = StaminaWild == "" ? "" : StaminaWild.Split('-')[0];
                StaminaWildMax = StaminaWild == "" ? "" : StaminaWild.Split('-')[1];
                ManaTamed = tokens[6];
                ManaTamedMin = ManaTamed == "" ? "" : ManaTamed.Split('-')[0];
                ManaTamedMax = ManaTamed == "" ? "" : ManaTamed.Split('-')[1];
                ManaWild = tokens[7];
                ManaWildMin = ManaWild == "" ? "" : ManaWild.Split('-')[0];
                ManaWildMax = ManaWild == "" ? "" : ManaWild.Split('-')[1];
                StrengthTamed = tokens[8];
                StrengthTamedMin = StrengthTamed == "" ? "" : StrengthTamed.Split('-')[0];
                StrengthTamedMax = StrengthTamed == "" ? "" : StrengthTamed.Split('-')[1];
                StrengthWild = tokens[9];
                StrengthWildMin = StrengthWild == "" ? "" : StrengthWild.Split('-')[0];
                StrengthWildMax = StrengthWild == "" ? "" : StrengthWild.Split('-')[1];
                DexterityTamed = tokens[10];
                DexterityTamedMin = DexterityTamed == "" ? "" : DexterityTamed.Split('-')[0];
                DexterityTamedMax = DexterityTamed == "" ? "" : DexterityTamed.Split('-')[1];
                DexterityWild = tokens[11];
                DexterityWildMin = DexterityWild == "" ? "" : DexterityWild.Split('-')[0];
                DexterityWildMax = DexterityWild == "" ? "" : DexterityWild.Split('-')[1];
                IntelligenceTamed = tokens[12];
                IntelligenceTamedMin = IntelligenceTamed == "" ? "" : IntelligenceTamed.Split('-')[0];
                IntelligenceTamedMax = IntelligenceTamed == "" ? "" : IntelligenceTamed.Split('-')[1];
                IntelligenceWild = tokens[13];
                IntelligenceWildMin = IntelligenceWild == "" ? "" : IntelligenceWild.Split('-')[0];
                IntelligenceWildMax = IntelligenceWild == "" ? "" : IntelligenceWild.Split('-')[1];
                ResistPhysicalRange = tokens[14];
                ResistPhysicalMinRange = ResistPhysicalRange == "" ? "" : ResistPhysicalRange.Split('-')[0];
                ResistPhysicalMaxRange = ResistPhysicalRange == "" ? "" : ResistPhysicalRange.Split('-')[1];
                ResistFireRange = tokens[15];
                ResistFireMinRange = ResistFireRange == "" ? "" : ResistFireRange.Split('-')[0];
                ResistFireMaxRange = ResistFireRange == "" ? "" : ResistFireRange.Split('-')[1];
                ResistColdRange = tokens[16];
                ResistColdMinRange = ResistColdRange == "" ? "" : ResistColdRange.Split('-')[0];
                ResistColdMaxRange = ResistColdRange == "" ? "" : ResistColdRange.Split('-')[1];
                ResistPoisonRange = tokens[17];
                ResistPoisonMinRange = ResistPoisonRange == "" ? "" : ResistPoisonRange.Split('-')[0];
                ResistPoisonMaxRange = ResistPoisonRange == "" ? "" : ResistPoisonRange.Split('-')[1];
                ResistEnergyRange = tokens[18];
                ResistEnergyMinRange = ResistEnergyRange == "" ? "" : ResistEnergyRange.Split('-')[0];
                ResistEnergyMaxRange = ResistEnergyRange == "" ? "" : ResistEnergyRange.Split('-')[1];
            }
            (Rarity, RarityColour, Status, Name) = GetRarityStatus(rarityColours);
            (TotalStats, TotalStatsValue) = CalculateStats(this);
            (TotalHits, TotalHitsValue) = CalculateHits(this);
            (TotalResists, TotalResistsValue) = CalculateResists(this);
            (TotalSkills, TotalSkillsValue) = CalculateSkills(this);
            double _total = Convert.ToDouble(TotalStats) + Convert.ToDouble(TotalHits) + double.Parse(TotalResists) + Convert.ToDouble(TotalSkills);
            TotalAll = _total.ToString();
            double _totalall = Convert.ToDouble(TotalStatsValue) + Convert.ToDouble(TotalHitsValue) + int.Parse(TotalResistsValue) + Convert.ToDouble(TotalSkillsValue);
            TotalAllValue = _totalall.ToString();
            (StatsWeight, HitsWeight, ResistsWeight, SkillsWeight, TotalWeight) = CalculateWeights(this);
        }

        public (string Rarity, string RarityColour, string Status, string Name) GetRarityStatus(Dictionary<string, string> rarityColours)
        {
            var targetPropList = Mobiles.GetPropStringList(Target.GetLast());
            string targetPropTextString = string.Join(",", targetPropList);
            string targetRarityColour;
            string targetStatus;
            string targetName;
            int _nameTest = targetPropTextString.IndexOf('[');

            if( _nameTest > 0)
            {
                targetName = _nameTest != -1 ? targetPropTextString.Substring(0, _nameTest).Trim() : targetPropTextString.Trim();
            }
            else
            {
                int _commaIndex = targetPropTextString.IndexOf(',');
                targetName = targetPropTextString.Substring(0, _commaIndex).Trim();
            }
            
            Handler.SendMessage(MessageType.Debug, $"Pet Def: {targetPropTextString}");
            Handler.SendMessage(MessageType.Debug, $"Name: {targetName}");

            if (targetPropTextString.Contains("Wild"))
            {
                targetStatus = "Wild";
            }
            else
            {
                targetStatus = "Tamed";
            }
                
            if (rarityColours.TryGetValue(targetPropTextString, out targetRarityColour))
            {
                return (targetPropTextString, targetRarityColour, targetStatus, targetName);
            }

            if (rarityColours.TryGetValue(targetPropTextString, out targetRarityColour)){} // Get the color based in pet rarity
            
            string[] keywords = { "Exotic", "Legendary", "Exquisite", "Rare" };
            foreach (string keyword in keywords)
                {
                    if (targetPropTextString.Contains(keyword))
                    {
                        return (keyword, rarityColours[keyword], targetStatus, targetName);
                    }
                }
            // Return common if the pet isn't anything special.
            return ("Common", rarityColours["Common"], targetStatus, targetName);
        }

        public (string, string) CalculateStats(Pet myPet)
        {
            var targetPropList = Mobiles.GetPropStringList(Target.GetLast());
            string targetPropTextString = string.Join(",", targetPropList);
            string totalStats = "";
            string totalStatsValue = "";

            int stats = int.Parse(myPet.Strength) + int.Parse(myPet.Dexterity) + int.Parse(myPet.Intelligence);

            if (myPet.Status == "Wild")
            {
                if (myPet.Halved == true)
                {
                    int statsValue = (int.Parse(myPet.StrengthWildMax) / 2) + (int.Parse(myPet.DexterityWildMax) / 2) + (int.Parse(myPet.IntelligenceWildMax) / 2);
                    totalStats = stats.ToString();
                    totalStatsValue = statsValue.ToString();
                }
                else
                {
                    int statsValue = int.Parse(myPet.StrengthWildMax) + int.Parse(myPet.DexterityWildMax) + int.Parse(myPet.IntelligenceWildMax);
                    totalStats = stats.ToString();
                    totalStatsValue = statsValue.ToString();
                }
            }
            else
            {
                int statsValue = int.Parse(myPet.StrengthTamedMax) + int.Parse(myPet.DexterityTamedMax) + int.Parse(myPet.IntelligenceTamedMax);
                totalStats = stats.ToString();
                totalStatsValue = statsValue.ToString();
            }

            return (totalStats, totalStatsValue);
        }

        public (string, string) CalculateHits(Pet myPet)
        {
            var targetPropList = Mobiles.GetPropStringList(Target.GetLast());
            string targetPropTextString = string.Join(",", targetPropList);
            string totalHits = "";
            string totalHitsValue = "";
            
            int hits = int.Parse(myPet.HitsMax);

            if (myPet.Status == "Wild")
            {
                if (myPet.Halved == true)
                {
                    int hitsValue = (int.Parse(myPet.HitsWildMax) / 2);
                    totalHits = hits.ToString();
                    totalHitsValue = hitsValue.ToString();
                }
                else
                {
                    int hitsValue = int.Parse(myPet.HitsWildMax);
                    totalHits = hits.ToString();
                    totalHitsValue = hitsValue.ToString();
                }
            }
            else
            {
                int hitsValue = int.Parse(myPet.HitsTamedMax);
                totalHits = hits.ToString();
                totalHitsValue = hitsValue.ToString();
            }

            //Handler.SendMessage(MessageType.Debug, $"TotalHits: {totalHits} TotalHitsValue: {totalHitsValue}");
            return (totalHits, totalHitsValue);
        }

        public (string, string) CalculateResists(Pet myPet)
        {

            string totalResists = "";
            string totalResistsValue = "";
            int resists = int.Parse(myPet.ResistPhysical) + int.Parse(myPet.ResistFire) + int.Parse(myPet.ResistCold) + int.Parse(myPet.ResistPoison) + int.Parse(myPet.ResistEnergy);
            int resistsValue = int.Parse(myPet.ResistPhysicalMaxRange) + int.Parse(myPet.ResistFireMaxRange) + int.Parse(myPet.ResistColdMaxRange) + int.Parse(myPet.ResistPoisonMaxRange) + int.Parse(myPet.ResistEnergyMaxRange);
            totalResists = resists.ToString();
            totalResistsValue = resistsValue.ToString();

            //Handler.SendMessage(MessageType.Debug, $"TotalResists: {totalResists} TotalResistsValue: {totalResistsValue}");
            return (totalResists, totalResistsValue);
        }

        public (string, string) CalculateSkills(Pet myPet)
        {
            string totalSkills = "";
            string totalSkillsValue = "";

            double skillMins = 0.0;
            double skillMinValue;
            if (double.TryParse(myPet.WrestlingMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.TacticsMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.ResistingSpellsMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.AnatomyMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.HealingMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.PoisoningMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.DetectingHiddenMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.HidingMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.ParryingMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.MageryMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.EvalIntelligenceMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.MeditationMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.NecromancyMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.SpiritSpeakMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.MysticismMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.FocusMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.SpellweavingMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.DiscordanceMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.BushidoMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.NinjitsuMin, out skillMinValue)) skillMins += skillMinValue;
            if (double.TryParse(myPet.ChivalryMin, out skillMinValue)) skillMins += skillMinValue;

            int skillMaxs = 0;
            int skillMaxValue;
            int.TryParse(myPet.WrestlingMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.TacticsMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.ResistingSpellsMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.AnatomyMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.HealingMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.PoisoningMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.DetectingHiddenMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.HidingMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.ParryingMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.MageryMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.EvalIntelligenceMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.MeditationMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.NecromancyMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.SpiritSpeakMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.MysticismMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.FocusMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.SpellweavingMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.DiscordanceMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.BushidoMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.NinjitsuMax, out skillMaxValue); skillMaxs += skillMaxValue;
            int.TryParse(myPet.ChivalryMax, out skillMaxValue); skillMaxs += skillMaxValue;

            totalSkills = skillMins.ToString();
            totalSkillsValue = skillMaxs.ToString();

            //Handler.SendMessage(MessageType.Debug, $"TotalSkills: {totalSkills} TotalSkillsValue: {totalSkillsValue}");
            return (totalSkills, totalSkillsValue);
        }

        public (string, string, string, string, string) CalculateWeights (Pet myPet)
        {        
            //////////////////////////////
            // Calculate Stat Weights
            //////////////////////////////
            int weightStatsCalc = int.Parse(myPet.Strength) + int.Parse(myPet.Dexterity) + int.Parse(myPet.Intelligence);
            int weightTamedStatsCalc = int.Parse(myPet.StrengthTamedMax) + int.Parse(myPet.DexterityTamedMax) + int.Parse(myPet.IntelligenceTamedMax);
            double weightStats = 0.0;

            if (myPet.Status == "Wild")
            {
                int weightWildStatsCalc = int.Parse(myPet.StrengthWildMax) + int.Parse(myPet.DexterityWildMax) + int.Parse(myPet.IntelligenceWildMax);

                if (myPet.Halved == true)
                {
                    weightStats = (double)5 * ((double)weightStatsCalc / ((double)weightWildStatsCalc / 2));

                }
                else
                {
                    weightStats = (double)5 * ((double)weightStatsCalc / ((double)weightWildStatsCalc));
                }
            }
            else
            {
                weightStats = (double)5 * ((double)weightStatsCalc / ((double)weightTamedStatsCalc));
            }
            
            //////////////////////////////
            // Calculate Hits Weights
            //////////////////////////////
            double weightHits = 0.0;

            if (myPet.Status == "Wild")
            {
                if (myPet.Halved == true)
                {
                    weightHits = (double)25 * ((double)int.Parse(myPet.HitsMax) / ((double)int.Parse(myPet.HitsWildMax) / 2));
                }
                else
                {
                    weightHits = (double)25 * ((double)int.Parse(myPet.HitsMax) / ((double)int.Parse(myPet.HitsWildMax)));
                }
            }
            else
            {
                weightHits = (double)25 * ((double)int.Parse(myPet.HitsMax) / ((double)int.Parse(myPet.HitsTamedMax)));
            }
            
            //////////////////////////////
            // Calculate Resists Weights
            //////////////////////////////
            double weightResists = (double)65 * (double.Parse(myPet.TotalResists) / double.Parse(myPet.TotalResistsValue));

            //////////////////////////////
            // Calculate Skills Weights
            //////////////////////////////
            double weightSkills = (double)5 * (double.Parse(myPet.TotalSkills) / double.Parse(myPet.TotalSkillsValue));

            //////////////////////////////
            // Calculate Total Weight
            //////////////////////////////
            double weightTotal = weightStats + weightHits + weightResists + weightSkills;

            return (weightStats.ToString("0.0"), weightHits.ToString("0.0"), weightResists.ToString("0.0"), weightSkills.ToString("0.0"), weightTotal.ToString("0.0"));
        }
    }

    public static class Handler
    {
        // Thanks Dorana for your logger!
        internal static void SendMessage(MessageType type, string message)
        {
            switch (type)
            {
                case MessageType.Prompt:
                    Player.HeadMessage(0x90, message);
                    break;
                case MessageType.Log:
                    Misc.SendMessage(message);
                    break;
                case MessageType.Error:
                    Misc.SendMessage(message, 33);
                    Player.HeadMessage(0x23, message);
                    break;
                case MessageType.Info:
                    Misc.SendMessage(message, 0x99);
                    Player.HeadMessage(0x99, message);
                    break;
                case MessageType.Debug:
                        Misc.SendMessage(message);
                        var logFile = Path.Combine(Engine.RootPath, "intensity.log");
                        File.AppendAllText(logFile, message + Environment.NewLine);
                    break;
            }
        }
    }

    internal enum MessageType
    {
        Prompt = 0,
        Log = 1,
        Error = 2,
        Info = 3,
        Debug = 4,
    }
}
