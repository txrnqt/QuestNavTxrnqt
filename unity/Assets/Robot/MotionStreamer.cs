using UnityEngine;
using NetworkTables;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using TMPro;
using UnityEngine.UI;
using System;

public static class VectorExtensions
{
    public static float[] ToArray(this Vector3 vector)
    {
        return new float[] { vector.x, vector.y, vector.z };
    }
}

public static class QuaternionExtensions
{
    public static float[] ToArray(this Quaternion quaternion)
    {
        return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
    }
}

public class MotionStreamer : MonoBehaviour
{
    /* Initialize local variables */
    public int frameIndex;
    public double timeStamp;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 eulerAngles;
    public OVRCameraRig cameraRig;
    public Nt4Source frcDataSink = null;
    private long command = 0;
    private bool resetInProgress = false;

    public TMP_InputField teamInput;
    public Button teamUpdateButton;
    public static string inputText = "5152";
    private string teamNumber = "";

    [SerializeField] public Transform vrCamera;
    [SerializeField] public Transform vrCameraRoot;
    [SerializeField] public Transform resetTransform; // The desired position & rotation (look direction) for your player
    private float batteryPercent = 0;

    /* NT configuration settings */
    private readonly string appName = "Quest3S";
    private readonly string serverAddress = "10.TE.AM.2";
    private readonly string serverDNS = "roboRIO-####-FRC.local";
    private string ipAddress = "";
    private bool useAddress = true;
    private readonly int serverPort = 5810;
    private int delayCounter = 0;

    void Start()
    {
        OVRPlugin.systemDisplayFrequency = 120.0f;
        teamNumber = PlayerPrefs.GetString("TeamNumber", "5152");
        setInputBox(teamNumber);
        teamInput.Select();
        ConnectToRobot();
        teamUpdateButton.onClick.AddListener(UpdateTeamNumber);
        teamInput.onSelect.AddListener(OnInputFieldSelected);
    }

    void LateUpdate()
    {
        if (frcDataSink.Client.Connected())
        {
            PublishFrameData();

            if (delayCounter >= 0)
            {
                ProcessCommands();
                delayCounter = 0;
            }
            else
            {
                delayCounter++;
            }
        }
        else
        {
            HandleDisconnectedState();
        }
    }

    private void ConnectToRobot()
    {
        if (useAddress == true)
        {
            ipAddress = getIP();
            useAddress = false;
        }
        else
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(getDNS());
                IPAddress[] ipv4Addresses = hostEntry.AddressList
                   .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                   .ToArray();
                ipAddress = ipv4Addresses[0].ToString();
            }
            catch (Exception ex)
            {
                Debug.Log($"Error resolving DNS name: {ex.Message}");
            }

