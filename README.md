# Black Ops II Plutonium Gun Locker Fix
Developed by  [**DoktorSAS**](https://github.com/DoktorSAS)

## Contributors
- [**fed**](https://github.com/fedddddd) for helping with the plugin
- 
Through these files it will be possible to get the Gun Locker Fully Working. Implementing the following scripts is very easy. In order to make the iw4m admin plugin work you also need to modify the mod in your server. You will have to add some lines of code
### Prerequisites:
- [*IW4M Admin*](https://github.com/RaidMax/IW4M-Admin/releases): To have the bank running on a server you must have IW4M 

### How does this work?
Everything has been done to make it as simple as possible, in fact it's just a few lines of code to add to a mod and a plugin to insert in the plugin folder of iw4m admin.

### How to use it?
To implement these features takes two minutes, just follow this guide carefully and you will understand how to implement the bank on your servers.

### Guide  
##### Step by step 
1. Download the [**compiled files**](https://github.com/DoktorSAS/bank-fix) and not the source code 
2. Take/Copy the **gun_locker_fix.dll** file and put it in the **plugins** folder of **IW4M Admin**

### How to add the code on my mods?
1. Open your not compiled mod file with GSC Studio or other text editor
2. Add to your init function this lines of code
```
 if (getDvar("mapname") == "zm_buried" && getDvar("mapname") == "zm_highrise" && getDvar("mapname") == "zm_transit") {
		level thread onPlayerConnect_gun_locker_fix();
		level thread onEndGame_gun_locker_fix();
	}
```
like
```
init(){
     if (getDvar("mapname") == "zm_buried" && getDvar("mapname") == "zm_highrise" && getDvar("mapname") == "zm_transit") {
		level thread onPlayerConnect_gun_locker_fix();
		level thread onEndGame_gun_locker_fix();
	}
}
```
3. Add the this other funciton where you want in the mod
```
onEndGame_gun_locker_fix(){
	level waittill("end_game");
	foreach(player in level.players){
		if(isDefined(player.stored_weapon_data)){
			new_dvar_value = "IW4MLOCKER;" + player.guid + "," + player.stored_weapon_data["name"] + "," + player.stored_weapon_data["lh_clip"] + "," + player.stored_weapon_data["clip"] + "," + player.stored_weapon_data["stock"] + "," + player.stored_weapon_data["alt_clip"]  + "," + player.stored_weapon_data["alt_stock"];
			logPrint(new_dvar_value + "\n");
		}else{
			new_dvar_value = "IW4MLOCKER;" + player.guid + "," + "none";
			logPrint(new_dvar_value + "\n");
		}
	}
}
```

```
onPlayerConnect_gun_locker_fix(){
    for(;;){
        level waittill("connected", player);
        player thread gun_locker_fix();
        player thread onPlayerDisconnect_fun_locker_fix();
    }
}
```
```
onPlayerDisconnect_fun_locker_fix(){
	self waittill("disconnect");
	stored_weapon_data = self.stored_weapon_data;
	guid = self.guid;
	if(isDefined(self.stored_weapon_data)){
		new_dvar_value = "IW4MLOCKER;" + guid + "," + stored_weapon_data["name"] + "," + stored_weapon_data["lh_clip"] + "," + stored_weapon_data["clip"] + "," + stored_weapon_data["stock"] + "," + stored_weapon_data["alt_clip"]  + "," + stored_weapon_data["alt_stock"];
		logPrint(new_dvar_value + "\n");
	}else{
		new_dvar_value = "IW4MLOCKER;" + guid + "," + "none";
		logPrint(new_dvar_value + "\n");
	}
}
```
```
gun_locker_fix(){
	level endon("end_game");
	self waittill("spawned_player");
	while(getDvar( "guns_clients_information" ) == "" || !self setLockerGun(  )) // As long as the value of the bank is not valid then it remains in the loop
		wait 0.001;

}
```

```
setLockerGun(  ) {
	guns_data = strTok(getDvar( "guns_clients_information" ), "-"); // The dvar is divided into many elements so many players are in game
	for (i = 0; i < guns_data.size; i++) {
		client_data = strTok(guns_data[i], ","); // Divides each player's data into arrays with two values
		if (int(client_data[0]) == int(self.guid)) { // If the GUID matches the user in analysis then sets the value of the bank and says that the value is valid
			weapondata = [];
			if(client_data[1] != "none"){
        		weapondata[ "name" ] 		= client_data[1];
       			weapondata[ "lh_clip" ] 	= client_data[2];
        		weapondata[ "clip" ] 		= client_data[3];
       		 	weapondata[ "stock" ]	 	= client_data[4];
        		weapondata[ "alt_clip" ] 	= client_data[5];
        		weapondata[ "alt_stock" ]	= client_data[6];
        		self.stored_weapon_data = weapondata;
        		self iprintln("Weapon Set!");
			}
			return 1;
		}
	}
	return 0;
}
```
4. Compile the file
5. Put the compiled file in maps\mp\gametypes_zm\
6. Start the server
7. Start IW4M
8. **END**
