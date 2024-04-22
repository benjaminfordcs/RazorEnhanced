# 4/19/24: Thanks to Q for filling out the necro spell dict, eval int, and some other suggestions! 

# If you're training Spellweaving, use at your own risk!! 
# Word of Death can hurt. Discard your focus crystal prior to running and make sure you have Greater Heal!
# If you're training anything but Spellweaving, make sure your spell training gear gives you 100% LRC
# If you're training Chivalry, be sure to tithe 100 gold
# If you're training Chivalry or Mysticism, it'll be helpful to have some skill in Magery to cast Harm and Greater Heal. If you don't, then you'll need to either train pre-45 manually or replace the harming/healing logic with what you do have available.
# This also assumes that you have at least 60 max health. Use at your own risk if you don't...

# TODO: Allow specification of a healing spell
# TODO: Come up with a system where the user can specify which skills are to be trained – Can the script check if the skill is set to go up/down/locked, and only train those skills that are set up, and skip ones locked or set to go down?

# Update this to point to a list that specifies your equipped armor that blocks meditation.
# Recommended dress drag delay: 500 ms
# If you specify an empty dress list "", this script won't undress or dress
dressList = ""
# dressList = "spell training"

# skillsToRaise = ["Magery", "Necromancy", "Chivalry", "Bushido", "Mysticism", "EvalInt", "Spellweaving"]
# TODO: Make this a list with applicable skills, then iterate. But figure out which skills of these the user wants to raise...
skillToRaise = "Magery"

maxManaCostDict = {
    "Magery": 50,
    "Spellweaving": 50,
    "Necromancy": 50,
    "Chivalry": 20,
    "Bushido": 10,
    "Mysticism": 50
}

# If we have a mage weapon, get the value so we can calculate the real skill value
weapon = Player.GetItemOnLayer("LeftHand")
mageWeaponValue = Items.GetPropValue(weapon, "Mage Weapon")
wasMageWeaponEquipped = mageWeaponValue > 0 # Track whether or not a mage weapon was equipped at the start. May need to unequip...

currentSkillCap = Player.GetSkillCap(skillToRaise)

# TODO: Fill these out with the other spells from UOGuide
magerySpellDict = {20: "Clumsy", 40: "Bless", 65: "Poison Field", 80: "Reveal", 87: "Mass Dispel", currentSkillCap: "Earthquake"}
spellweavingSpellDict = {15: "Arcane Circle", 32: "Immolating Weapon", 52: "Reaper Form", 89: "Essence of Wind", 103: "Wildfire", currentSkillCap: "Word of Death"} # Key: Max Skill to cast
necromancySpellDict = {40: "Curse Weapon", 50: "Pain Spike", 70: "Horrific Beast", 90: "Wither", currentSkillCap: "Lich Form"}
chivalrySpellDict =  {15: "Close Wounds", 45: "Consecrate Weapon", 60: "Divine Fury", 70: "Enemy of One", currentSkillCap: "Holy Light"}
bushidoSpellDict = {60: "Confidence", 77.5: "Counter Attack"} # Bushido is special in that the high-value skills need a hostile target.
mysticismSpellDict =  {20: "Healing Stone", 40: "Eagle Strike", 62: "Stone Form", 83: "Cleansing Winds", currentSkillCap: "Nether Cyclone"}
evalIntDict = {currentSkillCap: "Clumsy"}

spellDict = {
    "Magery": magerySpellDict,
    "Spellweaving" : spellweavingSpellDict,
    "Necromancy": necromancySpellDict,
    "Chivalry" : chivalrySpellDict,
    "Bushido": bushidoSpellDict,
    "Mysticism": mysticismSpellDict,
    "EvalInt": evalIntDict
}

def getCurrentSpell(currentSkill, effectiveMageWeaponValue):
    currentSkillValue=Player.GetSkillValue(currentSkill)

    # Note, if training magery, your "currentSkillValue" will be modified by any mage weapon in your hand.
    # But if your mage skill is lower than the mage weapon's value... you will fizzle a lot.
    if currentSkill == "Magery" and effectiveMageWeaponValue > 0:
        # Add in any mage weapon penalty back in since that's what your real skill is
        currentSkillValue += effectiveMageWeaponValue
    
    stoppingPoint = currentSkillCap + 0.01
    lowestSkillBreakpointToCast = stoppingPoint
    
    # Sort the list so we go through the breakpoints in order.
    for skillBreakpoint in sorted(list(spellDict[currentSkill].keys())):
        if currentSkillValue < skillBreakpoint:
            # Valid spell to train on
            if lowestSkillBreakpointToCast > skillBreakpoint:
                # This is the lowest skill ranking training spell to cast so far
                lowestSkillBreakpointToCast = skillBreakpoint
    
    if lowestSkillBreakpointToCast == stoppingPoint:
        # TODO: Return something or allow iteration over other skills
        raise Exception(f"All done raising {currentSkill}. Current skill value is: {currentSkillValue}")
    else:
        return spellDict[currentSkill][lowestSkillBreakpointToCast]


