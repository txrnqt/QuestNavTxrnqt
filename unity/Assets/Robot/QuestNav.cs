using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkTables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Robot
{
    /// <summary>
    /// Extension methods for Unity's Vector3 class to convert to array format.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a Vector3 to a float array containing x, y, and z components.
        /// </summary>
        /// <param name="vector">The Vector3 to convert</param>
        /// <returns>Float array containing [x, y, z] values</returns>
        public static float[] ToArray(this Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }
    }

    /// <summary>
    /// Extension methods for Unity's Quaternion class to convert to array format.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Converts a Quaternion to a float array containing x, y, z, and w components.
        /// </summary>
        /// <param name="quaternion">The Quaternion to convert</param>
        /// <returns>Float array containing [x, y, z, w] values</returns>
        public static float[] ToArray(this Quaternion quaternion)
        {
            return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }
    }

    /// <summary>
    /// Manages streaming of VR motion data to a FRC robot through NetworkTables.
    /// Handles pose tracking, reset operations, and network communication.
    /// </summary>
    public class QuestNav : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// Current frame index from Unity's Time.frameCount
        /// </summary>
        public int frameIndex;

        /// <summary>
        /// Current timestamp from Unity's Time.time
        /// </summary>
        public double timeStamp;

        /// <summary>
        /// Current position of the VR headset
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Current rotation of the VR headset as a Quaternion
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// Current rotation of the VR headset in Euler angles
        /// </summary>
        public Vector3 eulerAngles;

        /// <summary>
        /// Reference to the OVR Camera Rig for tracking
        /// </summary>
        public OVRCameraRig cameraRig;

        /// <summary>
        /// NetworkTables connection for FRC data communication
        /// </summary>
        public Nt4Source frcDataSink = null;

        /// <summary>
        /// Current command received from the robot
        /// </summary>
        private long command = 0;

        /// <summary>
        /// Flag indicating if a reset operation is in progress
        /// </summary>
        private bool resetInProgress = false;

        /// <summary>
        /// Input field for team number entry
        /// </summary>
        public TMP_InputField teamInput;

        // <summary>
        /// IP address text
        /// </summary>
        public TMP_Text ipAddressText;

        // <summary>
        /// ConState text
        /// </summary>
        public TMP_Text conStateText;

        /// <summary>
        /// Current log message
        /// </summary>
        private string conStateMessage = "No Message";

        /// <summary>
        /// Button to update team number
        /// </summary>
        public Button teamUpdateButton;

        /// <summary>
        /// Default team number text
        /// </summary>
        public static string inputText = "9999";

        /// <summary>
        /// Current team number
        /// </summary>
        private string teamNumber = "";

        /// <summary>
        /// Reference to the VR camera transform
        /// </summary>
        [SerializeField] 
        public Transform vrCamera;

        /// <summary>
        /// Reference to the VR camera root transform
        /// </summary>
        [SerializeField] 
        public Transform vrCameraRoot;

        /// <summary>
        /// Reference to the reset position transform
        /// </summary>
        [SerializeField] 
        public Transform resetTransform;

        /// <summary>
        /// Current battery percentage of the device
        /// </summary>
        private float batteryPercent = 0;

        #region NetworkTables Configuration
        /// <summary>
        /// Application name for NetworkTables connection
        /// </summary>
        private readonly string appName = "QuestNav";

        /// <summary>
        /// Server address format for NetworkTables connection
        /// </summary>
        private readonly string serverAddress = "10.TE.AM.2";

        /// <summary>
        /// Current IP address for connection
        /// </summary>
        private string ipAddress = "";

        /// <summary>
        /// NetworkTables server port
        /// </summary>
        private readonly int serverPort = 5810;

        /// <summary>
        /// Counter for connection retry delay
        /// </summary>
        private int delayCounter = 0;

        /// <summary>
        /// Quest display frequency (in Hz)
        /// </summary>
        private float displayFrequency = 120.0f;

        /// <summary>
        /// Holds the detected local IP address of the HMD
        /// </summary>
        private string myAddressLocal = "0.0.0.0";

        /// <summary>
        /// Default reconnect delay for failed connection attempts
        /// </summary>
        private float defaultReconnectDelay = 3.0f; //seconds

        /// <summary>
        /// Reconnection delay variable for delaying failed connection attempts.
        /// This value changes dynamically based on the number of failed attempts.
        /// </summary>
        private float reconnectDelay = 3.0f; //seconds

        /// <summary>
        /// Maximum reconnection delay (rate limited to prevent excessive retries lagging the main Unity thread)
        /// </summary>
        private const float maxReconnectDelay = 10.0f; //seconds

        /// <summary>
        /// List of failed connection attempt candidates (prevents excessive retries on failed addresses)
        /// </summary>
        private Dictionary<string, float> failedCandidates = new Dictionary<string, float>();

        /// <summary>
        /// Unreachable network delay
        /// </summary>
        private int unreachableNetworkDelay = 5; // seconds

        /// <summary>
        /// Cooldown before trying candidates that have failed previously
        /// </summary>
        private const float candidateFailureCooldown = 10.0f; // seconds

        /// <summary>
        /// Coroutine definition to enable controlling the connection process
        /// </summary>
        private Coroutine _connectionCoroutine;

        /// <summary>
        /// used to only set up the coroutine once in async
        /// </summary>
        private bool connectionAttempt = false;

        /// <summary>
        /// used to make sure connection completed on coroutine
        /// </summary>
        private bool connectionAttemptCompleted = true;

        /// <summary>
        /// Timestamp when the connection attempt started for timeout calculation
        /// </summary>
        private float connectionAttemptStartTime = 0;

        /// <summary>
        /// Maximum time to wait for a connection attempt before resetting state
        /// </summary>
        private const float CONNECTION_ATTEMPT_TIMEOUT = 10.0f; // seconds

        #endregion

        #region Heartbeat System
        /// <summary>
        /// Heartbeat counter value that increments with each successful exchange
        /// </summary>
        private int heartbeatCounter = 1;

        /// <summary>
        /// Time when the last heartbeat was sent to the robot
        /// </summary>
        private float lastHeartbeatSentTime = 0;

        /// <summary>
        /// Time when the last successful heartbeat response was received
        /// </summary>
        private float lastHeartbeatResponseTime = 0;

        /// <summary>
        /// Flag indicating whether we're waiting for a heartbeat response
        /// </summary>
        private bool heartbeatResponsePending = false;

        /// <summary>
        /// Counter for consecutive failed heartbeats
        /// </summary>
        private int consecutiveFailedHeartbeats = 0;

        /// <summary>
        /// Maximum number of consecutive heartbeat failures before forcing reconnection
        /// </summary>
        private const int MAX_FAILED_HEARTBEATS = 3;

        /// <summary>
        /// Time interval between heartbeat checks (seconds)
        /// </summary>
        private const float HEARTBEAT_INTERVAL = 1.0f;

        /// <summary>
        /// Maximum time to wait for a heartbeat response before considering it failed (seconds)
        /// </summary>
        private const float HEARTBEAT_TIMEOUT = 3.0f;
        #endregion

        #region Unity Lifecycle Methods
        /// <summary>
        /// Initializes the connection and UI components
        /// </summary>
        void Start()
        {
            OVRPlugin.systemDisplayFrequency = displayFrequency;
            teamNumber = PlayerPrefs.GetString("TeamNumber", "9999");
            setInputBox(teamNumber);
            teamInput.Select();
            UpdateIPAddressText();
            UpdateConStateText();
            ConnectToRobot();
            teamUpdateButton.onClick.AddListener(UpdateTeamNumber);
            teamInput.onSelect.AddListener(OnInputFieldSelected);
        }

        /// <summary>
        /// Handles frame updates for data publishing and command processing
        /// </summary>
        void LateUpdate()
        {
            // Check for connection attempt timeout to prevent zombie state
            if (connectionAttempt && (Time.time - connectionAttemptStartTime > CONNECTION_ATTEMPT_TIMEOUT)) {
                QueuedLogger.LogWarning("[QuestNav] Connection attempt timed out, resetting state");
                connectionAttempt = false;
                conStateMessage = "Connection attempt timed out, retry enabled";
                UpdateConStateText();
            }

            // If the connection isn't ready, attempt to reconnect
            if (frcDataSink == null || !frcDataSink.Client.Connected())
            {
                conStateMessage = frcDataSink == null ? "datasink null" : "not connected";
                UpdateConStateText();

                // Only start a new connection attempt if not already trying
                if (!connectionAttempt)
                {
                    conStateMessage = "initiating reconnection";
                    UpdateConStateText();
                    HandleDisconnectedState();
                }
                else
                {
                    conStateMessage = "connection attempt in progress";
                    UpdateConStateText();
                }
                return;
            }

            // Connected - manage heartbeat, publish data, and process commands
            if (frcDataSink.Client.Connected())
            {
                conStateMessage = "connected - sending data";
                
                // Manage heartbeat to detect zombie connections
                ManageHeartbeat();
                
                // Regular data processing
                PublishFrameData();
                ProcessCommands();
            }

            // Update UI periodically
            if (delayCounter >= (int)displayFrequency)
            {
                UpdateIPAddressText();
                UpdateConStateText();
                delayCounter = 0;
            }
            else
            {
                delayCounter++;
            }
        }
        #endregion
        #endregion

        #region Heartbeat System Methods
        /// <summary>
        /// Manages the heartbeat system to detect zombie connections.
        /// Called in LateUpdate when connection is established.
        /// </summary>
        private void ManageHeartbeat()
        {
            // Only run if connected
            if (frcDataSink == null || !frcDataSink.Client.Connected()) return;
            
            float currentTime = Time.time;
            
            // Check if previous heartbeat timed out
            if (heartbeatResponsePending)
            {
                if (currentTime - lastHeartbeatSentTime > HEARTBEAT_TIMEOUT)
                {
                    // Heartbeat timed out - increment failure counter
                    consecutiveFailedHeartbeats++;
                    heartbeatResponsePending = false;
                    QueuedLogger.LogWarning($"[QuestNav] Heartbeat #{heartbeatCounter} timed out. Failed count: {consecutiveFailedHeartbeats}");
                    
                    // Force reconnection if too many consecutive failures
                    if (consecutiveFailedHeartbeats >= MAX_FAILED_HEARTBEATS)
                    {
                        QueuedLogger.LogWarning("[QuestNav] Too many failed heartbeats, forcing reconnection");
                        ForceReconnection();
                        return;
                    }
                }
                else
                {
                    // Check for heartbeat response if still waiting
                    CheckHeartbeatResponse();
                }
            }
            
            // Send new heartbeat if interval elapsed and not waiting for response
            if (!heartbeatResponsePending && currentTime - lastHeartbeatSentTime > HEARTBEAT_INTERVAL)
            {
                SendHeartbeat();
            }
        }

        /// <summary>
        /// Sends a heartbeat value to the robot and starts waiting for response
        /// </summary>
        private void SendHeartbeat()
        {
            try
            {
                frcDataSink.PublishValue("/questnav/heartbeat/quest_to_robot", (double)heartbeatCounter);
                lastHeartbeatSentTime = Time.time;
                heartbeatResponsePending = true;
                QueuedLogger.Log($"[QuestNav] Sent heartbeat #{heartbeatCounter}");
            }
            catch (Exception ex)
            {
                QueuedLogger.LogWarning($"[QuestNav] Error sending heartbeat: {ex.Message}");
                // Don't set heartbeatResponsePending to true if we couldn't send
            }
        }

        /// <summary>
        /// Checks for a heartbeat response from the robot
        /// </summary>
        private void CheckHeartbeatResponse()
        {
            try
            {
                double response = frcDataSink.GetDouble("/questnav/heartbeat/robot_to_quest");
                
                if ((int)response == heartbeatCounter)
                {
                    // Valid response received
                    heartbeatResponsePending = false;
                    lastHeartbeatResponseTime = Time.time;
                    consecutiveFailedHeartbeats = 0;
                    QueuedLogger.Log($"[QuestNav] Received heartbeat response #{heartbeatCounter}");
                    
                    // Increment heartbeat counter for next round (with overflow protection)
                    heartbeatCounter++;
                    if (heartbeatCounter > 1000000) heartbeatCounter = 1;
                }
            }
            catch (Exception ex)
            {
                QueuedLogger.LogWarning($"[QuestNav] Error checking heartbeat response: {ex.Message}");
                // Don't reset heartbeatResponsePending here - let the timeout handle it
            }
        }

        /// <summary>
        /// Forces a reconnection due to heartbeat failure
        /// Resets connection state and begins a new connection attempt
        /// </summary>
        private void ForceReconnection()
        {
            QueuedLogger.Log("[QuestNav] Forcing reconnection due to heartbeat failure");
            
            // Reset heartbeat state
            heartbeatResponsePending = false;
            consecutiveFailedHeartbeats = 0;
            
            // Disconnect existing connection
            if (frcDataSink != null)
            {
                try
                {
                    frcDataSink.Client.Disconnect();
                }
                catch (Exception ex)
                {
                    QueuedLogger.LogWarning($"[QuestNav] Error disconnecting: {ex.Message}");
                }
                frcDataSink = null;
            }
            
            // Reset connection state flags to enable reconnection
            connectionAttempt = false;
            connectionAttemptCompleted = true;
            
            // Initiate reconnection
            ConnectToRobot();
        }
        #endregion

        #region Network Connection Methods
        /// <summary>
        /// Called to start the connection process.
        /// Manages connection flags and starts the coroutine for connection.
        /// </summary>
        private void ConnectToRobot()
        {
            // Don't start multiple connection attempts
            if (connectionAttempt)
            {
                QueuedLogger.LogWarning("[QuestNav] Connection attempt already in progress, skipping new request");
                return;
            }
            
            // Set connection flags and timestamp for timeout tracking
            connectionAttempt = true;
            connectionAttemptCompleted = false;
            connectionAttemptStartTime = Time.time;
            
            // Cancel any existing connection coroutine
            if (_connectionCoroutine != null)
            {
                StopCoroutine(_connectionCoroutine);
            }
            
            // Start a new connection attempt
            _connectionCoroutine = StartCoroutine(ConnectionCoroutineWrapper());
        }

        /// <summary>
        /// A coroutine wrapper that waits until the asynchronous connection method completes.
        /// This wrapper enables setting timeout guardbands for the connection process.
        /// </summary>
        private IEnumerator ConnectionCoroutineWrapper()
        {
            var connectionTask = AttemptConnectionAsync();
            
            while (!connectionTask.IsCompleted)
            {
                yield return null;
            }
            
            // Check if the task completed successfully
            if (connectionTask.IsFaulted)
            {
                QueuedLogger.LogError($"[QuestNav] Connection attempt failed with exception: {connectionTask.Exception}");
                connectionAttempt = false;  // Reset flag to allow new attempts
            }
        }

        /// <summary>
        /// Asynchronously attempts to connect to one of the candidate addresses.
        /// Uses async DNS resolution and wraps blocking calls in Task.Run to avoid blocking the main Unity thread.
        /// Will try candidates until successful or all fail, then retry with exponential backoff.
        /// </summary>
        private async Task AttemptConnectionAsync()
        {
            bool connectionEstablished = false;
            List<string> candidateAddresses = new List<string>()
            {
                generateIP(),
                "172.22.11.2",
                $"roboRIO-{teamNumber}-FRC.local",
                $"roboRIO-{teamNumber}-FRC.lan",
                $"roboRIO-{teamNumber}-FRC.frc-field.local"
            };

            while (!connectionEstablished)
            {
                // Check if the network is reachable.
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    QueuedLogger.LogWarning($"[QuestNav] Network not reachable. Waiting {unreachableNetworkDelay} seconds before reattempting.");
                    conStateMessage = "net no reach waiting";
                    await Task.Delay(unreachableNetworkDelay);
                    continue;
                }

                StringBuilder cycleLog = new StringBuilder();
                cycleLog.AppendLine("[QuestNav] Connection attempt cycle:");
                conStateMessage = "connection attempt cycle";

                foreach (string candidate in candidateAddresses)
                {
                    // Skip a candidate if it failed recently.
                    if (failedCandidates.ContainsKey(candidate) && (Time.time - failedCandidates[candidate] < candidateFailureCooldown))
                    {
                        cycleLog.AppendLine($"Skipping candidate {candidate} (failed less than {candidateFailureCooldown} seconds ago).");
                        conStateMessage = "skipping " + candidate + " cool " + candidateFailureCooldown;
                        continue;
                    }
                    else
                    {
                        failedCandidates.Remove(candidate);
                        conStateMessage = "removed candidate " + candidate;
                    }

                    string resolvedAddress = candidate;
                    try
                    {
                        // Use asynchronous DNS resolution.
                        IPHostEntry hostEntry = await Dns.GetHostEntryAsync(candidate);
                        IPAddress[] ipv4Addresses = hostEntry.AddressList
                            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                            .ToArray();
                        if (ipv4Addresses.Length > 0)
                        {
                            resolvedAddress = ipv4Addresses[0].ToString();
                        }
                        else
                        {
                            cycleLog.AppendLine($"DNS lookup returned no IPv4 for candidate '{candidate}'.");
                            conStateMessage = "no ipv4 for " + candidate;
                            failedCandidates[candidate] = Time.time;
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        cycleLog.AppendLine($"DNS resolution failed for candidate '{candidate}': {ex.Message}");
                        conStateMessage = "dns failed for " + candidate;
                        failedCandidates[candidate] = Time.time;
                        continue;
                    }

                    cycleLog.AppendLine($"Attempting connection to {resolvedAddress}...");
                    conStateMessage = "attempting " + resolvedAddress;

                    try
                    {
                        // Wrap the potentially blocking connection call in Task.Run.
                        var sink = await Task.Run(() =>
                        {
                            return new Nt4Source(appName, resolvedAddress, serverPort);
                        });

                        if (sink.Client.Connected())
                        {
                            ipAddress = resolvedAddress; // Cache the working address.
                            frcDataSink = sink;
                            cycleLog.AppendLine($"Connected successfully to {resolvedAddress}.");
                            conStateMessage = "connected to " + resolvedAddress;
                            connectionEstablished = true;
                            connectionAttempt = false;
                            connectionAttemptCompleted = true;
                            break;
                        }
                        else
                        {
                            cycleLog.AppendLine($"Connection attempt to {resolvedAddress} did not succeed.");
                            conStateMessage = "conn failed to " + resolvedAddress;
                        }
                    }
                    catch (Exception ex)
                    {
                        cycleLog.AppendLine($"Connection attempt failed for {resolvedAddress}: {ex.Message}");
                        conStateMessage = "conn failed " + resolvedAddress + " " + ex.Message;
                    }
                }

                // Handle a failed connection
                if (!connectionEstablished)
                {
                    cycleLog.AppendLine($"Could not establish a connection with any candidate addresses. Reattempting in {reconnectDelay} second(s)...");
                    conStateMessage = "no conn to any candidates trying in " + reconnectDelay;
                    QueuedLogger.Log(cycleLog.ToString(), QueuedLogger.LogLevel.Warning);
                    await Task.Delay((int)(reconnectDelay * 1000));
                    reconnectDelay = Mathf.Min(reconnectDelay * 2, maxReconnectDelay);
                }
            }

            // Reset delay on success.
            reconnectDelay = defaultReconnectDelay;
            QueuedLogger.Log("[QuestNav] Connection established. Publishing topics.");
            conStateMessage = "connected - publishing";
            PublishTopics();
        }

        /// <summary>
        /// Handles reconnection when connection is lost.
        /// Centralizes disconnection and reconnection logic.
        /// </summary>
        private void HandleDisconnectedState()
        {
            // Prevent multiple reconnection attempts
            if (connectionAttempt)
            {
                return;
            }
            
            QueuedLogger.Log("[QuestNav] Robot disconnected. Resetting connection and attempting to reconnect...");
            conStateMessage = "robot disconnected - retrying";
            
            // Safely disconnect if we have a connection
            if (frcDataSink != null)
            {
                try
                {
                    frcDataSink.Client.Disconnect();
                }
                catch (Exception ex)
                {
                    QueuedLogger.LogWarning($"[QuestNav] Error disconnecting: {ex.Message}");
                }
                frcDataSink = null;
            }
            
            // Start a fresh connection attempt
            ConnectToRobot();
        }

        /// <summary>
        /// Publishes and subscribes to required NetworkTables topics
        /// Includes heartbeat topics for connection monitoring
        /// </summary>
        private void PublishTopics()
        {
            // Standard QuestNav topics
            frcDataSink.PublishTopic("/questnav/miso", "int");
            frcDataSink.PublishTopic("/questnav/frameCount", "int");
            frcDataSink.PublishTopic("/questnav/timestamp", "double");
            frcDataSink.PublishTopic("/questnav/position", "float[]");
            frcDataSink.PublishTopic("/questnav/quaternion", "float[]");
            frcDataSink.PublishTopic("/questnav/eulerAngles", "float[]");
            frcDataSink.PublishTopic("/questnav/batteryPercent", "double");
            frcDataSink.Subscribe("/questnav/mosi", 0.1, false, false, false);
            frcDataSink.Subscribe("/questnav/init/position", 0.1, false, false, false);
            frcDataSink.Subscribe("/questnav/init/eulerAngles", 0.1, false, false, false);
            frcDataSink.Subscribe("/questnav/resetpose", 0.1, false, false, false);
            
            // Heartbeat system topics
            frcDataSink.PublishTopic("/questnav/heartbeat/quest_to_robot", "double");
            frcDataSink.Subscribe("/questnav/heartbeat/robot_to_quest", 0.1, false, false, false);
            
            // Reset heartbeat state when establishing a new connection
            heartbeatCounter = 1;
            heartbeatResponsePending = false;
            consecutiveFailedHeartbeats = 0;
            lastHeartbeatSentTime = Time.time;
        }
        #endregion

        #region Data Publishing Methods
        /// <summary>
        /// Publishes current frame data to NetworkTables
        /// </summary>
        private void PublishFrameData()
        {
            frameIndex = Time.frameCount;
            timeStamp = Time.time;
            position = cameraRig.centerEyeAnchor.position;
            rotation = cameraRig.centerEyeAnchor.rotation;
            eulerAngles = cameraRig.centerEyeAnchor.eulerAngles;
            batteryPercent = SystemInfo.batteryLevel * 100;

            frcDataSink.PublishValue("/questnav/frameCount", frameIndex);
            frcDataSink.PublishValue("/questnav/timestamp", timeStamp);
            frcDataSink.PublishValue("/questnav/position", position.ToArray());
            frcDataSink.PublishValue("/questnav/quaternion", rotation.ToArray());
            frcDataSink.PublishValue("/questnav/eulerAngles", eulerAngles.ToArray());
            frcDataSink.PublishValue("/questnav/batteryPercent", batteryPercent);
        }
        #endregion

        #region Command Processing Methods
        /// <summary>
        /// Processes commands received from the robot
        /// </summary>
        private void ProcessCommands()
        {
            command = frcDataSink.GetLong("/questnav/mosi");

            if (resetInProgress && command == 0)
            {
                resetInProgress = false;
                QueuedLogger.Log("[QuestNav] Reset operation completed");
                return;
            }

            switch (command)
            {
                case 1:
                    if (!resetInProgress)
                    {
                        QueuedLogger.Log("[QuestNav] Received heading reset request, initiating recenter...");
                        RecenterPlayer();
                        resetInProgress = true;
                    }
                    break;
                case 2:
                    if (!resetInProgress)
                    {
                        QueuedLogger.Log("[QuestNav] Received pose reset request, initiating reset...");
                        InitiatePoseReset();
                        QueuedLogger.Log("[QuestNav] Processing pose reset request.");
                        resetInProgress = true;
                    }
                    break;
                case 3:
                    QueuedLogger.Log("[QuestNav] Ping received, responding...");
                    frcDataSink.PublishValue("/questnav/miso", 97);  // 97 for ping response
                    break;
                default:
                    if (!resetInProgress)
                    {
                        frcDataSink.PublishValue("/questnav/miso", 0);
                    }
                    break;
            }
        }

        /// <summary>
        /// Initiates a pose reset based on received NetworkTables data
        /// </summary>
        private void InitiatePoseReset()
        {
            try {
                // Constants for retry logic and field dimensions
                const int maxRetries = 3;              // Maximum number of attempts to read pose data
                const float retryDelayMs = 50f;        // Delay between retry attempts
                const float FIELD_LENGTH = 16.54f;     // FRC field length in meters
                const float FIELD_WIDTH = 8.02f;       // FRC field width in meters

                double[] resetPose = null;
                bool success = false;
                int attemptCount = 0;

                // Attempt to read pose data from NetworkTables with retry logic
                for (int i = 0; i < maxRetries && !success; i++)
                {
                    attemptCount++;
                    // Add delay between retries to allow for network latency
                    if (i > 0) {
                        System.Threading.Thread.Sleep((int)retryDelayMs);
                        QueuedLogger.Log($"[QuestNav] Attempt {attemptCount} of {maxRetries}...");
                    }

                    QueuedLogger.Log($"[QuestNav] Reading NetworkTables Values (Attempt {attemptCount}):");

                    // Read the pose array from NetworkTables
                    // Format: [X, Y, Rotation] in FRC field coordinates
                    resetPose = frcDataSink.GetDoubleArray("/questnav/resetpose");

                    // Validate pose data format and field boundaries
                    if (resetPose != null && resetPose.Length == 3) {
                        // Check if pose is within valid field boundaries
                        if (resetPose[0] < 0 || resetPose[0] > FIELD_LENGTH ||
                            resetPose[1] < 0 || resetPose[1] > FIELD_WIDTH) {
                            QueuedLogger.LogWarning($"[QuestNav] Reset pose outside field boundaries: X:{resetPose[0]:F3} Y:{resetPose[1]:F3}");
                            continue;
                        }
                        success = true;
                        QueuedLogger.Log($"[QuestNav] Successfully read reset pose values on attempt {attemptCount}");
                    }

                    QueuedLogger.Log($"[QuestNav] Values (Attempt {attemptCount}): X:{resetPose?[0]:F3} Y:{resetPose?[1]:F3} Rot:{resetPose?[2]:F3}");
                }

                // Exit if we couldn't get valid pose data
                if (!success) {
                    QueuedLogger.LogWarning($"[QuestNav] Failed to read valid reset pose values after {attemptCount} attempts");
                    return;
                }

                // Extract pose components from the array
                double resetX = resetPose[0];          // FRC X coordinate (along length of field)
                double resetY = resetPose[1];          // FRC Y coordinate (along width of field)
                double resetRotation = resetPose[2];   // FRC rotation (CCW positive)

                // Normalize rotation to -180 to 180 degrees range
                while (resetRotation > 180) resetRotation -= 360;
                while (resetRotation < -180) resetRotation += 360;

                QueuedLogger.Log($"[QuestNav] Starting pose reset - Target: FRC X:{resetX:F2} Y:{resetY:F2} Rot:{resetRotation:F2}°");

                // Store current VR camera state for reference
                Vector3 currentCameraPos = vrCamera.position;
                Quaternion currentCameraRot = vrCamera.rotation;

                QueuedLogger.Log($"[QuestNav] Before reset - Camera Pos:{currentCameraPos:F3} Rot:{currentCameraRot.eulerAngles:F3}");

                // Convert FRC coordinates to Unity coordinates:
                // Unity uses a left-handed coordinate system with Y as the vertical axis (aligned with gravity)
                // FRC uses a right-handed coordinate system with Z as the vertical axis (aligned with gravity)
                // See this page for more Unity details: https://docs.unity3d.com/Manual/QuaternionAndEulerRotationsInUnity.html
                // See this page for more FRC details: https://docs.wpilib.org/en/stable/docs/software/basic-programming/coordinate-system.html

                // - Unity X = -FRC Y (right is positive in Unity)
                // - Unity Y = kept unchanged (height)
                // - Unity Z = FRC X (forward is positive in both)
                Vector3 targetUnityPosition = new Vector3(
                    (float)(-resetY),         // Negate Y for Unity's coordinate system
                    currentCameraPos.y,       // Maintain current height
                    (float)resetX            // FRC X maps directly to Unity Z
                );

                // Calculate the position difference we need to move
                Vector3 positionDelta = targetUnityPosition - currentCameraPos;

                // Convert FRC rotation to Unity rotation:
                // - Unity uses clockwise positive rotation around Y
                // - FRC uses counterclockwise positive rotation
                float targetUnityYaw = (float)(-resetRotation);  // Negate for Unity's rotation direction
                float currentYaw = currentCameraRot.eulerAngles.y;

                // Normalize both angles to 0-360 range for proper delta calculation
                while (currentYaw < 0) currentYaw += 360;
                while (targetUnityYaw < 0) targetUnityYaw += 360;

                // Calculate rotation delta and normalize to -180 to 180 range
                float yawDelta = targetUnityYaw - currentYaw;
                if (yawDelta > 180) yawDelta -= 360;
                if (yawDelta < -180) yawDelta += 360;

                QueuedLogger.Log($"[QuestNav] Calculated adjustments - Position delta:{positionDelta:F3} Rotation delta:{yawDelta:F3}°");
                QueuedLogger.Log($"[QuestNav] Target Unity Position: {targetUnityPosition:F3}");

                // Store the original offset between camera and its root
                // This helps maintain proper VR tracking space
                Vector3 cameraOffset = vrCamera.position - vrCameraRoot.position;

                // First rotate the root around the camera position
                // This maintains the camera's position while changing orientation
                vrCameraRoot.RotateAround(vrCamera.position, Vector3.up, yawDelta);

                // Then apply the position adjustment to achieve target position
                vrCameraRoot.position += positionDelta;

                // Log final position and rotation for verification
                QueuedLogger.Log($"[QuestNav] After reset - Camera Pos:{vrCamera.position:F3} Rot:{vrCamera.rotation.eulerAngles:F3}");

                // Calculate and check position error to ensure accuracy
                float posError = Vector3.Distance(vrCamera.position, targetUnityPosition);
                QueuedLogger.Log($"[QuestNav] Position error after reset: {posError:F3}m");

                // Warn if position error is larger than expected threshold
                if (posError > 0.01f) {  // 1cm threshold
                    QueuedLogger.LogWarning($"[QuestNav] Large position error detected!");
                }

                frcDataSink.PublishValue("/questnav/miso", 98);
            }
            catch (Exception e) {
                QueuedLogger.LogError($"[QuestNav] Error during pose reset: {e.Message}");
                QueuedLogger.LogException(e);
                frcDataSink.PublishValue("/questnav/miso", 0);
                resetInProgress = false;
            }
        }

        /// <summary>
        /// Recenters the player's view by adjusting the VR camera root transform to match the reset transform.
        /// Similar to the effect of long-pressing the Oculus button.
        /// </summary>
        void RecenterPlayer()
        {
            try {
                float rotationAngleY = vrCamera.rotation.eulerAngles.y - resetTransform.rotation.eulerAngles.y;

                vrCameraRoot.transform.Rotate(0, -rotationAngleY, 0);

                Vector3 distanceDiff = resetTransform.position - vrCamera.position;
                vrCameraRoot.transform.position += distanceDiff;

                frcDataSink.PublishValue("/questnav/miso", 99);
            }
            catch (Exception e) {
                QueuedLogger.LogError($"[QuestNav] Error during pose reset: {e.Message}");
                QueuedLogger.LogException(e);
                frcDataSink.PublishValue("/questnav/miso", 0);
                resetInProgress = false;
            }
        }
        #endregion

        #region UI Methods
        /// <summary>
        /// Updates the team number based on user input and triggers an asynchronous connection reset.
        /// </summary>
        public void UpdateTeamNumber()
        {
            QueuedLogger.Log("[QuestNav] Updating Team Number");
            teamNumber = teamInput.text;
            PlayerPrefs.SetString("TeamNumber", teamNumber);
            PlayerPrefs.Save();
            setInputBox(teamNumber);

            // Clear cached IP and candidate failure data.
            ipAddress = "";
            failedCandidates.Clear();

            // If a connection coroutine is already running, stop it.
            if (_connectionCoroutine != null)
            {
                StopCoroutine(_connectionCoroutine);
                _connectionCoroutine = null;
            }

            // Disconnect existing connection, if any.
            if (frcDataSink != null)
            {
                if (frcDataSink.Client.Connected())
                {
                    frcDataSink.Client.Disconnect();
                }
                frcDataSink = null;
            }

            // Reset connection state and restart connection process
            connectionAttempt = false;
            connectionAttemptCompleted = true;
            
            // Restart the asynchronous connection process.
            ConnectToRobot();
        }

        /// <summary>
        /// Updates the default IP address shown in the UI with the current HMD IP address
        /// </summary>
        /// 
        public void UpdateIPAddressText()
        {
            //Get the local IP
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myAddressLocal = ip.ToString();
                    TextMeshProUGUI ipText = ipAddressText as TextMeshProUGUI;
                    if (myAddressLocal == "127.0.0.1")
                    {
                        ipText.text = "No Adapter Found";
                    }
                    else
                    {
                        ipText.text = myAddressLocal;
                    }
                }
                break;
            }
        }

        public void UpdateConStateText()
        {
            TextMeshProUGUI conText = conStateText as TextMeshProUGUI;
            conText.text = conStateMessage;
        }

        /// <summary>
        /// Updates the input box placeholder text with the current team number
        /// </summary>
        /// <param name="team">The team number to display</param>
        private void setInputBox(string team)
        {
            teamInput.text = "";
            TextMeshProUGUI placeholderText = teamInput.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
            {
                placeholderText.text = "Current: " + team;
            }
            else
            {
                QueuedLogger.LogError("Placeholder is not assigned or not a TextMeshProUGUI component.");
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Generates the IP address for the roboRIO based on team number
        /// </summary>
        /// <returns>The formatted IP address string</returns>
        private string generateIP()
        {
            if (teamNumber.Length >= 1)
            {
                string tePart = teamNumber.Length > 2 ? teamNumber.Substring(0, teamNumber.Length - 2) : "0";
                string amPart = teamNumber.Length > 2 ? teamNumber.Substring(teamNumber.Length - 2) : teamNumber;
                return serverAddress.Replace("TE", tePart).Replace("AM", amPart);
            }
            else
            {
                return "10.0.0.2";
            }
        }

        /// <summary>
        /// Event handler for when the input field is selected
        /// </summary>
        /// <param name="text">The current text in the input field</param>
        private void OnInputFieldSelected(string text)
        {
            QueuedLogger.Log("[QuestNav] Input Selected");
        }
        #endregion
    }
}