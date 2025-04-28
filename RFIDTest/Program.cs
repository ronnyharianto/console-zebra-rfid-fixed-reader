using Symbol.RFID3;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.Text;
using System.Globalization;

internal class Program
{
    private static RFIDReader rfid3 = new();
    private static ReaderManagement readerManagement = new ReaderManagement()
    {
        TraceLevel = TRACE_LEVEL.TRACE_LEVEL_OFF
    };
    private static int readTagCount = 10;
    private static bool isMultipleReadTag = false;
    private static Socket? m_ListeningSocket = null;
    private static void Main(string[] args)
    {
        //ConnectRfidAsClientMode();

        int angka = 0;
        while (angka < 99)
        {
            Console.Clear();
            Console.WriteLine("0. Connect via Server Mode");
            Console.WriteLine("1. Auto Connect via Client Mode");
            Console.WriteLine("2. Reader Capabilities");
            Console.WriteLine("3. Start Read Single Tag (Event: ReadNotify)");
            Console.WriteLine("4. Start Read Multiple Tag (Event: ReadNotify)");
            Console.WriteLine("5. Login Rfid");
            Console.WriteLine("6. LogOut Rfid");
            Console.WriteLine("7. Restart RFID Reader");
            Console.WriteLine("8. Update Firmware");
            Console.WriteLine("9. Buzzer On");
            Console.WriteLine("10. Buzzer Off");
            Console.WriteLine("11. Health Status");
            Console.WriteLine("12. Write EPC");
            Console.WriteLine("13. Antenna Settings");
            Console.WriteLine("14. Change LLRP IP");
            Console.WriteLine("15. Configure Cable Loss Compensation");
            Console.WriteLine("16. Region And Frequency Configuration");
            Console.WriteLine("17. Change Password");
            Console.WriteLine("18. Disconnect");
            Console.WriteLine("99. Exit");

            Console.Write("Select Menu : ");
            bool v = int.TryParse(Console.ReadLine(), out angka);

            if (v)
            {
                Console.Clear();

                if (angka == 0)
                    ConnectRfidAsServerMode();
                else if (angka == 1)
                    AutoConnectRfidAsClientMode();
                else if (angka == 2)
                    ReadingCapabilities();
                else if (angka == 3)
                {
                    isMultipleReadTag = false;
                    ReadTag();
                }
                else if (angka == 4)
                {
                    isMultipleReadTag = true;
                    ReadTag();
                }
                else if (angka == 5)
                {
                    LoginRfid();
                }
                else if (angka == 6)
                {
                    LogoutRfid();
                }
                else if (angka == 7)
                {
                    RestartRfid();
                }
                else if (angka == 8)
                {
                    UpdateSoftware();
                }
                else if (angka ==  9)
                {
                    BuzzerOn();
                }
                else if (angka == 10)
                {
                    BuzzerOff();
                }
                else if (angka == 11)
                {
                    CheckHealthStatus();
                }
                else if (angka == 12)
                {
                    WriteEPC();
                }
                else if (angka == 13)
                {
                    SetAntennaSettings();
                }
                else if (angka == 14)
                {
                    SetClientModeLLRPIp();
                }
                else if (angka == 15)
                {
                    ConfigureCableLossCompensation();
                }
                else if (angka == 16)
                {
                    RegionAndFrequencyConfiguration();
                }
                else if (angka == 17)
                {
                    ChangePassword();
                }
                else if (angka == 18)
                {
                    DisconnectRfid();
                }

            }
        }

        Console.WriteLine("================================Press Enter To Exit================================");
        Console.ReadLine();
    }

