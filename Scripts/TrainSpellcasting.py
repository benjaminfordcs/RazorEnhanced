# WIP
# If you're training Spellweaving, use at your own risk!! 
# Word of Death can hurt. Discard your focus crystal prior to running and make sure you have Greater Heal!
# If you're training Magery or Necromancy or Mysticism, make sure your spell training gear gives you 100% LRC

# TODO: Allow specification of a healing spell


# Update this to point to a list that specifies your equipped armor that blocks meditation.
# Recommended dress drag delay: 500 ms
# If you specify an empty dress list, this script won't undress or dress
dressList = "spell training" 

maxManaCostForTrainingSpell=50

# If we have a mage weapon, get the value so we can calculate the real skill value
weapon = Player.GetItemOnLayer("LeftHand")
mageWeaponValue = Items.GetPropValue(weapon, "Mage Weapon")

# TODO: Make this a list with applicable skills, then iterate
skillToRaise = "Necromancy"
currentSkillCap = Player.GetSkillCap(skillToRaise)

# TODO: Fill these out with the other spells from UOGuide
magerySpellDict = {currentSkillCap: "Earthquake"}
spellweavingSpellDict = {89: "Essence of Wind", 103: "Wildfire", currentSkillCap: "Word of Death"} # Key: Max Skill to cast
necromancySpellDict = {currentSkillCap: "Vampiric Embrace"}

spellDict = {
    "Magery": magerySpellDict,
    "Spellweaving" : spellweavingSpellDict,
    "Necromancy": necromancySpellDict
}

def getCurrentSpell(currentSkill):
    currentSkillValue=Player.GetSkillValue(currentSkill)
    
    # Note, if we're training magery, your "currentSkillValue" will be modified by any mage weapon in your hand.
    if currentSkill == "Magery" and mageWeaponValue > 0:
        # Add in any mage weapon penalty back in since that's what your real skill is
        currentSkillValue += mageWeaponValue
    
    stoppingPoint = currentSkillCap + 0.01
    lowestSkillBreakpointToCast = stoppingPoint
    
    for skillBreakpoint in spellDict[currentSkill].keys():
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
        # TODO: Consider making a pause dict for each spell...
        Misc.Pause(4000)
    elif currentSkill == "Necromancy":
        Spells.CastNecro(currentSpell)
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

while Player.GetSkillValue(skillToRaise) < currentSkillCap:
    currentSpell = getCurrentSpell(skillToRaise)
    castSpell(skillToRaise, currentSpell)
   
    # Undress when low on mana
    if Player.Mana < maxManaCostForTrainingSpell:
        Misc.SendMessage("Low on mana. Meditate.", False)
        if dressList != "":
            getUndressed()
        Player.UseSkill("Meditation")
        
        while (Player.Mana / Player.ManaMax) < 1:
            Misc.Pause(2000)
        
        # Dress when mana full
        if dressList != "":
            getDressed()