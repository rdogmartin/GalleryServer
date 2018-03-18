using System;
using System.Collections.Generic;
using System.Globalization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Provides functionality for retrieving and storing the status of a synchronization.
  /// </summary>
  /// <remarks>This class is managed as a singleton on a per gallery instance, which means only once instance exists for 
  /// each gallery in the current app domain.</remarks>
  public class SynchronizationStatus : ISynchronizationStatus
  {
    #region Private fields

    private readonly object _lockObject = new object();

    private readonly int _galleryId;
    private string _synchId;
    private int _totalFileCount;
    private int _currentFileIndex;
    private readonly List<KeyValuePair<string, string>> _skippedMediaObjects = new List<KeyValuePair<string, string>>();
    private bool _shouldTerminate;
    private SynchronizationState _synchState;
    private string _currentFileName;
    private string _currentFilePath;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizationStatus"/> class for the specified <paramref name="galleryId" />.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    internal SynchronizationStatus(int galleryId)
      : this(galleryId, String.Empty, SynchronizationState.Complete, 0, String.Empty, 0, String.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizationStatus"/> class with the specified properties.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <param name="synchId">The GUID that uniquely identifies the current synchronization.</param>
    /// <param name="synchStatus">The status of the current synchronization.</param>
    /// <param name="totalFileCount">The total number of files in the directory or directories that are being processed in the current
    /// synchronization.</param>
    /// <param name="currentFileName">The name of the current file being processed.</param>
    /// <param name="currentFileIndex">The zero-based index value of the current file being processed. This is a number from 0 to 
    /// <see cref="TotalFileCount"/> - 1.</param>
    /// <param name="currentFilePath">The path to the current file being processed.</param>
    internal SynchronizationStatus(int galleryId, string synchId, SynchronizationState synchStatus, int totalFileCount, string currentFileName, int currentFileIndex, string currentFilePath)
    {
      this._galleryId = galleryId;
      this._synchId = synchId;
      this._synchState = synchStatus;
      this._totalFileCount = totalFileCount;
      this._currentFileName = currentFileName;
      this._currentFileIndex = currentFileIndex;
      this._currentFilePath = currentFilePath;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets a GUID that uniquely identifies the current synchronization.
    /// </summary>
    /// <value>The GUID that uniquely identifies the current synchronization.</value>
    public String SynchId
    {
      get
      {
        return this._synchId;
      }
    }

    /// <summary>
    /// Gets the value that uniquely identifies the gallery.
    /// </summary>
    /// <value>The value that uniquely identifies the gallery.</value>
    public int GalleryId
    {
      get
      {
        return this._galleryId;
      }
    }

    /// <summary>
    /// Gets the total number of files in the directory or directories that are being processed in the current
    /// synchronization. The number includes all files, not just ones that Gallery Server recognizes as
    /// valid media objects.
    /// </summary>
    /// <value>
    /// The total number of files in the directory or directories that are being processed in the current
    /// synchronization.
    /// </value>
    public int TotalFileCount
    {
      get
      {
        return this._totalFileCount;
      }
    }

    /// <summary>
    /// Gets the date and time of when the sync was started.
    /// </summary>
    /// <value>The date and time of when the sync was started.</value>
    public DateTime BeginTimestampUtc { get; private set; }

    /// <summary>
    /// Gets or sets the zero-based index value of the current file being processed. This is a number from 0 to <see cref="TotalFileCount"/> - 1.
    /// data store; only this instance is updated.
    /// </summary>
    /// <value>The zero-based index value of the current file being processed.</value>
    public int CurrentFileIndex
    {
      get
      {
        return this._currentFileIndex;
      }
    }

    /// <summary>
    /// Gets the name of the current file being processed (e.g. DesertSun.jpg).
    /// </summary>
    /// <value>
    /// The name of the current file being processed (e.g. DesertSun.jpg).
    /// </value>
    public string CurrentFileName
    {
      get
      {
        return this._currentFileName;
      }
    }

    /// <summary>
    /// Gets the path to the current file being processed. The path is relative to the media objects
    /// directory. For example, if the media objects directory is C:\mypics\ and the file currently being processed is
    /// in C:\mypics\vacations\india\, this property is vacations\india\.
    /// </summary>
    /// <value>
    /// The path to the current file being processed, relative to the media objects directory (e.g. vacations\india\).
    /// </value>
    public string CurrentFilePath
    {
      get
      {
        return this._currentFilePath;
      }
    }

    /// <summary>
    /// Gets a list of all files that were encountered during the synchronization but were not added. The key contains
    /// the name of the file; the value contains the reason why the object was skipped. Guaranteed to not return null.
    /// </summary>
    /// <value>
    /// The list of all files that were encountered during the synchronization but were not added.
    /// </value>
    public List<KeyValuePair<string, string>> SkippedMediaObjects
    {
      get
      {
        return this._skippedMediaObjects;
      }
    }

    /// <summary>
    /// Gets the status of the current synchronization. This property will never return
    /// <see cref="SynchronizationState.AnotherSynchronizationInProgress"/>. To find out if another
    /// synchronization is in progress, call the static Start method and catch
    /// <see cref="SynchronizationInProgressException" />.
    /// </summary>
    /// <value>The status of the current synchronization.</value>
    public SynchronizationState Status
    {
      get
      {
        return this._synchState;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the current synchronization should be terminated. This is typically set
    /// by code that is observing the synchronization, such as a progress indicator. This property is periodically
    /// queried by the code running the synchronization to discover if a cancellation has been requested, and
    /// subsequently carrying out the request if it has.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the current synchronization should be terminated; otherwise, <c>false</c>.
    /// </value>
    public bool ShouldTerminate
    {
      get
      {
        return this._shouldTerminate;
      }
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets a reference to the <see cref="ISynchronizationStatus" /> instance for the specified 
    /// <paramref name="galleryId">gallery ID</paramref>.
    /// </summary>
    /// <param name="galleryId">The ID of the gallery whose synchronization status you want to retrieve.</param>
    /// <returns>Gets a reference to the <see cref="ISynchronizationStatus" /> instance for the specified 
    /// <paramref name="galleryId">gallery ID</paramref>.</returns>
    public static ISynchronizationStatus GetInstance(int galleryId)
    {
      return Factory.LoadSynchronizationStatus(galleryId);
    }

    /// <summary>
    /// Begins the process of a new synchronization by updating the status object and the Synchronize table in the
    /// data store. Throws an exception if another synchronization is already in process.
    /// </summary>
    /// <param name="synchId">A GUID string that uniquely identifies the current synchronization.</param>
    /// <param name="galleryId">The ID of the gallery to be synchronized.</param>
    /// <returns>Returns an instance of <see cref="ISynchronizationStatus" /> representing the current synchronization.</returns>
    /// <exception cref="SynchronizationInProgressException">
    /// Thrown when a synchronization with another synchId is already in progress.</exception>
    public static ISynchronizationStatus Start(string synchId, int galleryId)
    {
      ISynchronizationStatus syncStatus = GetInstance(galleryId);

      syncStatus.Begin(synchId);

      return syncStatus;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Perform actions that are required at the start of a synchronization. This includes resetting the properties of the
    /// current instance and setting the status to <see cref="SynchronizationState.SynchronizingFiles"/>. The new values are
    /// persisted to the data store. Throws a SynchronizationInProgressException if another synchronization is already in progress.
    /// </summary>
    /// <param name="synchId">The GUID that uniquely identifies the synchronization.</param>
    public void Begin(string synchId)
    {
      lock (_lockObject)
      {
        var syncInProgress = (_synchState == SynchronizationState.SynchronizingFiles || _synchState == SynchronizationState.PersistingToDataStore);

        if ((synchId != _synchId) && syncInProgress)
        {
          throw new Events.CustomExceptions.SynchronizationInProgressException();
        }

        BeginTimestampUtc = DateTime.UtcNow;
        _synchId = synchId;
        _totalFileCount = 0;
        _currentFileIndex = 0;
        _synchState = SynchronizationState.SynchronizingFiles;
        _skippedMediaObjects.Clear();

        // Save to data store. Even though it might have been valid to start the synchronizing above, by the time
        // we try to save to the data store, someone else may have started it (for example, from another application).
        //  So the data provider will check one more time just before saving our data, throwing an exception if necessary.
        Save();
      }
    }

    /// <summary>
    /// Updates the current instance with the new values, persisting the changes to the data store if <paramref name="persistToDataStore"/>
    /// is <c>true</c>. Specify a null value for each parameter that should not be updated (the existing value will be retained).
    /// </summary>
    /// <param name="synchStatus">The status of the current synchronization.</param>
    /// <param name="totalFileCount">The total number of files in the directory or directories that are being processed in the current
    /// synchronization.</param>
    /// <param name="currentFileName">The name of the current file being processed.</param>
    /// <param name="currentFileIndex">The zero-based index value of the current file being processed. This is a number from 0 to
    /// <see cref="TotalFileCount"/> - 1.</param>
    /// <param name="currentFilePath">The path to the current file being processed.</param>
    /// <param name="shouldTerminate">Indicates whether the current synchronization should be terminated.</param>
    /// <param name="persistToDataStore">If set to <c>true</c> persist the new values to the data store.</param>
    public void Update(SynchronizationState synchStatus, int? totalFileCount, string currentFileName, int? currentFileIndex, string currentFilePath, bool? shouldTerminate, bool persistToDataStore)
    {
      lock (_lockObject)
      {
        if (synchStatus != SynchronizationState.NotSet)
        {
          this._synchState = synchStatus;
        }

        if (totalFileCount.HasValue)
        {
          if (totalFileCount.Value < 0)
          {
            throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, Resources.SynchronizationStatus_TotalFileCount_Ex_Msg, totalFileCount.Value));
          }

          this._totalFileCount = totalFileCount.Value;
        }

        if (currentFileName != null)
        {
          this._currentFileName = currentFileName;
        }

        if (currentFileIndex.HasValue)
        {
          if ((currentFileIndex.Value < 0) || ((currentFileIndex.Value > 0) && (currentFileIndex.Value >= this._totalFileCount)))
          {
            throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, Resources.SynchronizationStatus_CurrentFileIndex_Ex_Msg, currentFileIndex.Value, this._totalFileCount));
          }

          this._currentFileIndex = currentFileIndex.Value;
        }

        if (currentFilePath != null)
        {
          this._currentFilePath = currentFilePath;
        }

        if (shouldTerminate.HasValue)
        {
          this._shouldTerminate = shouldTerminate.Value;
        }

        if (persistToDataStore)
        {
          Save();
        }
      }
    }

    /// <summary>
    /// Set a flag (<see cref="ShouldTerminate"/>) to indicate the current synchronization should be canceled. This property is periodically
    /// queried by the code running the synchronization to discover if a cancellation has been requested, and subsequently carrying out
    /// the request if it has.
    /// </summary>
    /// <param name="synchId">The GUID that uniquely identifies the synchronization. If the value does not match the sync ID
    /// of the current instance, no action is taken.</param>
    public void CancelSynchronization(string synchId)
    {
      lock (_lockObject)
      {
        if (this._synchId.Equals(synchId))
        {
          this._shouldTerminate = true;
        }
      }
    }

    /// <summary>
    /// Completes the current synchronization by updating the status instance and the Synchronize table in the
    /// data store. Calling this method is required before subsequent synchronizations can be performed.
    /// </summary>
    public void Finish()
    {
      lock (_lockObject)
      {
        // Updates database to show synchronization is no longer occuring.
        // Should be called when synchronization is finished.
        this._synchState = SynchronizationState.Complete;

        // Don't reset the file counts in case the UI wants to know how many files were processed.
        //this._currentFileIndex = 0;
        //this._totalFileCount = 0;

        Save();
      }
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// Persist the current state of this instance to the data store.
    /// </summary>
    private void Save()
    {
      lock (_lockObject)
      {
        //Factory.GetDataProvider().Synchronize_SaveStatus(this);
        using (var repo = new SynchronizeRepository())
        {
          var sDto = repo.Find(GalleryId);

          if (sDto != null)
          {
            if ((sDto.SynchId != SynchId) && ((sDto.SynchState == SynchronizationState.SynchronizingFiles) || (sDto.SynchState == SynchronizationState.PersistingToDataStore)))
            {
              throw new Events.CustomExceptions.SynchronizationInProgressException();
            }
            else
            {
              sDto.SynchId = SynchId;
              sDto.SynchState = Status;
              sDto.TotalFiles = TotalFileCount;
              sDto.CurrentFileIndex = CurrentFileIndex;
            }
          }
          else
          {
            sDto = new SynchronizeDto
            {
              SynchId = SynchId,
              FKGalleryId = GalleryId,
              SynchState = Status,
              TotalFiles = TotalFileCount,
              CurrentFileIndex = CurrentFileIndex
            };

            repo.Add(sDto);
          }

          repo.Save();
        }
      }
    }

    #endregion
  }
}