def castSpell(currentSkill, spellName):
    if currentSkill == "Spellweaving":
        # TODO: Edit this to be mindful of foolish casters with less than 60 max HP
        if currentSpell == "Word of Death" and Player.Hits < 60:
            while Player.Hits < 60:
                Spells.Cast("Greater Heal")
                Target.WaitForTarget(10000, False)
                Target.Self()
                Misc.Pause(1000)
                
        Spells.CastSpellweaving(currentSpell)
        
        if currentSpell == "Word of Death":
            Target.WaitForTarget(10000, False)
            Target.Self()
            Misc.Pause(1000)
        elif currentSpell == "Wildfire":
            Target.WaitForTarget(10000, False)
            Target.Self()
            Misc.Pause(1000)
        else:
            Misc.Pause(4000)
    elif currentSkill == "Magery":
        Spells.CastMagery(currentSpell)
        if currentSpell in ["Clumsy", "Bless", "Poison Field", "Reveal", "Energy Field", "Mass Dispel"]:
            Target.WaitForTarget(10000, False)
            Target.Self()
        # TODO: Consider making a pause dict for each spell...
        Misc.Pause(4000)
    elif currentSkill == "Necromancy":
        Spells.CastNecro(currentSpell)
        if currentSpell == "Pain Spike" and Player.Hits < 60:
            while Player.Hits < 60:
                Spells.Cast("Greater Heal")
                Target.WaitForTarget(10000, False)
                Target.Self()
                Misc.Pause(1000)
        if currentSpell == "Lich Form" and Player.Hits < 60:
            while Player.Hits < 60:
                Spells.Cast("Greater Heal")
                Target.WaitForTarget(10000, False)
                Target.Self()
                Misc.Pause(1000)
        if currentSpell in ["Pain Spike"]:
            Target.WaitForTarget(10000, False)
            Target.Self()
        if currentSpell in ["Wither"] and Player.BuffsExist("Horrific Beast"):
            Spells.CastNecro("Horrific Beast")
            Misc.Pause(4000) 
        Misc.Pause(4000)
    elif currentSkill == "Chivalry":
        if currentSpell == "Close Wounds" and Player.Hits == Player.HitsMax:
            Spells.CastMagery("Harm")
            Target.WaitForTarget(10000, False)
            Target.Self()
            Misc.Pause(4000)
        Spells.CastChivalry(currentSpell)
        if currentSpell in ["Close Wounds"]:
            Target.WaitForTarget(10000, False)
            Target.Self()
        Misc.Pause(4000)
    elif currentSkill == "Bushido":
        Spells.CastBushido(currentSpell)
        Misc.Pause(4000)
    elif currentSkill == "Mysticism":
        Spells.CastMysticism(currentSpell)
        if currentSpell == "Eagle Strike" and Player.Hits < 60:
            while Player.Hits < 60:
                Spells.Cast("Greater Heal")
                Target.WaitForTarget(10000, False)
                Target.Self()
                Misc.Pause(1000)
        if currentSpell in ["Eagle Strike", "Cleansing Winds", "Hail Storm", "Nether Cyclone"]:
            Target.WaitForTarget(10000, False)
            Target.Self()
        Misc.Pause(4000)
    elif currentSkill == "EvalInt":
        Spells.CastMagery(currentSpell)
        Misc.Pause(4000)

        
def getDressed():
    Dress.DressFStart()
    while Dress.DressStatus():
        Misc.Pause(500)
    Misc.Pause(1000)

def getUndressed():
    Dress.UnDressFStart()
    while Dress.UnDressStatus():
        Misc.Pause(500)
    Misc.Pause(1000)
    
# MAIN
# Make sure you are dressed
if dressList != "":
    Dress.ChangeList(dressList)
    getDressed()

while (Player.GetSkillValue(skillToRaise) + mageWeaponValue) < currentSkillCap:
    effectiveMageWeaponValue = mageWeaponValue # Whether or not mage weapon is in effect is, well, whether or not it is equipped.
    
    # Update Mage weapon calculations only if we're actually training magery
    if skillToRaise == "Magery":
        currentRawMagerySkillValue = Player.GetSkillValue("Magery")        
        # If a mage weapon was unequipped earlier, and our magery skill is high enough, equip it.
        if wasMageWeaponEquipped and currentRawMagerySkillValue > mageWeaponValue:
            effectiveMageWeaponValue = mageWeaponValue
            Player.EquipItem(weapon)
            Misc.Pause(1000)
        elif mageWeaponValue > 0 and currentRawMagerySkillValue <= mageWeaponValue:
            # Unequip the mage weapon and re-equip it later (assuming it is equipped)
            if Player.GetItemOnLayer('LeftHand') != None:
                Player.UnEquipItemByLayer("LeftHand")
                effectiveMageWeaponValue = 0 # Unequipped
                Misc.Pause(1000)

    currentSpell = getCurrentSpell(skillToRaise, effectiveMageWeaponValue) # TODO: ... is it really caring about scoping now?
    castSpell(skillToRaise, currentSpell)
   
    # Undress when low on mana
    if Player.Mana < maxManaCostDict[skillToRaise]:
        Misc.SendMessage("Low on mana. Meditate.", False)
        if dressList != "":
            getUndressed()
        Player.UseSkill("Meditation")
        
        while (Player.Mana / Player.ManaMax) < 1:
            Misc.Pause(2000)
        
        # Dress when mana full
        if dressList != "":
            getDressed()

# Before ending... make sure not in lich form. Or else.
if Player.BuffsExist("Lich Form"):
    while Player.BuffsExist("Lich Form"):
        Spells.CastNecro("Lich Form")
        Misc.Pause(4000)