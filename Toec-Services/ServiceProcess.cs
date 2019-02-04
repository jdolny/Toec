using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using log4net;
using Toec_Common.Dto;

namespace Toec_Services
{
    public class ServiceProcess
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DtoProcessArgs _processArgs;
        private readonly StringBuilder _error;
        private readonly StringBuilder _output;

        public ServiceProcess(DtoProcessArgs processArgs)
        {
            _processArgs = processArgs;
            _output = new StringBuilder();
            _error = new StringBuilder();
        }

        private Process GenerateArgs()
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            if (!string.IsNullOrEmpty(_processArgs.WorkingDirectory))
                process.StartInfo.WorkingDirectory = _processArgs.WorkingDirectory;
            if (_processArgs.RedirectOutput)
                process.StartInfo.RedirectStandardOutput = true;
            if (_processArgs.RedirectError)
                process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = _processArgs.RunWith;
            process.StartInfo.Arguments = _processArgs.RunWithArgs + " " + _processArgs.Command + " " +
                                          _processArgs.Arguments;
            if (_processArgs.Timeout == 0)
                _processArgs.Timeout = 60;
            if (_processArgs.RedirectOutput)
                process.OutputDataReceived += (sender, args) => _output.AppendLine(args.Data);
            if (_processArgs.RedirectError)
                process.ErrorDataReceived += (sender, args) => _error.AppendLine(args.Data);
            return process;
        }

        public DtoProcessResult RunProcess()
        {
            var result = new DtoProcessResult();

            using (var p = GenerateArgs())
            {
                Logger.Debug("Starting Command: " + p.StartInfo.FileName + " " + p.StartInfo.Arguments);
                try
                {
                    p.Start();

                    if (_processArgs.RedirectOutput)
                        p.BeginOutputReadLine();
                    if (_processArgs.RedirectError)
                        p.BeginErrorReadLine();

                    p.WaitForExit(_processArgs.Timeout*1000*60);
                    if (p.HasExited)
                    {
                        result.ExitCode = p.ExitCode;
                    }
                    else
                    {
                        var error = "Process has exceeded the timeout value";
                        Logger.Debug(error);
                        result.StandardError = error;
                        p.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception Occurred While Executing Process");
                    Logger.Error(ex.Message);
                    result.StandardError = ex.Message;
                }
            }

            result.StandardOut += _output.ToString();
            result.StandardError += _error.ToString();
            return result;
        }
    }
}