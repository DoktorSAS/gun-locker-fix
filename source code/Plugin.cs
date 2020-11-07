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
        public string Name => "Gun Locker Fix v1.0";

        public float Version => (float)Utilities.GetVersionAsDouble();

        public string Author => "DoktorSAS";

        public readonly IMetaService _metaService;

        public string dvarValue = "";

        public Plugin(IConfigurationHandlerFactory configurationHandlerFactory, IDatabaseContextFactory databaseContextFactory, ITranslationLookup translationLookup, IMetaService metaService)
        {
            _metaService = metaService;
        }

        public void SetGunDvar(Server S)
        {
            try
            {
                for (int i = 0; i < S.Clients.Count; i++)
                {
                    if (S.Clients[i] == null)
                    {
                        continue;
                    }

                    dvarValue += $"{S.Clients[i].NetworkId},{S.Clients[i].GetAdditionalProperty<string>("locker_gun")}{(i < S.Clients.Count - 1 ? "-" : "")}";
                }
                ////S.RconParser.SetDvarAsync(S.RemoteConnection, "guns_clients_information", dvarValue);
            }
            catch (Exception e)
            {

            }
         
        }

        public async void LockerGunStatus(EFClient client, string mapName)
        {
            if ((await _metaService.GetPersistentMeta($"{mapName}_gun", client)) == null)
            {
                await _metaService.AddPersistentMeta($"{mapName}_gun", "none", client);
            }
        }

        public async void SetGunsDvar(Server S)
        {
            try
            {
                S.RconParser.SetDvarAsync(S.RemoteConnection, "guns_clients_information", "");
                string dvar = "";
                for (int i = 0; i < S.Clients.Count; i++)
                {
                    if (S.Clients[i] == null)
                    {
                        continue;
                    }

                    dvar += (i > 0 ? "-" : "") + $"{S.Clients[i].NetworkId},{(await _metaService.GetPersistentMeta($"{S.CurrentMap.Name}_gun", S.Clients[i])).Value}";
                }
                S.RconParser.SetDvarAsync(S.RemoteConnection, "guns_clients_information", dvar);
            }
            catch (Exception e)
            {

            }
         
        }
        public async Task SetGunMeta(EFClient C, string value, string data_name)
        {
            await _metaService.AddPersistentMeta(data_name, value, C);
        }

        public async Task OnEventAsync(GameEvent E, Server S)
        {

            switch (E.Type)
            {
                case (GameEvent.EventType.PreConnect):
                case (GameEvent.EventType.Join):
                case (GameEvent.EventType.MapChange):
                    if (S.CurrentMap.Name == "zm_buried" || S.CurrentMap.Name == "zm_highrise" || S.CurrentMap.Name == "zm_transit")
                    {
                        LockerGunStatus(E.Origin, S.CurrentMap.Name);
                        SetGunsDvar(S);
                    }
                    break;
                case (GameEvent.EventType.Unknown):
                    if (Regex.Match(E.Data, @"IW4MLOCKER;(\d+),(.+),(\d+),(\d+),(\d+),(\d+),(\d+)").Length > 0)
                    {
                        Console.WriteLine(E.Data);
                        string[] clientData = E.Data.Split(';')[1].Split(',');
                        EFClient C = S.GetClientsAsList().Find(c => c.NetworkId == Convert.ToInt64(clientData[0]));
                        await SetGunMeta(C, String.Join(",", clientData.Skip(1)), $"{S.CurrentMap.Name}_gun");
                        SetGunsDvar(S);
                    }

                    if (Regex.Match(E.Data, @"IW4MLOCKER;(\d+),none").Length > 0)
                    {
                        string[] clientData = E.Data.Split(';')[1].Split(',');
                        EFClient C = S.GetClientsAsList().Find(c => c.NetworkId == Convert.ToInt64(clientData[0]));
                        await SetGunMeta(C, "none", $"{S.CurrentMap.Name}_gun");
                        SetGunsDvar(S);
                    }
                    break;
            }
        }

        public async Task OnLoadAsync(IManager manager)
        {
            Console.WriteLine("Gun Locker Fix Plugin - Loaded Correctly");
            Console.WriteLine($"Developed by ({Author}) \nThanks to fed for Helping");
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
