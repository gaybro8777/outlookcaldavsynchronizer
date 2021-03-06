﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.IntegrationTests
{
  public class TestOptionsFactory
  {
    private const string AppointmantFolderName = "IntegrationTestCalendar";
    private const string TaskFolderName = "IntegrationTestTasks";
    private const string ContactFolderName = "IntegrationTestContacts";

    private readonly IOutlookSession _session;
    private readonly IOptionsDataAccess _optionsDataAccess;
    

    public TestOptionsFactory(IOutlookSession session, IOptionsDataAccess optionsDataAccess)
    {
      _optionsDataAccess = optionsDataAccess ?? throw new ArgumentNullException(nameof(optionsDataAccess));
      _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public Options CreateSogoEvents()
    {
      var options = CreateDefaultOptions("IntegrationTest/Events/Sogo", AppointmantFolderName);
      options.MappingConfiguration = CreateDefaultEventMappingConfiguration();
      return options;
    }

    public Options CreateGoogleEvents()
    {
      var options = CreateDefaultOptions("IntegrationTest/Events/Google", AppointmantFolderName);
      options.MappingConfiguration = CreateDefaultEventMappingConfiguration();
      return options;
    }

    public Options CreateSogoContacts()
    {
      var options = CreateDefaultOptions("IntegrationTests/Contacts/Sogo", ContactFolderName);
      options.MappingConfiguration = CreateDefaultContactMappingConfiguration();
      return options;
    }

    public Options CreateGoogleContacts()
    {
      var options = CreateDefaultOptions("IntegrationTests/Contacts/Google", ContactFolderName);
      options.MappingConfiguration = CreateDefaultContactMappingConfiguration();
      return options;
    }

    public Options CreateSogoTasks()
    {
      var options = CreateDefaultOptions("IntegrationTest/Tasks/Sogo", TaskFolderName);
      options.MappingConfiguration = CreateDefaultTaskMappingConfiguration();
      return options;
    }

    public Options CreateGoogleTasks()
    {
      var options = CreateDefaultOptions("IntegrationTest/Tasks/Google", TaskFolderName);
      options.MappingConfiguration = CreateDefaultTaskMappingConfiguration();
      return options;
    }

    public Options CreateLocalFolderEvents() => CreateLocalFolder(AppointmantFolderName);

    private Options CreateLocalFolder(string outlookFolderName)
    {
      const string folderPath = @"D:\temp\IntegrationTests\Entities\";
      if (!Directory.Exists(folderPath))
      {
        Directory.CreateDirectory(folderPath);
      }
      else
      {
        foreach (var file in Directory.GetFiles(folderPath))
          File.Delete(file);
      }
      var options = CreateDefaultOptions(
        new Options() { CalenderUrl = $"file://{folderPath}" },
        outlookFolderName);
      options.MappingConfiguration = CreateDefaultEventMappingConfiguration();
      return options;
    }

    private static EventMappingConfiguration CreateDefaultEventMappingConfiguration()
    {
      return new EventMappingConfiguration();
    }

    private static MappingConfigurationBase CreateDefaultContactMappingConfiguration()
    {
      return new ContactMappingConfiguration();
    }

    private static MappingConfigurationBase CreateDefaultTaskMappingConfiguration()
    {
      return new TaskMappingConfiguration();
    }

    private Options CreateDefaultOptions(string profileName, string outlookFolderName)
    {
      var options = _optionsDataAccess.Load().Single(o => o.Name == profileName);
      return CreateDefaultOptions(options, outlookFolderName);
    }

    private Options CreateDefaultOptions(Options optionsWithConnectionData, string outlookFolderName)
    {
      var outlookFolder = _session.GetFoldersByName().GetOrDefault(outlookFolderName)?.SingleOrDefault() ?? throw new System.Exception($"Didn't find single folder {outlookFolderName}");

      return new Options
      {
        ProtectedPassword = optionsWithConnectionData.ProtectedPassword,
        Salt = optionsWithConnectionData.Salt,
        UserName = optionsWithConnectionData.UserName,
        CalenderUrl = optionsWithConnectionData.CalenderUrl,
        EmailAddress = optionsWithConnectionData.EmailAddress,
        ServerAdapterType = optionsWithConnectionData.ServerAdapterType,
        ForceBasicAuthentication = optionsWithConnectionData.ForceBasicAuthentication,
        CloseAfterEachRequest = optionsWithConnectionData.CloseAfterEachRequest,
        PreemptiveAuthentication = optionsWithConnectionData.PreemptiveAuthentication,

        OutlookFolderEntryId = outlookFolder.EntryId,
        OutlookFolderStoreId = outlookFolder.StoreId,

        IsChunkedSynchronizationEnabled = false,
        ChunkSize = 100,
        IgnoreSynchronizationTimeRange = true,
        SynchronizationMode = Implementation.SynchronizationMode.MergeInBothDirections,
        ConflictResolution = Implementation.ConflictResolution.Automatic,
        SynchronizationIntervalInMinutes = 0,
      };
    }
  }
}
