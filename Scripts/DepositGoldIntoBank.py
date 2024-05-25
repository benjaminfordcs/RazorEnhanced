# Searches your bag for all gold piles and puts them in your bank
#########
# Change these 2 serials to your respective containers. (Use inspect entities and get the serial)
backpack_serial=0x406AE3F7 # Your main backpack
bank_serial=0x40B5EDCE # Your bank box

##########
gold=0x0EED
color=-1
search_depth=2

Player.ChatSay("bank")
Misc.Pause(1000)

gold_piles = Items.FindAllByID([gold], color, backpack_serial, search_depth)
print(gold_piles)
for gold_pile in gold_piles:    
    Items.Move(gold_pile, bank_serial, 0)
