using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LenovoBatteryTray.Utilities;

namespace LenovoBatteryTray.Battery
{
    public sealed class LenovoVantagePathResolver
    {
        private const string AddinRelativePath = @"Lenovo\Vantage\Addins\IdeaNotebookAddin";
        private const string ContractDllName = "BatteryManagementContract.dll";
        private const string AgentDllName = "IdeaBatteryAgent.dll";

        public string Resolve()
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                AddinRelativePath);

            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException(LocalizationManager.Text("Error.LenovoDllsNotFound"));
            }

            var candidates = Directory.GetDirectories(root)
                .Where(HasRequiredDlls)
                .Select(CreateCandidate)
                .ToList();

            if (candidates.Count == 0)
            {
                throw new DirectoryNotFoundException(LocalizationManager.Text("Error.LenovoDllsNotFound"));
            }

            Candidate selected;
            var versionedCandidates = candidates.Where(candidate => candidate.Version != null).ToList();

            if (versionedCandidates.Count > 0)
            {
                selected = versionedCandidates
                    .OrderByDescending(candidate => candidate.Version)
                    .ThenByDescending(candidate => candidate.LastWriteTimeUtc)
                    .First();
            }
            else
            {
                selected = candidates
                    .OrderByDescending(candidate => candidate.LastWriteTimeUtc)
                    .First();
            }

            AppLogger.Info("Lenovo addin directory resolved: " + selected.Path);
            return selected.Path;
        }

        private static bool HasRequiredDlls(string directory)
        {
            return File.Exists(Path.Combine(directory, ContractDllName))
                && File.Exists(Path.Combine(directory, AgentDllName));
        }

        private static Candidate CreateCandidate(string path)
        {
            Version version;
            Version.TryParse(Path.GetFileName(path), out version);

            return new Candidate
            {
                Path = path,
                Version = version,
                LastWriteTimeUtc = Directory.GetLastWriteTimeUtc(path)
            };
        }

        private sealed class Candidate
        {
            public string Path { get; set; }

            public Version Version { get; set; }

            public DateTime LastWriteTimeUtc { get; set; }
        }
    }
}
