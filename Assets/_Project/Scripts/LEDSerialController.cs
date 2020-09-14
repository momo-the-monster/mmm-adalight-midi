using UnityEngine;
using RJCP.IO.Ports;
using SerialStrings = Helios.Settings.Strings.Serial;

namespace MMM.LED
{
    public class LEDSerialController : MonoBehaviour
    {
        byte[] buffer;
        SerialPortStream serialPort;
        
        private string _port;
        [SerializeField] private int ledCount = 140;
        [SerializeField] private int baudRate = 500000;
        
        void Start()
        {
            int portInt = Helios.Settings.Group.Get(SerialStrings.GroupName).Get<int>(SerialStrings.ComPort);
            _port = ($"COM{portInt}");

            serialPort = new SerialPortStream(_port, baudRate);
            serialPort.Open();

            PrepBuffer();
            SendSingleColor(Color.black);
        }
        
        private void PrepBuffer()
        {
            buffer = new byte[6 + ledCount * 3];
            buffer[0] = (byte)'A';
            buffer[1] = (byte)'d';
            buffer[2] = (byte)'a';
            buffer[3] = (byte)((ledCount - 1) >> 8);            // LED count high byte
            buffer[4] = (byte)((ledCount - 1) & 0xff);          // LED count low byte
            buffer[5] = (byte)(buffer[3] ^ buffer[4] ^ 0x55); // Checksum
        }

        public void Send()
        {
            serialPort.Write(buffer, 0, buffer.Length);
        }

        public void SetSingleLight(int index, Color32 c)
        {
            int offset = index * 3 + 6;
            buffer[offset] = c.r;
            buffer[offset + 1] = c.g;
            buffer[offset + 2] = c.b;
        }
        
        public void SendSingleColor(Color32 c)
        {
            for (int i = 6; i < buffer.Length; i += 0)
            {
                buffer[i++] = c.r;
                buffer[i++] = c.g;
                buffer[i++] = c.b;
            }

            try
            {
                serialPort.Write(buffer, 0, buffer.Length);
            }
            catch (System.IO.IOException e)
            {
                Debug.LogError($"{e.Message}");
                throw;
            }
        }
        
        private void OnDestroy()
        {
            //tweener.Kill(false);
            SendSingleColor(Color.black);
            serialPort.Close();
        }
    }

}
