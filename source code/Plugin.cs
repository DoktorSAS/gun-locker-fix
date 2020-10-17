using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using SharedLibraryCore.Configuration;
using System.Xml;
using System.Collections.Generic;
using SharedLibraryCore.Helpers;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using SharedLibraryCore.Database.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace gun_locker_fix
{
    public class Plugin : IPlugin
    {
        public string Name => "Gun Locker Fix";

        public float Version => (float)Utilities.GetVersionAsDouble();

        public string Author => "DoktorSAS";

        public readonly IMetaService _metaService;

        public string dvar_value = "";

        public Plugin(IConfigurationHandlerFactory configurationHandlerFactory, IDatabaseContextFactory databaseContextFactory, ITranslationLookup translationLookup, IMetaService metaService)
        {
            _metaService = metaService;
        }

        public void SetGunDvar(Server S)
        {
            for (int i = 0; i < S.Clients.Count; i++)
            {
                if (S.Clients[i] == null)
                {
                    continue;
                }
                long guid = S.Clients[i].NetworkId;
                dvar_value = dvar_value + S.Clients[i].NetworkId + "," + S.Clients[i].GetAdditionalProperty<string>("locker_gun");
                if(i < S.Clients.Count-1)
                    dvar_value = dvar_value + "-";
            }
            S.RconParser.SetDvarAsync(S.RemoteConnection, "guns_clients_information", dvar_value);
        }

        public async void LcokerGunStatus( EFClient client , string map_name)
        {
            if (((await _metaService.GetPersistentMeta(map_name + "_gun", client)) == null))
            {
                await _metaService.AddPersistentMeta(map_name + "_gun", "none", client);
                Console.WriteLine("The gun is not defined");
            }
            else
            {
                Console.WriteLine("The gun is defined and is value is " + (await _metaService.GetPersistentMeta(map_name + "_gun", client)).Value);
            }
                
        }
        public async void SetGunsDvar(Server S)
        {
            S.RconParser.SetDvarAsync(S.RemoteConnection, "guns_clients_information", "");
            string map_name = S.CurrentMap.Name;
            string dvar = "";
            for (int i = 0; i < S.Clients.Count; i++)
            {
                if (S.Clients[i] == null)
                {
                    continue;
                }
                
                dvar += (i > 0 ? "-" : "") + $"{S.Clients[i].NetworkId},{(await _metaService.GetPersistentMeta(map_name + "_gun", S.Clients[i])).Value}";
            }
            S.RconParser.SetDvarAsync(S.RemoteConnection, "guns_clients_information", dvar);
        }
        public async Task SetGunMeta(EFClient C, string value, string data_name)
        {
            //Console.WriteLine("Client: " + C.Name );
            await _metaService.AddPersistentMeta(data_name, value, C);
        }


        public async Task OnEventAsync(GameEvent E, Server S)
        {

            switch (E.Type)
            {
                case (GameEvent.EventType.PreConnect):
                case (GameEvent.EventType.Join):
                case (GameEvent.EventType.MapChange):
                    if(S.CurrentMap.Name == "zm_buried" || S.CurrentMap.Name == "zm_highrise" || S.CurrentMap.Name == "zm_transit") {
                        LcokerGunStatus(E.Origin, S.CurrentMap.Name);
                        SetGunsDvar(S);
                    }
                        
                break;
                case (GameEvent.EventType.Unknown):
   
                    if (Regex.Match(E.Data, @"IW4MLOCKER;(\d+),(.+),(\d+),(\d+),(\d+),(\d+),(\d+)").Length > 0)
                    {
                        string[] clinet_data = E.Data.Split(';')[1].Split(',');
                        EFClient c = S.GetClientsAsList().Find(c => c.NetworkId == Convert.ToInt64(clinet_data[0]));
                        await SetGunMeta(c, clinet_data[1] + "," + clinet_data[2] + "," + clinet_data[3] + "," + clinet_data[4] + "," + clinet_data[5] + "," + clinet_data[6], S.CurrentMap.Name + "_gun");
                        SetGunsDvar( S );
                    }

                    if(Regex.Match(E.Data, @"IW4MLOCKER;(\d+),none").Length > 0){
                        string[] clinet_data = E.Data.Split(';')[1].Split(',');
                        EFClient c = S.GetClientsAsList().Find(c => c.NetworkId == Convert.ToInt64(clinet_data[0]));
                        await SetGunMeta(c, "none", S.CurrentMap.Name + "_gun");
                        SetGunsDvar(S);
                    }
                 break;
            }
        }

        public async Task OnLoadAsync(IManager manager)
        {

            try { 
                Console.WriteLine("Gun Locker Fix Plugin - Loaded Correctly");
                Console.WriteLine($"Developed by ({Author}) \nThanks to fed for Helping");
            }
            catch (Exception e)
            {
                //Console.WriteLine($"BankFix not loaded, make sure you didn't mess up the json file: {e}");
            }
        }

        public Task OnTickAsync(Server S)
        {
            throw new NotImplementedException();
        }

        public Task OnUnloadAsync()
        {
            return Task.CompletedTask;
        }
    }
}