    private static void ConnectRfidAsClientMode()
    {
        Console.WriteLine("CONNECT RFID READER CLIENT MODE");
        Console.WriteLine("Initializing Socket...");
        var m_ListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var iplocal = new IPEndPoint(IPAddress.Any, 5084);

        Console.WriteLine("Binding Socket...");
        m_ListeningSocket.Bind(iplocal);
        m_ListeningSocket.Listen(1);
        var m_ReaderSocket = m_ListeningSocket.Accept();
       
        IPEndPoint newIpRemote = m_ReaderSocket.RemoteEndPoint as IPEndPoint ?? iplocal;
        rfid3 = new RFIDReader(newIpRemote.Address.ToString(), (uint)newIpRemote.Port, 0);
        Console.WriteLine("Socket Accepting Connection...");
        rfid3.AcceptConnection(m_ReaderSocket);
        Console.WriteLine("RFID Connected...");

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static void ConnectRfidAsServerMode()
    {
        string hostname = "169.254.111.1";

        rfid3 = new RFIDReader(hostname, 5084, 0);

        rfid3.Connect();
    }
    
    private static void AutoConnectRfidAsClientMode()
    {
        if (!readerManagement.IsLoggedIn)
        {
            Console.WriteLine("Not logged in. Attempting login...");
            LoginRfid();
            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Login failed. Please check credentials and device status.");
                return;
            }
        }
        Console.WriteLine("CONNECT RFID READER CLIENT MODE");
        Console.WriteLine("Initializing Socket...");

        var backendIpAddress = IPAddress.Parse("169.254.162.130");
        var iplocal = new IPEndPoint(backendIpAddress, 5084);
        if (m_ListeningSocket == null)
        {
            m_ListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Binding Socket...");

            m_ListeningSocket.Bind(iplocal);
            m_ListeningSocket.Listen(1);
        }
        
        //koneksi
        readerManagement.LLRPConnection.InitiateFromReader();
        var m_ReaderSocket = m_ListeningSocket.Accept();

        //IPEndPoint newIpRemote = m_ReaderSocket.RemoteEndPoint as IPEndPoint ?? iplocal;
        //rfid3 = new RFIDReader(newIpRemote.Address.ToString(), (uint)newIpRemote.Port, 0);
        rfid3 = new RFIDReader();
        Console.WriteLine("Socket Accepting Connection...");
        rfid3.AcceptConnection(m_ReaderSocket);
        Console.WriteLine("RFID Connected...");

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static void DisconnectRfid()
    {
        //LoginRfid();

        //readerManagement.LLRPConnection.DisconnectFromReader();

        rfid3.Disconnect();
        LogoutRfid();

        //m_ListeningSocket?.Close();
    }
    
    private static void UpdateSoftware()
    {
        Console.WriteLine("Starting software update...");
        if (!readerManagement.IsLoggedIn)
        {
            Console.WriteLine("Not logged in. Attempting login...");
            LoginRfid();

            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Login failed. Please check credentials and device status.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;  
            }
        }
        string updateDirectory = @"C:\path\to\your\update\directory";
        if (!System.IO.Directory.Exists(updateDirectory))
        {
            Console.WriteLine($"Error: Update path '{updateDirectory}' not found.");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            return; 
        }

        try
        {
            SoftwareUpdateInfo updateInfo = new SoftwareUpdateInfo(updateDirectory, null, null);

            Console.WriteLine("Uploading software...");
            readerManagement.SoftwareUpdate.Update(updateInfo); 
            UpdateStatus updateStatus;

            while (true)
            {
                updateStatus = readerManagement.SoftwareUpdate.UpdateStatus;
                Console.WriteLine($"Update progress: {updateStatus.Percentage}%");

                // Exit when update is complete
                if (updateStatus.Percentage == 100)
                {
                    Console.WriteLine("Software update successful.");
                    break;
                }
                System.Threading.Thread.Sleep(1000); // Wait for 1 second
            }
        }
        catch (OperationFailureException ex)
        {
            Console.WriteLine($"Error during software update: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error during software update: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            return;
        }

        try
        {
            // Restart the reader after the update process
            Console.WriteLine("Restarting the reader...");
            readerManagement.Restart();
            Console.WriteLine("Reader restarted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restarting the reader: {ex.Message}");
        }

        Console.WriteLine("Software update process completed.");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
    
    private static void LoginRfid()
    {
        LoginInfo loginInfo = new LoginInfo()
        {
            HostName = "169.254.111.1",//rfid3.HostName,
            UserName = "admin",
            Password = "Change@1234",
            SecureMode = SECURE_MODE.HTTPS,
            ForceLogin = true,
        };

        try
        {
            // Login ke perangkat RFID
            readerManagement.Login(loginInfo, READER_TYPE.FX);

            if (readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Login succeeded");
                string readerInfo = GetSystemInfo();
                Console.WriteLine(readerInfo);
            }
            else
            {
                Console.WriteLine("Login failed: Invalid login credentials or device error.");
            }
        }
        catch (XmlException ex)
        {
            Console.WriteLine($"XML Parsing Error: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during login: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }
    
    private static void LogoutRfid()
    {
        try
        {
            if (readerManagement != null && readerManagement.IsLoggedIn)
            {
                // Logout dari perangkat RFID
                readerManagement.Logout();

                Console.WriteLine("Logout succeeded.");
            }
            else
            {
                Console.WriteLine("No active session to logout.");
            }
        }
        catch (XmlException ex)
        {
            Console.WriteLine($"XML Parsing Error during logout: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error during logout: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during logout: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static string GetSystemInfo()
    {
        try
        {
            var systemInfo = readerManagement.GetSystemInfo();

            if (systemInfo == null)
            {
                return "Error: System information is null or invalid.";
            }

            var properties = systemInfo.GetType().GetProperties();
            StringBuilder sb = new StringBuilder();

            foreach (var property in properties)
            {
                sb.AppendLine($"{property.Name}: {property.GetValue(systemInfo)}");
            }

            return sb.ToString();
        }
        catch (XmlException ex)
        {
            return $"XML Parsing Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving system info: {ex.Message}";
        }
    }
    
    private static void RestartRfid()
    {
        try
        {
            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Not logged in. Attempting login...");
                LoginRfid();
                if (!readerManagement.IsLoggedIn)
                {
                    Console.WriteLine("Login failed. Please check credentials and device status.");
                    return;
                }
            }

            if (readerManagement.IsLoggedIn)
            {
                try
                {
                    readerManagement.Restart();
                    Console.WriteLine("Reader Restarted");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restart the reader: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Reader is not logged in.");
            }
            Console.WriteLine("================================Press Enter To Continue================================");
            Console.ReadLine();
        }
        catch (OperationFailureException ex)
        {
            throw;
        }
        catch (InvalidUsageException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    private static void ReadingCapabilities()
    {
        Console.WriteLine("FirwareVersion={0}", rfid3.ReaderCapabilities.FirwareVersion);
        Console.WriteLine("ModelName={0}", rfid3.ReaderCapabilities.ModelName);
        Console.WriteLine("NumAntennaSupported={0}", rfid3.ReaderCapabilities.NumAntennaSupported);
        Console.WriteLine("NumGPIPorts={0}", rfid3.ReaderCapabilities.NumGPIPorts);
        Console.WriteLine("NumGPOPorts={0}", rfid3.ReaderCapabilities.NumGPOPorts);
        Console.WriteLine("IsUTCClockSupported= {0}", rfid3.ReaderCapabilities.IsUTCClockSupported);
        Console.WriteLine("IsPeriodicTagReportsSupported= {0}", rfid3.ReaderCapabilities.IsPeriodicTagReportsSupported);
        Console.WriteLine("IsTagPhaseReportingSupported= {0}", rfid3.ReaderCapabilities.IsTagPhaseReportingSupported);
        Console.WriteLine("IsBlockEraseSupported={0}", rfid3.ReaderCapabilities.IsBlockEraseSupported);
        Console.WriteLine("IsBlockPermaLockSupported={0}", rfid3.ReaderCapabilities.IsBlockPermalockSupported);
        Console.WriteLine("IsTagInventoryStateAwareSingulationSupported={0}", rfid3.ReaderCapabilities.IsTagInventoryStateAwareSingulationSupported);
        Console.WriteLine("MaxNumOperationsInAccessSequence={0}", rfid3.ReaderCapabilities.MaxNumOperationsInAccessSequence);
        Console.WriteLine("MaxNumPreFilters={0}", rfid3.ReaderCapabilities.MaxNumPreFilters);
        Console.WriteLine("CommunicationStandard={0}", rfid3.ReaderCapabilities.CommunicationStandard);
        Console.WriteLine("CountryCode={0}", rfid3.ReaderCapabilities.CountryCode);
        Console.WriteLine("IsHoppingEnabled={0}", rfid3.ReaderCapabilities.IsHoppingEnabled);

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static void ReadTag()
    {
        if (isMultipleReadTag)
        {
            bool inputValid = false;
            while (!inputValid)
            {
                Console.Write("Berapa tag yang mau diambil dalam satu waktu: ");
                inputValid = int.TryParse(Console.ReadLine(), out readTagCount);
            }
        }

        try
        {
            Console.WriteLine("Add Event Read Notify...");
            rfid3.Events.ReadNotify += new Events.ReadNotifyHandler(Events_ReadNotify);
            rfid3.Events.AttachTagDataWithReadEvent = !isMultipleReadTag;
            rfid3.Actions.PreFilters.DeleteAll();

            string filterTagPattern = ""; //E2801170
            int filterMaskLength = filterTagPattern.Length / 2;
            if (filterMaskLength > 0)
            {
                byte[] filterMask = new byte[filterMaskLength];

                for (int index = 0; index < filterMaskLength; index++)
                {
                    filterMask[index] = byte.Parse(filterTagPattern.Substring(index * 2, 2), NumberStyles.HexNumber);
                }
                //byte[] filterMask = Enumerable.Range(0, filterMaskLength)
                //                              .Select(i => byte.Parse(filterTagPattern.Substring(i * 3, 3), NumberStyles.HexNumber))
                //                              .ToArray();

                var filter = new PreFilters.PreFilter
                {
                    MemoryBank = MEMORY_BANK.MEMORY_BANK_EPC,
                    BitOffset = 32,
                    TagPattern = filterMask,
                    TagPatternBitCount = (uint)filterMaskLength * 8,
                    //FilterAction = FILTER_ACTION.FILTER_ACTION_DEFAULT,
                };
                //filter.StateUnawareAction.Action = STATE_UNAWARE_ACTION.STATE_UNAWARE_ACTION_UNSELECT;
                rfid3.Actions.PreFilters.Add(filter);
            }

            var triggerInfo = new TriggerInfo();
            triggerInfo.ReportTriggers.Period = 10;

            Console.WriteLine("Start Perform Event");           
            rfid3.Actions.Inventory.Perform(null, triggerInfo, null);

            Console.WriteLine("================================Press Enter To Stop Read Tag================================");
            Console.ReadLine();

            if (rfid3.Actions.TagAccess.OperationSequence.Length > 0)
            {
                Console.WriteLine("Stop Sequence Perform Event");
                rfid3.Actions.TagAccess.OperationSequence.StopSequence();
            }
            else
            {
                Console.WriteLine("Stop Perform Event");
                rfid3.Actions.Inventory.Stop();
            }

            Console.WriteLine("Remove Event Read Notify...");
            rfid3.Events.ReadNotify -= new Events.ReadNotifyHandler(Events_ReadNotify);
        }
        catch (Exception ex)
        { 
            if (ex is OperationFailureException ofex)
            {
                if (ofex.Result == RFIDResults.RFID_COMM_NO_CONNECTION)
                {
                    Console.WriteLine("Connection has been lost. Disconnecting...");
                    rfid3.Disconnect();
                    Console.WriteLine("Disconnected");

                    AutoConnectRfidAsClientMode();
                }
            }
        }
        
        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static void Events_ReadNotify(object sender, Events.ReadEventArgs e)
    { 
        //Console.WriteLine("Event Read Notify Running ({0})... at {1}", isMultipleReadTag ? "Multiple" : "Single", DateTime.Now );
        TagData[] myTags = isMultipleReadTag ? rfid3.Actions.GetReadTags(readTagCount) : [e.ReadEventData.TagData];
        if (myTags != null)
        {
            if (isMultipleReadTag) Console.WriteLine("Total Tag Readed => {0}", myTags.Count());
            foreach (var tag in myTags)
            {
                string readerIp = rfid3.HostName;
                //Console.WriteLine("Reader IP => {0}", readerIp);
                Console.WriteLine("Tag Id => {0}", tag.TagID);
                //Console.WriteLine("Tag Seen Count => {0}", tag.TagSeenCount);
                //Console.WriteLine("Antenna Id => {0}", tag.AntennaID);
                //Console.WriteLine("Memory Bank Data Allocated => {0}", tag.MemoryBankDataAllocated);
                //Console.WriteLine("Memory Bank Data Offset => {0}", tag.MemoryBankDataOffset);
                //Console.WriteLine("Tag ID Allocated => {0}", tag.TagIDAllocated);
                //Console.WriteLine("Peak RSSI => {0}", tag.PeakRSSI);
            }
            //Console.WriteLine();
        }
        //else
        //    Console.WriteLine("There is no tag data");

        //Console.WriteLine("End Read Tag => {0}", DateTime.Now);
    }
    
    private static void BuzzerOn()
    {
        try
        {
            int buzzerGPO = 1;

            Console.WriteLine($"Activating buzzer on GPO port {buzzerGPO}...");

            SetGPOState(buzzerGPO, GPOs.GPO_PORT_STATE.TRUE);
        }
        catch (OperationFailureException ex)
        {
            Console.WriteLine($"Operation Failure: {ex.Result} - {ex.Message}");
        }
        catch (InvalidUsageException ex)
        {
            Console.WriteLine($"Invalid Usage: {ex.Info} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error controlling the buzzer: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }
    
    private static void BuzzerOff()
    {
        try
        {
            int buzzerGPO = 1;

            Console.WriteLine($"Activating buzzer on GPO port {buzzerGPO}...");

            SetGPOState(buzzerGPO, GPOs.GPO_PORT_STATE.FALSE);
        }
        catch (OperationFailureException ex)
        {
            Console.WriteLine($"Operation Failure: {ex.Result} - {ex.Message}");
        }
        catch (InvalidUsageException ex)
        {
            Console.WriteLine($"Invalid Usage: {ex.Info} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error controlling the buzzer: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static void SetGPOState(int gpoPort, GPOs.GPO_PORT_STATE state)
    {
        try
        {
            Console.WriteLine($"Setting GPO port {gpoPort} to state {state}...");
            rfid3.Config.GPO[gpoPort].PortState = state;
            Console.WriteLine($"GPO port {gpoPort} set to {state}.");
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"Error: GPO port {gpoPort} is out of range. Please check the GPO configuration.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error setting GPO state: {ex.Message}");
        }
    }
    
    private static void CheckHealthStatus()
    {
        try
        {
            LoginRfid();

            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Not logged in. Attempting login...");
                LoginRfid();
                if (!readerManagement.IsLoggedIn)
                {
                    Console.WriteLine("Login failed. Unable to retrieve health status.");
                    return;
                }
            }
            SERVICE_ID serviceID = SERVICE_ID.LLRP_SERVER;
            var healthStatus = readerManagement.GetHealthStatus(serviceID);

            if (healthStatus == null)
            {
                Console.WriteLine("Error: Unable to retrieve health status.");
                return;
            }

            Console.WriteLine("Reader Health Status:");
            Console.WriteLine(healthStatus);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving health status: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }
    
    private static void WriteEPC()
    {
        try
        {
            Console.Write("Masukkan EPC baru yang ingin ditulis: ");
            string newEPC = Console.ReadLine();

            if (string.IsNullOrEmpty(newEPC) || newEPC.Length % 4 != 0)
            {
                Console.WriteLine("EPC harus memiliki panjang kelipatan 4 karakter (HEX).");
                return;
            }

            Console.WriteLine("Mulai membaca tag untuk menulis EPC...");
            TagData[] tags = rfid3.Actions.GetReadTags(1);

            if (tags == null || tags.Length == 0)
            {
                Console.WriteLine("Tidak ada tag yang ditemukan.");
                return;
            }

            var targetTag = tags[0];
            Console.WriteLine($"Tag ditemukan: {targetTag.TagID}");
            TagAccess.WriteAccessParams writeParams = new TagAccess.WriteAccessParams
            {
                AccessPassword = 0x0,
                MemoryBank = MEMORY_BANK.MEMORY_BANK_EPC,
                ByteOffset = 2,
                WriteDataLength = (ushort)(newEPC.Length / 4),
                WriteData = HexStringToByteArray(newEPC)
            };

            rfid3.Actions.TagAccess.WriteWait(targetTag.TagID, writeParams, null);
            Console.WriteLine("EPC berhasil ditulis.");
        }
        catch (OperationFailureException ex)
        {
            Console.WriteLine($"Operasi gagal: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Terjadi kesalahan: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
    
    private static void SetAntennaSettings()
    {
        try
        {
            Console.WriteLine("Mengatur konfigurasi RF antena...");
            int totalAntennas = rfid3.Config.Antennas.Length;
            Console.WriteLine($"Jumlah total antena yang didukung: {totalAntennas}");
            ushort[] availableAntennas = rfid3.Config.Antennas.AvailableAntennas;
            Console.WriteLine($"Antena yang tersedia: {string.Join(", ", availableAntennas)}");

            int connectedAntennas = 0;
            var physicallyConnectedAntennas = new List<int>();

            // Memeriksa status koneksi setiap antena
            foreach (ushort antennaId in availableAntennas)
            {
                Antennas.PhysicalProperties antPhysicalProperties = rfid3.Config.Antennas[antennaId].GetPhysicalProperties();
                bool isAntennaConnected = antPhysicalProperties.IsConnected;

                if (isAntennaConnected)
                {
                    connectedAntennas++;
                    physicallyConnectedAntennas.Add(antennaId);
                    Console.WriteLine($"Antena ID {antennaId} terhubung.");
                }
                else
                {
                    Console.WriteLine($"Antena {antennaId} terputus.");
                }
            }

            Console.WriteLine($"Jumlah antena yang terhubung secara fisik: {connectedAntennas}");

            // Menampilkan konfigurasi Transmit Power dalam dBm
            if (connectedAntennas > 0)
            {
                foreach (int antennaId in physicallyConnectedAntennas)
                {
                    Console.WriteLine($"\nMengatur konfigurasi untuk antena ID: {antennaId}");

                    // Mendapatkan konfigurasi RF untuk antena yang terhubung
                    var antennaProperties = rfid3.Config.Antennas[antennaId];
                    var rfConfig = antennaProperties.GetRfConfig();

                    // Mengonversi Transmit Power Index menjadi dBm
                    double transmitPowerDbm = ConvertTransmitPowerIndexToDbm(rfConfig.TransmitPowerIndex);
                    Console.WriteLine($"Transmit Power (dBm) untuk Antena ID {antennaId}: {transmitPowerDbm}");

                    // Menampilkan nilai power transmisi dan meminta input dari pengguna
                    Console.Write($"Masukkan nilai Transmit Power (dBm) untuk Antena {antennaId} (antara 10.0 dan 20.0 dengan kenaikan 0.1): ");
                    double userTransmitPowerDbm = double.Parse(Console.ReadLine());

                    // Validasi input untuk memastikan nilai berada dalam rentang yang valid
                    if (userTransmitPowerDbm < 10.0 || userTransmitPowerDbm > 20.0)
                    {
                        Console.WriteLine("Nilai Transmit Power harus berada antara 10.0 dan 20.0 dBm.");
                        continue; // Lewati antena ini dan lanjutkan ke antena berikutnya
                    }

                    rfConfig.TransmitPowerIndex = ConvertDbmToTransmitPowerIndex(userTransmitPowerDbm);

                    // Menampilkan Receive Sensitivity Index dan meminta input dari pengguna
                    Console.WriteLine($"Receive Sensitivity Index saat ini untuk Antena {antennaId}: {rfConfig.ReceiveSensitivityIndex}");
                    Console.Write($"Masukkan nilai Receive Sensitivity Index untuk Antena {antennaId}: ");
                    rfConfig.ReceiveSensitivityIndex = ushort.Parse(Console.ReadLine());

                    // Menyimpan konfigurasi yang diperbarui
                    antennaProperties.SetRfConfig(rfConfig);
                    Console.WriteLine($"Konfigurasi untuk Antena ID {antennaId} berhasil diperbarui.");
                }
            }
            else
            {
                Console.WriteLine("Tidak ada antena yang terhubung secara fisik.");
            }

            Console.WriteLine("================================Press Enter To Continue================================");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kesalahan saat mengatur RF Config: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    // Mengonversi Transmit Power Index menjadi nilai dBm dengan kenaikan 0.1 dBm
    private static double ConvertTransmitPowerIndexToDbm(ushort transmitPowerIndex)
    {
        // Rentang Transmit Power (dBm) antara 10.0 hingga 20.0 dengan kenaikan 0.1
        double minDbm = 10.0;
        double stepDbm = 0.1;
        return minDbm + transmitPowerIndex * stepDbm;
    }

    private static ushort ConvertDbmToTransmitPowerIndex(double dbm)
    {
        double minDbm = 10.0;
        double stepDbm = 0.1;
        ushort index = (ushort)((dbm - minDbm) / stepDbm);

        // Pastikan indeks berada dalam rentang yang valid
        if (index < 0) index = 0;
        if (index > 100) index = 100; // Asumsi ada maksimal 100 index (hingga 20.0 dBm)

        return index;
    }

    private static void SetClientModeLLRPIp()
    {
        try
        {
            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Not logged in. Attempting login...");
                LoginRfid();
                if (!readerManagement.IsLoggedIn)
                {
                    Console.WriteLine("Login failed. Unable to retrieve health status.");
                    return;
                }
            }

            LLRPConnectionConfig config = readerManagement.LLRPConnection.Config;
            Console.WriteLine("Get LLRP Connection");

            Console.WriteLine("IsClient         : " + config.IsClient);

            Console.WriteLine("Port             : " + config.Port);

            Console.WriteLine("Host Server IP   : " + config.HostServerIP);

            config.HostServerIP = "169.254.162.130";
            config.IsClient = true;

            readerManagement.LLRPConnection.Config = config;

            Console.WriteLine("Mode LLRP client berhasil diatur.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting LLRP client mode: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }
    
    private static void ConfigureCableLossCompensation()
    {
        try
        {
            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Not logged in. Attempting login...");
                LoginRfid();
                if (!readerManagement.IsLoggedIn)
                {
                    Console.WriteLine("Login failed. Unable to retrieve health status.");
                    return;
                }
            }
            Console.Write("Enter Antenna ID to get existing Cable Loss Compensation: ");
            ushort[] antennaIds = { 1, 2, 3, 4, 5, 6, 7, 8 };
            foreach (var antennaId in antennaIds)
            {
                try
                {
                    CableLossCompensation cableLossCompensation = readerManagement.GetCableLossCompensation(antennaId);
                    Console.WriteLine($"Antenna ID: {cableLossCompensation.AntennaID}, " +
                                      $"Cable Length (ft): {cableLossCompensation.CableLenghInFeet}, " +
                                      $"Cable Loss per 100ft: {cableLossCompensation.CableLossPer100Feet}");
                }
                catch (OperationFailureException exception)
                {
                    Console.WriteLine($"Failed to get Cable Loss Compensation for Antenna ID {antennaId}: " + exception.ToString());
                }
            }
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid Antenna ID. Please enter a numeric value.");
        }
        catch (OperationFailureException exception)
        {
            Console.WriteLine("GetCableLossCompensation failed: " + exception.ToString());
        }

        try
        {
            CableLossCompensation[] cableLossArray = new CableLossCompensation[2];
            for (int i = 0; i < cableLossArray.Length; i++)
            {
                cableLossArray[i] = new CableLossCompensation();
                Console.WriteLine($"Enter data for Antenna {i + 1}:");

                Console.Write($"Cable Length in Feet for Antenna {i + 1}: ");
                cableLossArray[i].CableLenghInFeet = float.Parse(Console.ReadLine());

                Console.Write($"Cable Loss per 100 Feet for Antenna {i + 1}: ");
                cableLossArray[i].CableLossPer100Feet = float.Parse(Console.ReadLine());

                cableLossArray[i].AntennaID = (ushort)(i + 1); 
            }

            readerManagement.SetCableLossCompensation(cableLossArray);
            Console.WriteLine("Cable Loss Compensation successfully configured.");
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid input. Please enter numeric values for cable length and loss.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("SetCableLossCompensation failed: " + ex.ToString());
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }
    
    private static void RegionAndFrequencyConfiguration()
    {
        Console.WriteLine("Region setting test on Fixed Reader");

        if (!readerManagement.IsLoggedIn)
        {
            Console.WriteLine("Not logged in. Attempting login...");
            LoginRfid();
            if (!readerManagement.IsLoggedIn)
            {
                Console.WriteLine("Login failed. Unable to retrieve health status.");
                return;
            }
        }

        try
        {
            string[] regionList = readerManagement.ReaderRegion.GetSupportedRegionList();
            var regionselect = readerManagement.ReaderRegion.GetActiveRegion();
            ReaderRegion.ActiveRegionInfo activeRegion = readerManagement.ReaderRegion.GetActiveRegion();
            Console.WriteLine($"Active Region: {activeRegion.RegionName}, Standard: {activeRegion.StandardName}");

            if (regionList.Length == 0)
            {
                Console.WriteLine("No regions available.");
                return;
            }

            Console.WriteLine("Available Regions:");
            for (int i = 0; i < regionList.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {regionList[i]}");
            }

            Console.Write("Select a region by number: ");
            if (!int.TryParse(Console.ReadLine(), out int regionIndex) || regionIndex < 1 || regionIndex > regionList.Length)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            string selectedRegion = regionList[regionIndex - 1];
            Console.WriteLine($"Selected Region: {selectedRegion}");

            ReaderRegion.CommunicationStandardInfo[] standardInfoList = readerManagement.ReaderRegion.GetRegionStandardList(selectedRegion);
            if (standardInfoList.Length == 0)
            {
                Console.WriteLine("No standards available for the selected region.");
                return;
            }

            Console.WriteLine("Available Standards:");
            for (int i = 0; i < standardInfoList.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {standardInfoList[i].StandardName}");
            }

            Console.Write("Select a standard by number: ");
            if (!int.TryParse(Console.ReadLine(), out int standardIndex) || standardIndex < 1 || standardIndex > standardInfoList.Length)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            ReaderRegion.CommunicationStandardInfo selectedStandard = standardInfoList[standardIndex - 1];
            Console.WriteLine($"Selected Standard: {selectedStandard.StandardName}");

            // Set active region and standard
            readerManagement.ReaderRegion.SetActiveRegion(selectedRegion, selectedStandard.StandardName);
            Console.WriteLine($"SetActiveRegion ({selectedRegion}, {selectedStandard.StandardName}) successful");

            // Configure frequency hopping if applicable
            if (selectedStandard.IsHoppingConfigurable)
            {
                Console.WriteLine("Frequency hopping is configurable for this standard.");
                int[] channelList = selectedStandard.ChannelIndexList;
                readerManagement.ReaderRegion.SetFrequencySetting(true, channelList);
                Console.WriteLine("Frequency hopping enabled.");
            }
            else
            {
                Console.WriteLine("Frequency hopping is not configurable for this standard.");
            }

            // Get active region and standard info
         
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Region setting test failed: {ex.Message}");
        }

        Console.WriteLine("================================Press Enter To Continue================================");
        Console.ReadLine();
    }

    private static void ChangePassword()
    {
        Console.Write("Input Password Lama (Admin) : ");
        var oldPassword = Console.ReadLine();

        Console.Write("Input Password Baru (Admin) : ");
        var newPassword = Console.ReadLine();

        var userInfo = new UserInfo
        {
            Username = "admin",
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        LoginRfid();
        readerManagement.ChangePassword(userInfo);
    }
}
