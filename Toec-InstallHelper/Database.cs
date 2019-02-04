using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace Toec_InstallHelper
{
    public class Database
    {
        private readonly string _basePath;
        private readonly string _clientVersion;
        private readonly string _connectionString;
        private readonly string _localApiPort;
        private readonly string _logLevel;
        private readonly string _remoteApiPort;
        private readonly string _resetDb;
        private readonly string _serverKey;
        private readonly Session _session;
        private readonly string _thumbPrint;
        private readonly string _userPortRange;
        private string _comServers;
        private bool _isNewInstall;

        public Database(Session session)
        {
            _basePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            _basePath = Path.Combine(_basePath, "Toec") + @"\";
            _comServers = session.CustomActionData["COM_SERVERS"];
            _localApiPort = session.CustomActionData["LOCAL_API_PORT"];
            _remoteApiPort = session.CustomActionData["REMOTE_API_PORT"];
            _serverKey = session.CustomActionData["SERVER_KEY"];
            _thumbPrint = session.CustomActionData["CA_THUMBPRINT"];
            _userPortRange = session.CustomActionData["USER_PORT_RANGE"];
            _resetDb = session.CustomActionData["RESET_DB"];
            _clientVersion = session.CustomActionData["CLIENT_VERSION"];
            _logLevel = session.CustomActionData["LOG_LEVEL"];
            _connectionString = @"Data Source=" + _basePath + "Toec.db;" + "Version=3; FailIfMissing=True;";
            _session = session;
        }

        private void DisplayError(string errorMessage)
        {
            _session.Log(errorMessage);
            var record = new Record();
            record.SetString(0, errorMessage);
            _session.Message(InstallMessage.Error, record);
        }

        public static byte[] EncryptData(byte[] data, bool isSystem, byte[] entropy)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length <= 0)
                throw new ArgumentException("data");

            ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
            return ProtectedData.Protect(data, entropy,
                isSystem ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private string GetDbSettingValue(string settingName)
        {
            try
            {
                string result;
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    var sql = "SELECT setting_value FROM settings WHERE setting_name = '" + settingName + "';";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        result = cmd.ExecuteScalar().ToString();
                    }
                    conn.Close();
                }
                return result;
            }
            catch (SQLiteException ex)
            {
                DisplayError("Could Not Read Setting " + settingName);
                _session.Log(ex.Message);
                if (_isNewInstall)
                    RemoveDb();
                throw;
            }
        }

        public ActionResult Initialize()
        {
            _session.Log("Starting Database Initialization");
            try
            {
                var installTempDir = Directory.GetCurrentDirectory() + @"\";
                File.Copy(_basePath + "SQLite.Interop.dll", installTempDir + "SQLite.Interop.dll");

                if (!File.Exists(_basePath + "Toec.db"))
                {
                    return NewSetup(false);
                }

                if (_resetDb.ToLower().Equals("true"))
                {
                    RemoveDb();
                    return NewSetup(true);
                }

                //From here on assume we are upgrading or reinstalling

                var existingVersion = GetDbSettingValue("version");
                if (string.IsNullOrEmpty(existingVersion))
                {
                    DisplayError("Could Not Determine Existing Client Version.");
                    return ActionResult.Failure;
                }
                if (string.IsNullOrEmpty(_clientVersion))
                {
                    DisplayError("Could Not Determine New Client Version.");
                    return ActionResult.Failure;
                }

                //If database schema update required, code here.
                /*not used yet, example db update
                if (!existingVersion.Equals(_clientVersion))
                {
                    var clientVersionArray = existingVersion.Split('.');
                    if (clientVersionArray.Length != 4)
                    {
                        DisplayError("Could Not Parse Existing Client Version. " + existingVersion);
                        return ActionResult.Failure;
                    }
                    var clientMajor = Convert.ToInt32(clientVersionArray[0]);
                    var clientMinor = Convert.ToInt32(clientVersionArray[1]);
                    var clientBuild = Convert.ToInt32(clientVersionArray[2]);
                    var clientRevision = Convert.ToInt32(clientVersionArray[3]);

                    if (clientMajor == 1 && clientMinor == 0 && clientBuild < 1)
                    {
                        _session.Log("Updating Client Database Schema To 1.0.1.0");
                        if (!UpdateDb_1_0_1())
                        {
                            DisplayError("Failed To Update Database Schema.");
                            return ActionResult.Failure;
                        }
                    }
                }
                */
                UpdateDbSetting(_clientVersion, "version");

                //All variables are optional on ugprade / reinstall
                if (!string.IsNullOrEmpty(_serverKey))
                {
                    var entropy = new byte[16];
                    new RNGCryptoServiceProvider().GetBytes(entropy);
                    var serverKeyBytes = Encoding.ASCII.GetBytes(_serverKey);
                    var encryptedKey = EncryptData(serverKeyBytes, true, entropy);
                    UpdateDbSetting(Convert.ToBase64String(entropy), "server_key_entropy");
                    UpdateDbSetting(Convert.ToBase64String(encryptedKey), "server_key");
                }

                if (!string.IsNullOrEmpty(_comServers))
                {
                    ValidateComServers();
                    UpdateDbSetting(_comServers, "initial_com_servers");
                    UpdateDbSetting(_comServers, "active_com_servers");
                }

                if (!string.IsNullOrEmpty(_localApiPort))
                    UpdateDbSetting(_localApiPort, "local_api_port");

                if (!string.IsNullOrEmpty(_remoteApiPort))
                    UpdateDbSetting(_remoteApiPort, "remote_api_port");

                if (!string.IsNullOrEmpty(_userPortRange))
                    UpdateDbSetting(_userPortRange, "user_port_range");

                if (!string.IsNullOrEmpty(_thumbPrint))
                    UpdateDbSetting(_thumbPrint, "ca_thumbprint");

                if (!string.IsNullOrEmpty(_logLevel))
                    UpdateDbSetting(_logLevel, "log_level");
            }

            catch (Exception ex)
            {
                _session.Log(ex.Message);
                return ActionResult.Failure;
            }

            //Service should have been killed already by msi, just need to start it again
            StartService();
            _session.Log("Finished Update/Reinstall Database Initialization");
            return ActionResult.Success;
        }

        private ActionResult NewSetup(bool isReset)
        {
            _isNewInstall = true;
            File.Copy(_basePath + "DBshell.db", _basePath + "Toec.db");

            //Create Log directory and give users permissions to write for the tray app log to work
            try
            {
                var path = Path.Combine(_basePath, "logs");
                Directory.CreateDirectory(path);

                var logSec1 = Directory.GetAccessControl(path);
                logSec1.SetAccessRuleProtection(true, true);
                Directory.SetAccessControl(path, logSec1);

                var logSec2 = Directory.GetAccessControl(path);
                var logSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                logSec2.AddAccessRule(new FileSystemAccessRule(logSid,FileSystemRights.Write,InheritanceFlags.ContainerInherit,PropagationFlags.None, AccessControlType.Allow));
                logSec2.AddAccessRule(new FileSystemAccessRule(logSid, FileSystemRights.Write, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(path,logSec2);

            }
            catch
            {
                //Ignored
            }

            //remove well known users read permissions from database, only leaving system and administrators
            var sec = File.GetAccessControl(_basePath + "Toec.db");
            sec.SetAccessRuleProtection(true, true);
            File.SetAccessControl(_basePath + "Toec.db", sec);

            var sec2 = File.GetAccessControl(_basePath + "Toec.db");
            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            sec2.PurgeAccessRules(sid);
            File.SetAccessControl(_basePath + "Toec.db", sec2);

            if (string.IsNullOrEmpty(_comServers) ||
                string.IsNullOrEmpty(_thumbPrint) || string.IsNullOrEmpty(_serverKey))
            {
                DisplayError(
                    "User Defined Variables Missing For New Setup.\r\nCOM_SERVERS, SERVER_KEY, CA_THUMBPRINT\r\n Must Be Defined From The Command Line.");
                RemoveDb();
                return ActionResult.Failure;
            }

            ValidateComServers();
            UpdateDbSetting(_clientVersion, "version");
            UpdateDbSetting(_comServers, "initial_com_servers");
            UpdateDbSetting(_thumbPrint, "ca_thumbprint");
            UpdateDbSetting(Guid.NewGuid().ToString(), "installation_id");

            //Optional Variables
            if (!string.IsNullOrEmpty(_localApiPort))
                UpdateDbSetting(_localApiPort, "local_api_port");

            if (!string.IsNullOrEmpty(_remoteApiPort))
                UpdateDbSetting(_remoteApiPort, "remote_api_port");

            if (!string.IsNullOrEmpty(_userPortRange))
                UpdateDbSetting(_userPortRange, "user_port_range");

            if (!string.IsNullOrEmpty(_logLevel))
                UpdateDbSetting(_logLevel, "log_level");

            var entropy = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(entropy);
            var serverKeyBytes = Encoding.ASCII.GetBytes(_serverKey);
            var encryptedKey = EncryptData(serverKeyBytes, true, entropy);
            UpdateDbSetting(Convert.ToBase64String(entropy), "server_key_entropy");
            UpdateDbSetting(Convert.ToBase64String(encryptedKey), "server_key");

            if (isReset)
            {
                //Service should have been killed already by msi, just need to start it again
                StartService();
            }

            _session.Log("Finished New Database Initialization");
            return ActionResult.Success;
        }

        private void RemoveDb()
        {
            File.Delete(_basePath + "Toec.db");
        }

        private void StartService()
        {
            var service = new ServiceController("Toec");
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                //ignored
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void UpdateDbSetting(string settingValue, string settingName)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText =
                        "UPDATE settings SET setting_value = @SettingValue WHERE setting_name = @SettingName;";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@SettingValue", settingValue);
                    cmd.Parameters.AddWithValue("@SettingName", settingName);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        DisplayError("Could Not Update Setting " + settingName);
                        _session.Log(ex.Message);
                        if (_isNewInstall)
                            RemoveDb();
                        throw;
                    }
                }
                conn.Close();
            }
        }

        private void ValidateComServers()
        {
            var list = new List<string>();
            foreach (var cs in _comServers.Split(','))
            {
                if (!cs.EndsWith("/"))
                    list.Add(cs + "/");
                else
                    list.Add(cs);
            }

            _comServers = "";
            foreach (var cs in list)
            {
                _comServers += cs + ",";
            }

            _comServers = _comServers.Trim(',');
        }

        private bool UpdateDb_1_0_1()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText =
                        "CREATE TABLE IF NOT EXISTS app_monitor ( app_monitor_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, application_name TEXT, application_path TEXT, start_date_time_utc TEXT, end_date_time_utc TEXT, username TEXT, pid INTEGER )";                  
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        _session.Log(ex.Message);
                        return false;
                    }
                }
                conn.Close();
            }
            return true;
        }
    }
}