            useAddress = true;
        }
        Debug.Log("[MotionStreamer] Attempting to connect to the RoboRIO at " + ipAddress + ".");
        frcDataSink = new Nt4Source(appName, ipAddress, serverPort);
        PublishTopics();
    }

    private void HandleDisconnectedState()
    {
        Debug.Log("[MotionStreamer] Robot disconnected. Resetting connection and attempting to reconnect...");
        frcDataSink.Client.Disconnect();
        ConnectToRobot();
    }

    private void PublishTopics()
    {
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
        frcDataSink.Subscribe("/questnav/resetpose", 0.1, false, false, false);  // Single subscription for reset pose array
    }

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

    private void ProcessCommands()
    {
        command = frcDataSink.GetLong("/questnav/mosi");

        if (resetInProgress && command == 0)
        {
            resetInProgress = false;
            Debug.Log("[MotionStreamer] Reset operation completed");
            return;
        }

        switch (command)
        {
            case 1:
                if (!resetInProgress)
                {
                    Debug.Log("[MotionStreamer] Received heading reset request, initiating recenter...");
                    RecenterPlayer();
                    resetInProgress = true;
                }
                break;
            case 2:
                if (!resetInProgress)
                {
                    Debug.Log("[MotionStreamer] Received pose reset request, initiating reset...");
                    InitiatePoseReset();
                    Debug.Log("[MotionStreamer] Processing pose reset request.");
                    resetInProgress = true;
                }
                break;
            case 3:
                Debug.Log("[MotionStreamer] Ping received, responding...");
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

    private void InitiatePoseReset()
    {
        try {
            const int maxRetries = 3;
            const float retryDelayMs = 50f;

            // FRC field boundaries in meters
            const float FIELD_LENGTH = 16.54f;
            const float FIELD_WIDTH = 8.02f;

            double[] resetPose = null;
            bool success = false;
            int attemptCount = 0;

            for (int i = 0; i < maxRetries && !success; i++)
            {
                attemptCount++;
                if (i > 0) {
                    System.Threading.Thread.Sleep((int)retryDelayMs);
                    Debug.Log($"[MotionStreamer] Attempt {attemptCount} of {maxRetries}...");
                }

                Debug.Log($"[MotionStreamer] Reading NetworkTables Values (Attempt {attemptCount}):");

                resetPose = frcDataSink.GetDoubleArray("/questnav/resetpose");

                // Check if we got valid data
                if (resetPose != null && resetPose.Length == 3) {
                    // Validate field boundaries - from (0,0) to (LENGTH, WIDTH)
                    if (resetPose[0] < 0 || resetPose[0] > FIELD_LENGTH ||
                        resetPose[1] < 0 || resetPose[1] > FIELD_WIDTH) {
                        Debug.LogWarning($"[MotionStreamer] Reset pose outside field boundaries: X:{resetPose[0]:F3} Y:{resetPose[1]:F3}");
                        continue;
                    }
                    success = true;
                    Debug.Log($"[MotionStreamer] Successfully read reset pose values on attempt {attemptCount}");
                }

                Debug.Log($"[MotionStreamer] Values (Attempt {attemptCount}): X:{resetPose?[0]:F3} Y:{resetPose?[1]:F3} Rot:{resetPose?[2]:F3}");
            }

            if (!success) {
                Debug.LogWarning($"[MotionStreamer] Failed to read valid reset pose values after {attemptCount} attempts");
                return;
            }

            double resetX = resetPose[0];
            double resetY = resetPose[1];
            double resetRotation = resetPose[2];

            // Normalize rotation to -180 to 180 degrees
            while (resetRotation > 180) resetRotation -= 360;
            while (resetRotation < -180) resetRotation += 360;

            // Also log MOSI value
            long mosi = frcDataSink.GetLong("/questnav/mosi");
            Debug.Log($"[MotionStreamer] mosi: {mosi}");

            Debug.Log($"[MotionStreamer] Starting pose reset - Target: FRC X:{resetX:F2} Y:{resetY:F2} Rot:{resetRotation:F2}°");

            // Store current states for reference
            Vector3 currentCameraPos = vrCamera.position;
            Quaternion currentCameraRot = vrCamera.rotation;

            Debug.Log($"[MotionStreamer] Before reset - Camera Pos:{currentCameraPos:F3} Rot:{currentCameraRot.eulerAngles:F3}");

            // 2. Convert FRC coordinates to Unity coordinates
            Vector3 targetUnityPosition = new Vector3(
                (float)(-resetY),        // Unity X = -FRC Y (right is positive in Unity)
                currentCameraPos.y,      // Maintain current height
                (float)resetX           // Unity Z = FRC X (forward is positive in both)
            );

            // 3. Calculate deltas in world space
            Vector3 positionDelta = targetUnityPosition - currentCameraPos;

            // 4. Handle rotation
            float targetUnityYaw = (float)(-resetRotation);  // Negate because Unity is clockwise
            float currentYaw = currentCameraRot.eulerAngles.y;

            // Normalize yaw angles
            while (currentYaw < 0) currentYaw += 360;
            while (targetUnityYaw < 0) targetUnityYaw += 360;

            float yawDelta = targetUnityYaw - currentYaw;
            if (yawDelta > 180) yawDelta -= 360;
            if (yawDelta < -180) yawDelta += 360;

            Debug.Log($"[MotionStreamer] Calculated adjustments - Position delta:{positionDelta:F3} Rotation delta:{yawDelta:F3}°");
            Debug.Log($"[MotionStreamer] Target Unity Position: {targetUnityPosition:F3}");

            // 5. Apply transformations
            // Store original root-to-camera offset
            Vector3 cameraOffset = vrCamera.position - vrCameraRoot.position;

            // First rotate the root
            vrCameraRoot.RotateAround(vrCamera.position, Vector3.up, yawDelta);

            // Then move the root by the required delta
            vrCameraRoot.position += positionDelta;

            // 6. Verify result
            Debug.Log($"[MotionStreamer] After reset - Camera Pos:{vrCamera.position:F3} Rot:{vrCamera.rotation.eulerAngles:F3}");

            // 7. Verify the position error
            float posError = Vector3.Distance(vrCamera.position, targetUnityPosition);
            Debug.Log($"[MotionStreamer] Position error after reset: {posError:F3}m");

            if (posError > 0.01f) {
                Debug.LogWarning($"[MotionStreamer] Large position error detected!");
            }

            // 8. Send success acknowledgment
            frcDataSink.PublishValue("/questnav/miso", 98);
        }
        catch (Exception e) {
            Debug.LogError($"[MotionStreamer] Error during pose reset: {e.Message}");
            Debug.LogException(e);
            frcDataSink.PublishValue("/questnav/miso", 0);
            resetInProgress = false;
        }
    }

    // Transform the HMD's rotation to virtually "zero" the robot position. Similar result as long-pressing the Oculus button.
    void RecenterPlayer()
    {
        try {
            float rotationAngleY = vrCamera.rotation.eulerAngles.y - resetTransform.rotation.eulerAngles.y;

            vrCameraRoot.transform.Rotate(0, -rotationAngleY, 0);

            Vector3 distanceDiff = resetTransform.position - vrCamera.position;
            vrCameraRoot.transform.position += distanceDiff;

            // Send OK value
            frcDataSink.PublishValue("/questnav/miso", 99);
        }
        catch (Exception e) {
        Debug.LogError($"[MotionStreamer] Error during pose reset: {e.Message}");
        Debug.LogException(e);
        frcDataSink.PublishValue("/questnav/miso", 0);
        resetInProgress = false;
    }
    }

    public void UpdateTeamNumber()
    {
        Debug.Log("[MotionStreamer] Updating Team Number");
        teamNumber = teamInput.text;
        PlayerPrefs.SetString("TeamNumber", teamNumber);
        PlayerPrefs.Save();
        setInputBox(teamNumber);
        HandleDisconnectedState();
    }

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
            Debug.LogError("Placeholder is not assigned or not a TextMeshProUGUI component.");
        }
    }

    private string getDNS()
    {
        return serverDNS.Replace("####", teamNumber);
    }

    private string getIP()
    {
        string tePart = teamNumber.Length > 2 ? teamNumber.Substring(0, teamNumber.Length - 2) : "0";
        string amPart = teamNumber.Length > 2 ? teamNumber.Substring(teamNumber.Length - 2) : teamNumber;
        return serverAddress.Replace("TE", tePart).Replace("AM", amPart);
    }

    private void OnInputFieldSelected(string text)
    {
        Debug.Log("[MotionStreamer] Input Selected");
    }
}