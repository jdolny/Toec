using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Software : IInventorySearcher
    {
        private readonly List<DtoSoftwareInventory> _softwareIventory;

        public Software()
        {
            _softwareIventory = new List<DtoSoftwareInventory>();
        }

        public void Search(DtoInventoryCollection collection)
        {
            var userkey = Registry.Users;
            if (userkey == null) return;
            foreach (var subKey in userkey.GetSubKeyNames())
            {
                var userUninstallKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
                userUninstallKey =
                    userUninstallKey.OpenSubKey(string.Format(
                        @"{0}\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", subKey));
                if (userUninstallKey == null) continue;
                AddToCollection(userUninstallKey);

                userUninstallKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry32);
                userUninstallKey =
                    userUninstallKey.OpenSubKey(string.Format(
                        @"{0}\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", subKey));
                if (userUninstallKey == null) continue;
                AddToCollection(userUninstallKey);
            }

            var machineUninstallKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            machineUninstallKey = machineUninstallKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (machineUninstallKey == null) return;
            AddToCollection(machineUninstallKey);

            machineUninstallKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            machineUninstallKey = machineUninstallKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (machineUninstallKey == null) return;
            AddToCollection(machineUninstallKey);

            collection.Software = _softwareIventory.Distinct(new SoftwareComparer()).ToList();
        }

        private void AddToCollection(RegistryKey key)
        {
            foreach (var subKey in key.GetSubKeyNames())
            {
                var softwareKey = key.OpenSubKey(subKey);
                if (softwareKey == null) continue;
                var name = Convert.ToString(softwareKey.GetValue("DisplayName"));
                var version = Convert.ToString(softwareKey.GetValue("DisplayVersion"));
                var uninstallString = Convert.ToString(softwareKey.GetValue("UninstallString"));
                if (string.IsNullOrEmpty(name)) continue;
                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(version))
                    continue;
                var softwareInventory = new DtoSoftwareInventory();
                softwareInventory.Name = name;
                softwareInventory.Version = version;
                softwareInventory.UninstallString = uninstallString;
                if (string.IsNullOrEmpty(version))
                {
                    _softwareIventory.Add(softwareInventory);
                    continue;
                }

                var versionArray = version.Split('.');
                if (versionArray.Length == 1)
                {
                    softwareInventory.Major = ParseVersion(versionArray[0]);
                }
                else if (versionArray.Length == 2)
                {
                    softwareInventory.Major = ParseVersion(versionArray[0]);
                    softwareInventory.Minor = ParseVersion(versionArray[1]);
                }
                else if (versionArray.Length == 3)
                {
                    softwareInventory.Major = ParseVersion(versionArray[0]);
                    softwareInventory.Minor = ParseVersion(versionArray[1]);
                    softwareInventory.Build = ParseVersion(versionArray[2]);
                }
                else if (versionArray.Length == 4)
                {
                    softwareInventory.Major = ParseVersion(versionArray[0]);
                    softwareInventory.Minor = ParseVersion(versionArray[1]);
                    softwareInventory.Build = ParseVersion(versionArray[2]);
                    softwareInventory.Revision = ParseVersion(versionArray[3]);
                }
                else
                {
                    softwareInventory.Major = ParseVersion(versionArray[0]);
                    softwareInventory.Minor = ParseVersion(versionArray[1]);
                    softwareInventory.Build = ParseVersion(versionArray[2]);
                    softwareInventory.Revision = ParseVersion(versionArray[3]);
                }
                _softwareIventory.Add(softwareInventory);
            }
        }

        private int ParseVersion(string num)
        {
            int value;
            if (!int.TryParse(num, out value))
                return 0;
            return value;
        }
    }

    internal class SoftwareComparer : IEqualityComparer<DtoSoftwareInventory>
    {
        public bool Equals(DtoSoftwareInventory x, DtoSoftwareInventory y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Name.Equals(y.Name) && x.Version.Equals(y.Version);
        }

        public int GetHashCode(DtoSoftwareInventory software)
        {
            var hashName = software.Name == null ? 0 : software.Name.GetHashCode();
            var hashVersion = software.Version.GetHashCode();
            return hashName ^ hashVersion;
        }
    }
}