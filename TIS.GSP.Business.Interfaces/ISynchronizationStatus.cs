using System;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Indicates the current state of a synchronization process.
  /// </summary>
  public enum SynchronizationState
  {
    /// <summary>
    /// The default value to use when the state is unknown or it is not relevant to specify.
    /// </summary>
    NotSet = 0,
    /// <summary>
    /// The synchronization is complete and there is no current activity.
    /// </summary>
    Complete = 1,
    /// <summary>
    /// Indicates the current user is performing a synchronization. During this state no changes will be
    /// persisted to the data store. The changes will be saved to the data store in the next state
    /// PersistingToDataStore.
    /// </summary>
    SynchronizingFiles = 2,
    /// <summary>
    /// Indicates the files have been synchronized and now the changes are being persisted to the data store.
    /// </summary>
    PersistingToDataStore = 3,
    /// <summary>
    /// An error occurred during the most recent synchronization.
    /// </summary>
    Error = 4,
    /// <summary>
    /// Indicates another synchronization is already in progress.
    /// </summary>
    AnotherSynchronizationInProgress = 5,
    /// <summary>
    /// Indicates the synchronization was canceled by the user.
    /// </summary>
    Aborted = 6,
    /// <summary>
    /// Indicates the synchronization was interrupted by the application domain recycling.
    /// </summary>
    InterruptedByAppRecycle = 7
  }

  /// <summary>
  /// Provides functionality for retrieving and storing the status of a synchronization.
  /// </summary>
  /// <remarks>This class is managed as a singleton on a per gallery instance, which means only once instance exists for 
  /// each gallery in the current app domain.</remarks>
  public interface ISynchronizationStatus
  {
    /// <summary>
    /// Gets the date and time of when the sync was started.
    /// </summary>
    /// <value>The date and time of when the sync was started.</value>
    DateTime BeginTimestampUtc { get; }

    /// <summary>
    /// Gets or sets the zero-based index value of the current file being processed. This is a number from 0 to <see cref="TotalFileCount"/> - 1.
    /// data store; only this instance is updated.
    /// </summary>
    /// <value>The zero-based index value of the current file being processed.</value>
    int CurrentFileIndex { get; }
    
    /// <summary>
    /// Gets the name of the current file being processed (e.g. DesertSun.jpg).
    /// </summary>
    /// <value>The name of the current file being processed (e.g. DesertSun.jpg).</value>
    string CurrentFileName { get; }
    
    /// <summary>
    /// Gets the path to the current file being processed. The path is relative to the media objects 
    /// directory. For example, if the media objects directory is C:\mypics\ and the file currently being processed is
    /// in C:\mypics\vacations\india\, this property is vacations\india\.
    /// </summary>
    /// <value>The path to the current file being processed, relative to the media objects directory (e.g. vacations\india\).</value>
    string CurrentFilePath { get; }

    /// <summary>
    /// Gets the status of the current synchronization. This property will never return
    /// <see cref="SynchronizationState.AnotherSynchronizationInProgress"/>. To find out if another
    /// synchronization is in progress, call the static Start method and catch
    /// GalleryServer.Events.CustomExceptions.SynchronizationInProgressException.
    /// </summary>
    /// <value>The status of the current synchronization.</value>
    SynchronizationState Status { get; }

    /// <summary>
    /// Gets a GUID that uniquely identifies the current synchronization.
    /// </summary>
    /// <value>The GUID that uniquely identifies the current synchronization.</value>
    string SynchId { get; }

    /// <summary>
    /// Gets the value that uniquely identifies the gallery.
    /// </summary>
    /// <value>The value that uniquely identifies the gallery.</value>
    int GalleryId { get; }

    /// <summary>
    /// Gets the total number of files in the directory or directories that are being processed in the current
    /// synchronization. The number includes all files, not just ones that Gallery Server recognizes as
    /// valid media objects.
    /// </summary>
    /// <value>The total number of files in the directory or directories that are being processed in the current
    /// synchronization.</value>
    int TotalFileCount { get; }

    /// <summary>
    /// Gets a list of all files that were encountered during the synchronization but were not added. The key contains
    /// the name of the file; the value contains the reason why the object was skipped. Guaranteed to not return null.
    /// </summary>
    /// <value>The list of all files that were encountered during the synchronization but were not added.</value>
    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>> SkippedMediaObjects { get; }

    /// <summary>
    /// Gets a value indicating whether the current synchronization should be terminated. This is typically set
    /// by code that is observing the synchronization, such as a progress indicator. This property is periodically
    /// queried by the code running the synchronization to discover if a cancellation has been requested, and
    /// subsequently carrying out the request if it has.
    /// </summary>
    /// <value><c>true</c> if the current synchronization should be terminated; otherwise, <c>false</c>.</value>
    bool ShouldTerminate { get; }

    /// <summary>
    /// Perform actions that are required at the start of a synchronization. This includes resetting the properties of the 
    /// current instance and setting the status to <see cref="SynchronizationState.SynchronizingFiles" />. The new values are
    /// persisted to the data store. Throws a SynchronizationInProgressException if another synchronization is already in progress.
    /// </summary>
    /// <param name="synchId">The GUID that uniquely identifies the synchronization.</param>
    void Begin(string synchId);

    /// <summary>
    /// Updates the current instance with the new values, persisting the changes to the data store is <paramref name="persistToDataStore" />
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
    void Update(SynchronizationState synchStatus, int? totalFileCount, string currentFileName, int? currentFileIndex, string currentFilePath, bool? shouldTerminate, bool persistToDataStore);

    /// <summary>
    /// Set a flag (<see cref="ShouldTerminate" />) to indicate the current synchronization should be canceled. This property is periodically
    /// queried by the code running the synchronization to discover if a cancellation has been requested, and subsequently carrying out 
    /// the request if it has.
    /// </summary>
    /// <param name="synchId">The GUID that uniquely identifies the synchronization. If the value does not match the synch ID
    /// of the current instance, no action is taken.</param>
    void CancelSynchronization(string synchId);

    /// <summary>
    /// Completes the current synchronization by updating the status instance and the Synchronize table in the
    /// data store. Calling this method is required before subsequent synchronizations can be performed.
    /// </summary>
    void Finish();
  }
}
